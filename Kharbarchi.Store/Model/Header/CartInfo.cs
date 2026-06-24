namespace Kharbarchi.Client.Model.Header
{
	public class CartInfo
	{
		public int ItemCount { get; set; }
		public decimal Total { get; set; }
		public string Currency { get; set; } = "تومان";
	}
}
