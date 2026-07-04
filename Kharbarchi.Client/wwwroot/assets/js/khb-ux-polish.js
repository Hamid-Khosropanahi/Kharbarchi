window.khbPremiumUi = window.khbPremiumUi || {
  init: function () {
    const theme = localStorage.getItem('khb.theme') || 'light';
    document.documentElement.setAttribute('data-khb-theme', theme);
    document.body.classList.toggle('khb-dark', theme === 'dark');
    this.installThemeButton();
  },
  installThemeButton: function () {
    if (document.querySelector('[data-khb-theme-toggle-auto="true"]')) return;
    const host = document.querySelector('.khb-topbar, .khb-hero-actions, header, body');
    if (!host) return;
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.setAttribute('data-khb-theme-toggle-auto', 'true');
    btn.className = 'khb-btn khb-btn-ghost khb-theme-auto-button';
    btn.innerHTML = 'تغییر تم';
    btn.addEventListener('click', () => this.toggleTheme());
    if (host === document.body) {
      btn.style.position = 'fixed';
      btn.style.left = '18px';
      btn.style.bottom = '18px';
      btn.style.zIndex = '9999';
    }
    host.appendChild(btn);
  },
  toggleTheme: function () {
    const current = document.documentElement.getAttribute('data-khb-theme') || 'light';
    const next = current === 'dark' ? 'light' : 'dark';
    localStorage.setItem('khb.theme', next);
    document.documentElement.setAttribute('data-khb-theme', next);
    document.body.classList.toggle('khb-dark', next === 'dark');
  }
};

window.addEventListener('DOMContentLoaded', () => window.khbPremiumUi.init());
