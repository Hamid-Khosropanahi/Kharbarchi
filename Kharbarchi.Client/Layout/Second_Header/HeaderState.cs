namespace Kharbarchi.Client
{
	public class HeaderState
	{
		public bool IsMobileMenuOpen { get; private set; }
		public bool IsSticky { get; private set; }

		public event Action? OnChange;

		public void ToggleMobileMenu()
		{
			//IsMobileMenuOpen = !IsMobileMenuOpen;
			//Notify();
		}

		public void CloseMobileMenu()
		{
			//IsMobileMenuOpen = false;
			//Notify();
		}

		public void SetSticky(bool value)
		{
			//if (IsSticky != value)
			//{
			//	IsSticky = value;
			//	Notify();
			//}
		}

		private void Notify() => OnChange?.Invoke();
	}

}
