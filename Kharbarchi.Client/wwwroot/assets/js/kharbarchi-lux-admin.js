window.kharbarchiLux = window.kharbarchiLux || {};

window.kharbarchiLux.init = function () {
    const savedTheme = localStorage.getItem('khb-theme');
    if (savedTheme === 'dark') document.body.classList.add('khb-dark');
    if (savedTheme === 'light') document.body.classList.remove('khb-dark');

    document.querySelectorAll('[data-khb-theme-toggle]').forEach(btn => {
        if (btn.dataset.bound === '1') return;
        btn.dataset.bound = '1';
        btn.addEventListener('click', () => {
            document.body.classList.toggle('khb-dark');
            localStorage.setItem('khb-theme', document.body.classList.contains('khb-dark') ? 'dark' : 'light');
        });
    });

    document.querySelectorAll('[data-khb-sidebar-toggle]').forEach(btn => {
        if (btn.dataset.bound === '1') return;
        btn.dataset.bound = '1';
        btn.addEventListener('click', () => document.body.classList.toggle('khb-sidebar-open'));
    });

    document.querySelectorAll('.khb-nav-link').forEach(link => {
        const current = location.pathname.toLowerCase().replace(/\/$/, '');
        const href = new URL(link.href, location.origin).pathname.toLowerCase().replace(/\/$/, '');
        link.classList.toggle('khb-active', current === href || (href !== '/local-admin' && current.startsWith(href)));
        if (link.dataset.bound === '1') return;
        link.dataset.bound = '1';
        link.addEventListener('click', () => document.body.classList.remove('khb-sidebar-open'));
    });
};

window.kharbarchiLux.copyToClipboard = async function (text) {
    await navigator.clipboard.writeText(text || '');
};

document.addEventListener('DOMContentLoaded', () => window.kharbarchiLux.init());
