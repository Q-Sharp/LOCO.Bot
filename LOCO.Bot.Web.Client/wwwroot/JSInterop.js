function renderJS(timeStamp) {
    theInstance.invokeMethodAsync('RenderInBlazor', timeStamp);
    window.requestAnimationFrame(renderJS);
    resizeToFitWindow();
}

function resizeToFitWindow() {
    var page = document.getElementById('pageContainer');
    if (page) {
        page.width = window.innerWidth;
        page.height = window.innerHeight;
        theInstance.invokeMethodAsync('ResizeInBlazor', page.width, page.height);
    }
}

window.initRenderJS = (instance) => {
    window.theInstance = instance;
    window.addEventListener("resize", resizeToFitWindow);
    resizeToFitWindow();
    window.requestAnimationFrame(renderJS);
};

function ChangeSpin(start) {
    var holder = document.getElementById('holder');
    if (start) {
        holder.style.animationName = "spinr";
        holder.style.animationDuration = "15s";
    }
    else {
        holder.style.animationName = null;
        holder.style.animationDuration = "0s";
    }
    
}
