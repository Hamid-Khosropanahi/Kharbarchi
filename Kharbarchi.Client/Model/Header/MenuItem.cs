namespace Kharbarchi.Client.Model.Header
{
	public class MenuItem
	{
		public int Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Url { get; set; } = string.Empty;
		public List<MenuItem> Children { get; set; } = new();
		public string CssClass { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public string AriaLabel { get; set; } = string.Empty;
	}
}
