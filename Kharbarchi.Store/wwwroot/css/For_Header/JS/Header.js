window.headerSticky = {
    header: null,
    stickyRow: null,
    topBar: null,
    headerHeight: 0,
    scrollListener: null,
    isSticky: false,

    init: function () {
        console.log('🔧 Initializing header sticky...');

        this.header = document.querySelector('.whb-header');
        this.stickyRow = document.querySelector('.whb-sticky-row');
        this.topBar = document.querySelector('.whb-row.whb-top-bar.whb-not-sticky-row');

        if (!this.header) {
            console.warn('❌ Header element not found');
            return;
        }

        if (!this.stickyRow) {
            console.warn('❌ Sticky row not found');
            return;
        }

        if (!this.topBar) {
            console.warn('⚠️ Top bar not found');
        }

        this.headerHeight = this.header.offsetHeight;
        const stickyOffset = 150;
        let ticking = false;

        
            const updateHeader = () => {
                const currentScroll = window.scrollY || document.documentElement.scrollTop || document.body.scrollTop || 0;

                // Rest of your header update logic here...
            

            if (currentScroll > stickyOffset && !this.isSticky) {
                // Scrolled Down - Make Sticky
                this.isSticky = true;
                this.header.classList.add('whb-sticky-on', 'whb-sticked');
                document.body.classList.add('header-sticky-active');
                console.log('✅ Header is now sticky (Top Bar Hidden)');
            } else if (currentScroll <= stickyOffset && this.isSticky) {
                // Scrolled Up - Remove Sticky
                this.isSticky = false;
                this.header.classList.remove('whb-sticky-on', 'whb-sticked');
                document.body.classList.remove('header-sticky-active');
                console.log('✅ Header is now normal (Top Bar Visible)');
            }

            ticking = false;
        };

        const requestTick = () => {
            if (!ticking) {
                window.requestAnimationFrame(updateHeader);
                ticking = true;
            }
        };

        this.scrollListener = requestTick;
        window.addEventListener('scroll', this.scrollListener, { passive: true });

        // Initial check
        updateHeader();

        console.log('✅ Header sticky initialized successfully');
    },

    destroy: function () {
        console.log('🧹 Destroying header sticky...');

        if (this.scrollListener) {
            window.removeEventListener('scroll', this.scrollListener);
        }

        if (this.header) {
            this.header.classList.remove('whb-sticky-on', 'whb-sticked');
            document.body.classList.remove('header-sticky-active');
        }

        this.isSticky = false;
        console.log('✅ Header sticky destroyed');
    }
};
