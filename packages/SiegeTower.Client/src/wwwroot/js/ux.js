window.siegetower = window.siegetower || {};
window.siegetower.ux = window.siegetower.ux || {};
window.siegetower.drag = window.siegetower.drag || {};

window.siegetower.drag.mousePosition = null;
window.siegetower.drag.dragPreviewPosition = null;

window.siegetower.drag.UpdatePreview = function (timestamp) {
    const drag = window.siegetower.drag;
    const preview = document.querySelector(".drag-preview");

    if (!preview) {
        drag.dragPreviewPosition = null;
    } else if (drag.mousePosition) {
        if (!drag.dragPreviewPosition) {
            drag.dragPreviewPosition = { ...drag.mousePosition };
        } else {
            const deltaTime = drag.lastFrameTime === undefined
                ? 0
                : (timestamp - drag.lastFrameTime) / 1000;
            const lerpAmount = deltaTime * 10;

            drag.dragPreviewPosition.x += (drag.mousePosition.x - drag.dragPreviewPosition.x) * lerpAmount;
            drag.dragPreviewPosition.y += (drag.mousePosition.y - drag.dragPreviewPosition.y) * lerpAmount;
        }

        preview.style.position = "absolute";
        preview.style.left = `${drag.dragPreviewPosition.x}px`;
        preview.style.top = `${drag.dragPreviewPosition.y}px`;
    }

    drag.lastFrameTime = timestamp;
    requestAnimationFrame(drag.UpdatePreview);
};

window.siegetower.ux.SetHorizontalScrollable = function (element) {
    element.addEventListener("wheel", e => {
        if (e.deltaY !== 0) {
            e.preventDefault();
            element.scrollLeft += e.deltaY;
        }
    }, { passive: false });
};

document.addEventListener("mousemove", e => {
    window.siegetower.drag.mousePosition = {
        x: e.pageX,
        y: e.pageY
    };
});

requestAnimationFrame(window.siegetower.drag.UpdatePreview);
