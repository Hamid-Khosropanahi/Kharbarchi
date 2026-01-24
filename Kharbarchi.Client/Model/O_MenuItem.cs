namespace Kharbarchi.Client.Model
{
	public class O_MenuItem
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Url { get; set; }
		public List<O_MenuItem> Children { get; set; } = new List<O_MenuItem>();
		public string Class { get; set; }
	}
}
