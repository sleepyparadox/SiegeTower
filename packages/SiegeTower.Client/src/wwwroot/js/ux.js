window.siegetower = window.siegetower || {};
window.siegetower.ux = window.siegetower.ux || {};

window.siegetower.ux.SetHorizontalScrollable = function (element) {
    element.addEventListener("wheel", e => {
        if (e.deltaY !== 0) {
            e.preventDefault();
            element.scrollLeft += e.deltaY;
        }
    }, { passive: false });
};
