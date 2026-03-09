/* ============================================================
   Career Rookies - site.js
   ============================================================ */

(function () {
    'use strict';

    /* ---------------------------------------------------------
       COOKIE CONSENT
       --------------------------------------------------------- */
    var COOKIE_KEY = 'cr_cookie_consent';

    function initCookieConsent() {
        var bar = document.querySelector('.cookie-consent-bar');
        if (!bar) return;

        // If already accepted, hide immediately
        if (localStorage.getItem(COOKIE_KEY) === 'accepted') {
            bar.classList.add('hidden');
            return;
        }

        // Show the bar
        bar.classList.remove('hidden');

        var acceptBtn = bar.querySelector('.btn-accept-cookies');
        if (acceptBtn) {
            acceptBtn.addEventListener('click', function () {
                localStorage.setItem(COOKIE_KEY, 'accepted');
                bar.classList.add('hidden');
            });
        }
    }

    /* ---------------------------------------------------------
       SMOOTH SCROLL FOR ANCHOR LINKS
       --------------------------------------------------------- */
    function initSmoothScroll() {
        document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
            anchor.addEventListener('click', function (e) {
                var targetId = this.getAttribute('href');
                if (targetId === '#' || targetId.length < 2) return;

                var target = document.querySelector(targetId);
                if (!target) return;

                e.preventDefault();

                var navbar = document.querySelector('.navbar.fixed-top, .navbar.sticky-top');
                var offset = navbar ? navbar.offsetHeight + 10 : 0;

                var targetPosition = target.getBoundingClientRect().top + window.pageYOffset - offset;

                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });

                // Update URL hash without jumping
                if (history.pushState) {
                    history.pushState(null, null, targetId);
                }
            });
        });
    }

    /* ---------------------------------------------------------
       NAVBAR ACTIVE STATE
       --------------------------------------------------------- */
    function initNavbarActiveState() {
        var currentPath = window.location.pathname.toLowerCase().replace(/\/$/, '');
        var navLinks = document.querySelectorAll('.navbar .nav-link');

        navLinks.forEach(function (link) {
            var href = link.getAttribute('href');
            if (!href) return;

            // Build a comparable path
            var linkPath;
            try {
                var url = new URL(href, window.location.origin);
                linkPath = url.pathname.toLowerCase().replace(/\/$/, '');
            } catch (e) {
                linkPath = href.toLowerCase().replace(/\/$/, '');
            }

            // Remove existing active
            link.classList.remove('active');

            // Exact match or home page
            if (linkPath === currentPath) {
                link.classList.add('active');
            } else if (currentPath !== '' && currentPath !== '/' && linkPath !== '' && linkPath !== '/' && currentPath.startsWith(linkPath)) {
                // Partial match for sub-pages (e.g., /workshops/details matches /workshops)
                link.classList.add('active');
            }
        });
    }

    /* ---------------------------------------------------------
       CAROUSEL INITIALIZATION
       Bootstrap 5 auto-initializes carousels via data attributes.
       This helper ensures cycling options are set if needed.
       --------------------------------------------------------- */
    function initCarousels() {
        var heroCarousel = document.querySelector('#heroCarousel');
        if (heroCarousel && typeof bootstrap !== 'undefined' && bootstrap.Carousel) {
            new bootstrap.Carousel(heroCarousel, {
                interval: 5000,
                ride: 'carousel',
                pause: 'hover',
                wrap: true
            });
        }
    }

    /* ---------------------------------------------------------
       NAVBAR SCROLL SHADOW
       Adds a class when the user scrolls down for enhanced shadow.
       --------------------------------------------------------- */
    function initNavbarScroll() {
        var navbar = document.querySelector('.navbar.fixed-top');
        if (!navbar) return;

        function onScroll() {
            if (window.scrollY > 50) {
                navbar.classList.add('navbar-scrolled');
            } else {
                navbar.classList.remove('navbar-scrolled');
            }
        }

        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }

    /* ---------------------------------------------------------
       ADMIN SIDEBAR TOGGLE (mobile)
       --------------------------------------------------------- */
    function initAdminSidebar() {
        var toggleBtn = document.querySelector('[data-toggle-sidebar]');
        var sidebar = document.querySelector('.admin-sidebar');
        if (!toggleBtn || !sidebar) return;

        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });

        // Close on outside click
        document.addEventListener('click', function (e) {
            if (sidebar.classList.contains('show') &&
                !sidebar.contains(e.target) &&
                !toggleBtn.contains(e.target)) {
                sidebar.classList.remove('show');
            }
        });
    }

    /* ---------------------------------------------------------
       INIT ON DOM READY
       --------------------------------------------------------- */
    document.addEventListener('DOMContentLoaded', function () {
        initCookieConsent();
        initSmoothScroll();
        initNavbarActiveState();
        initCarousels();
        initNavbarScroll();
        initAdminSidebar();
    });

})();
