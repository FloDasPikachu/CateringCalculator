let observer = null;

export function observeHeader(element, dotNetHelper) {
    const options = {
        root: null,
        rootMargin: '-220px 0px 0px 0px', 
        threshold: 0 
    };

    observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            dotNetHelper.invokeMethodAsync('TargetVisibilityChanged', entry.isIntersecting);
        });
    }, options);

    if (element) {
        observer.observe(element);
    }
}

export function unobserveHeader() {
    if (observer) {
        observer.disconnect();
    }
}