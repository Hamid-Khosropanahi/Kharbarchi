
window.setTheme = function (dark) {
    document.body.classList.toggle("dark", dark);
};




//For Test Page
function initializeStickyMenus() {
    // این تابع می‌تونه توسعه پیدا کنه، ولی برای این مثال فقط چک می‌کنیم که اسکرول کار می‌کنه
    window.addEventListener('scroll', function () {
        const headerHeight = document.querySelector('.main-header')?.offsetHeight || 0;
        const menu1 = document.querySelector('.sticky-menu-1');
        const menu2 = document.querySelector('.sticky-menu-2');

        if (menu1) {
            menu1.style.top = `${Math.max(0, window.scrollY - headerHeight)}px`;
        }

        if (menu2) {
            menu2.style.top = `${Math.max(0, window.scrollY - headerHeight - menu1.offsetHeight)}px`;
        }
    });

};
//For Test Page


//window.headerInit = function () {
//    if (window.jQuery) {
//        console.log("Header Init OK");

//        if (typeof window.WD !== "undefined") {
//            window.WD.init();
//        }

//        if (typeof window.headerBuilder !== "undefined") {
//            window.headerBuilder.init();
//        }
//    } else {
//        console.error("jQuery not loaded");
//    }
//};

//window.toggleBodyClass = function (open) {
//    document.body.classList.toggle("menu-open", open);
//};
//window.registerScroll = () => {
//    window.addEventListener("scroll", () => {
//        DotNet.invokeMethodAsync("Kharbarchi.Client", "OnScroll",
//            window.scrollY > 80);
//    });
//};
