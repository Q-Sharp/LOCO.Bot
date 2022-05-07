function renderJS(timeStamp) {
    theInstance.invokeMethodAsync('RenderInBlazor', timeStamp);
    window.requestAnimationFrame(renderJS);
}

function resizeCanvasToFitWindow() {
    var page = document.getElementById('pageContainer');
    if (page) {
        page.width = window.innerWidth;
        page.height = window.innerHeight;
        theInstance.invokeMethodAsync('ResizeInBlazor', page.width, page.height);
    }
}

window.initRenderJS = (instance) => {
    window.theInstance = instance;
    window.addEventListener("resize", resizeCanvasToFitWindow);
    resizeCanvasToFitWindow();
    window.requestAnimationFrame(renderJS);
};