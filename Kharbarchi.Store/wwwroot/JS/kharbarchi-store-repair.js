window.khbStoreRepair = window.khbStoreRepair || {
  init: function () {
    document.documentElement.setAttribute('dir', 'rtl');
    document.querySelectorAll('nav ul, [class*="menu"] ul, [class*="Menu"] ul').forEach(ul => {
      ul.style.listStyle = 'none';
    });
    document.querySelectorAll('img').forEach(img => {
      const src = (img.getAttribute('src') || '').toLowerCase();
      if (src.includes('logo')) {
        img.loading = 'eager';
      }
    });
  }
};
window.addEventListener('DOMContentLoaded', () => window.khbStoreRepair.init());
