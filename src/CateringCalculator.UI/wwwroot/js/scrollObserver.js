let observer = null;

export function observeHeader(element, dotNetHelper) {
    if (!element) return;

    // 1. Dynamisch die Höhe der fixierten Navbar auslesen
    const navbar = document.querySelector('.top-navbar');
    const navbarHeight = navbar ? navbar.offsetHeight : 60; // Fallback auf 60px

    // 2. rootMargin nach oben hin anpassen (-NavbarHöhe)
    // Das lässt den Observer genau an der Unterkante der Navbar "feuern".
    const options = {
        root: null,
        rootMargin: `-${navbarHeight}px 0px 0px 0px`,
        threshold: 0
    };

    observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            // entry.isIntersecting sagt uns, ob der Header noch "unterhalb" der Navbar sichtbar ist
            dotNetHelper.invokeMethodAsync('TargetVisibilityChanged', entry.isIntersecting);
        });
    }, options);

    observer.observe(element);
}

export function unobserveHeader() {
    if (observer) {
        observer.disconnect();
        observer = null;
    }
}