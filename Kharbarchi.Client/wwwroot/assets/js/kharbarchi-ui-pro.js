window.kharbarchiUi = window.kharbarchiUi || {};
window.kharbarchiUi.enhance = () => {
    document.querySelectorAll('.khb-btn, .khb-mini-btn, .khb-nav-card').forEach((el) => {
        if (el.dataset.khbEnhanced === '1') return;
        el.dataset.khbEnhanced = '1';
        el.addEventListener('pointermove', (event) => {
            const rect = el.getBoundingClientRect();
            el.style.setProperty('--mx', `${event.clientX - rect.left}px`);
            el.style.setProperty('--my', `${event.clientY - rect.top}px`);
        });
    });
};
