const scrollHandler = () => {
    window.addEventListener("focusin", (e) => {
        if (e.target.nodeName === "BUTTON" || e.target.nodeName === "A" || e.target.nodeName === "IFRAME") {
            return;
        }
        const inputTop = document.activeElement.getBoundingClientRect().top;
        const relativeHeight = (screen.availHeight / 2) - inputTop;
        if (relativeHeight >= 0) {
            return;
        }
        window.scroll(0, Math.abs(relativeHeight) + window.scrollY);
    });
}
window.addEventListener("load", scrollHandler);