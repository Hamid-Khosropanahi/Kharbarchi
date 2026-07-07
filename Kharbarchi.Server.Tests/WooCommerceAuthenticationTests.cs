using System.Net;
using System.Text;
using Kharbarchi.Server.Infrastructure.Safety;
using Kharbarchi.Server.Options;
using Kharbarchi.Server.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kharbarchi.Server.Tests;

[TestClass]
public sealed class WooCommerceAuthenticationTests
{
    private const string BaseUrl = "https://localhost:4433/Kharbarchi";

    [TestMethod]
    public async Task GetProductsAsync_AttachesBasicConsumerAuthenticationWithoutUrlCredentials()
    {
        const string consumerKey = "test-consumer-کلید";
        const string consumerSecret = "test-consumer-secret";
        var handler = new CapturingHandler(
            () => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });
        var logger = new RecordingLogger<WooCommerceApiClient>();
        using var context = CreateClient(handler, logger, consumerKey, consumerSecret);

        using var result = await context.Client.GetProductsAsync(1, 1, CancellationToken.None);

        var captured = handler.LastRequest
            ?? throw new AssertFailedException("The WooCommerce request was not captured.");
        var expectedParameter = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));

        Assert.AreEqual("Basic", captured.AuthorizationScheme);
        Assert.AreEqual(expectedParameter, captured.AuthorizationParameter);
        Assert.AreEqual(
            "https://localhost:4433/Kharbarchi/wp-json/wc/v3/products?page=1&per_page=1&orderby=id&order=asc&status=any",
            captured.Uri.ToString());
        Assert.IsFalse(captured.Uri.Query.Contains("consumer_key", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(captured.Uri.Query.Contains("consumer_secret", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task GetProductsAsync_SanitizesCredentialsFromErrorsAndLogs()
    {
        const string consumerKey = "test-consumer-key";
        const string consumerSecret = "test-consumer-secret";
        var encodedToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
        var responseBody =
            $"consumer_key={consumerKey}&consumer_secret={consumerSecret} Authorization: Basic {encodedToken}";
        var handler = new CapturingHandler(
            () => new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        var logger = new RecordingLogger<WooCommerceApiClient>();
        using var context = CreateClient(handler, logger, consumerKey, consumerSecret);

        HttpRequestException exception;
        try
        {
            using var unused = await context.Client.GetProductsAsync(1, 1, CancellationToken.None);
            throw new AssertFailedException("An unsuccessful WooCommerce response should throw.");
        }
        catch (HttpRequestException ex)
        {
            exception = ex;
        }

        var combinedOutput = exception.Message + Environment.NewLine + string.Join(Environment.NewLine, logger.Messages);
        Assert.IsFalse(combinedOutput.Contains(consumerKey, StringComparison.Ordinal));
        Assert.IsFalse(combinedOutput.Contains(consumerSecret, StringComparison.Ordinal));
        Assert.IsFalse(combinedOutput.Contains(encodedToken, StringComparison.Ordinal));
        Assert.IsTrue(combinedOutput.Contains("[REDACTED]", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ApplyBasicAuthentication_UsesUtf8ConsumerKeyAndSecret()
    {
        const string consumerKey = "کلید-test";
        const string consumerSecret = "رمز-test";
        using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);

        WooCommerceRequestSecurity.ApplyBasicAuthentication(
            request,
            consumerKey,
            consumerSecret);

        var expected = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{consumerKey}:{consumerSecret}"));
        Assert.AreEqual("Basic", request.Headers.Authorization?.Scheme);
        Assert.AreEqual(expected, request.Headers.Authorization?.Parameter);
    }

    [TestMethod]
    public void RejectCredentialQueryParameters_RejectsLegacyQueryAuthentication()
    {
        InvalidOperationException exception;
        try
        {
            WooCommerceRequestSecurity.RejectCredentialQueryParameters(
                "/wp-json/wc/v3/products?consumer_key=legacy-key&consumer_secret=legacy-secret");
            throw new AssertFailedException("Credential query parameters should be rejected.");
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsTrue(exception.Message.Contains("Authorization header", StringComparison.Ordinal));
    }

    private static TestClientContext CreateClient(
        HttpMessageHandler handler,
        ILogger<WooCommerceApiClient> logger,
        string consumerKey,
        string consumerSecret)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl.TrimEnd('/') + "/")
        };
        var options = Microsoft.Extensions.Options.Options.Create(
            new WooCommerceOptions
            {
                BaseUrl = BaseUrl,
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                EnvironmentType = "Local",
                VerifySsl = false,
                AllowInsecureLocalhostSsl = true,
                TimeoutSeconds = 30
            });
        var guard = new EnvironmentSafetyGuard(
            NullLogger<EnvironmentSafetyGuard>.Instance,
            new TestHostEnvironment());

        return new TestClientContext(
            new WooCommerceApiClient(httpClient, options, logger, guard),
            httpClient);
    }

    private sealed class TestClientContext : IDisposable
    {
        private readonly HttpClient _httpClient;

        public TestClientContext(WooCommerceApiClient client, HttpClient httpClient)
        {
            Client = client;
            _httpClient = httpClient;
        }

        public WooCommerceApiClient Client { get; }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responseFactory;

        public CapturingHandler(Func<HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public CapturedRequest? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = new CapturedRequest(
                request.RequestUri ?? throw new InvalidOperationException("Request URI was not set."),
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter);
            return Task.FromResult(_responseFactory());
        }
    }

    private sealed record CapturedRequest(
        Uri Uri,
        string? AuthorizationScheme,
        string? AuthorizationParameter);

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull =>
            NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Kharbarchi.Server.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
