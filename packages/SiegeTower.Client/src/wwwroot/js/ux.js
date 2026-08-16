window.siegetower = window.siegetower || {};
window.siegetower.ux = window.siegetower.ux || {};
window.siegetower.drag = window.siegetower.drag || {};

window.siegetower.drag.mousePosition = null;
window.siegetower.drag.dragPreviewPosition = null;
window.siegetower.drag.dragType = null;

window.siegetower.drag.DragStart = function (dragType) {
    window.siegetower.drag.dragType = dragType;
    window.siegetower.drag.dragPreviewPosition = null;
};

window.siegetower.drag.DragStop = function () {
    window.siegetower.drag.dragType = null;
    window.siegetower.drag.dragPreviewPosition = null;
};

window.siegetower.drag.UpdateDockWidth = function () {
    const drag = window.siegetower.drag;
    if (!drag.mousePosition) {
        return;
    }

    if (drag.dragType === "DragDockWidthLeft") {
        const width = Math.max(0, drag.mousePosition.clientX);
        const dock = document.querySelector(".ux-drag-dock-width-left");
        if (dock) {
            dock.style.flexBasis = `${width}px`;
            dock.style.width = `${width}px`;
        }
    } else if (drag.dragType === "DragDockWidthRight") {
        const width = Math.max(0, window.innerWidth - drag.mousePosition.clientX);
        const dock = document.querySelector(".ux-drag-dock-width-right");
        if (dock) {
            dock.style.flexBasis = `${width}px`;
            dock.style.width = `${width}px`;
        }
    }
};

window.siegetower.drag.UpdatePreview = function (timestamp) {
    const drag = window.siegetower.drag;
    const preview = document.querySelector(".drag-preview");

    if (drag.dragType !== "Preview") {
        drag.dragPreviewPosition = null;
    } else if (!preview) {
        drag.dragPreviewPosition = null;
    } else if (drag.mousePosition) {
        if (!drag.dragPreviewPosition) {
            drag.dragPreviewPosition = { ...drag.mousePosition };
        } else {
            const deltaTime = drag.lastFrameTime === undefined
                ? 0
                : (timestamp - drag.lastFrameTime) / 1000;
            const lerpAmount = deltaTime * 20;

            drag.dragPreviewPosition.x += (drag.mousePosition.x - drag.dragPreviewPosition.x) * lerpAmount;
            drag.dragPreviewPosition.y += (drag.mousePosition.y - drag.dragPreviewPosition.y) * lerpAmount;
        }

        preview.style.position = "absolute";
        preview.style.left = `${drag.dragPreviewPosition.x}px`;
        preview.style.top = `${drag.dragPreviewPosition.y}px`;
    }

    drag.UpdateDockWidth();

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
        y: e.pageY,
        clientX: e.clientX
    };
});

document.addEventListener("mouseup", () => {
    window.siegetower.drag.DragStop();
});

requestAnimationFrame(window.siegetower.drag.UpdatePreview);
