function gameLoop(timeStamp) {
    window.requestAnimationFrame(gameLoop);
    game.instance.invokeMethodAsync('GameLoop', timeStamp,
        game.canvas.width, game.canvas.height);
}

window.initGame = (instance) => {
    var canvasContainer = document.getElementById('canvasContainer'),
        canvases = canvasContainer.getElementsByTagName('canvas') || [];
    window.game = {
        instance: instance,
        canvas: canvases.length ? canvases[0] : null
    };

    window.requestAnimationFrame(gameLoop);
};
