window.khbPremium = (() => {
  const storageKey = 'khb-theme';
  function applyTheme(theme) {
    const resolved = theme || localStorage.getItem(storageKey) || 'light';
    document.documentElement.setAttribute('data-theme', resolved);
    localStorage.setItem(storageKey, resolved);
  }
  function toggleTheme() {
    const current = document.documentElement.getAttribute('data-theme') || localStorage.getItem(storageKey) || 'light';
    applyTheme(current === 'dark' ? 'light' : 'dark');
  }
  function toggleSidebar() {
    document.body.classList.toggle('khb-sidebar-open');
  }
  function closeSidebar() { document.body.classList.remove('khb-sidebar-open'); }
  document.addEventListener('click', e => {
    if (e.target.closest('.khb-nav-item')) closeSidebar();
  });
  applyTheme();
  return { applyTheme, toggleTheme, toggleSidebar, closeSidebar };
})();
