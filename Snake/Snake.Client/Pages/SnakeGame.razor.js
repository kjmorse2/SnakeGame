var framecount = 0;
var animationFrameId = 0;
var animating = false;

export function ToggleAnimation( on )
{
    console.log("N: Toggle Animation " + on);

    animating = on;
    if (on)
    {
        window.requestAnimationFrame( AnimationLoopJS );
    }
    else
    {
        window.cancelAnimationFrame(animationFrameId);
    }
}

/**
 * This code tells the C# side to draw the scene.
 * It then "sleeps" and recalls itself X frames a second
 * where X is usually 60, or whatever the browser is optimized
 * for.
 *
 * Additionally, it only calls this method if the windows is
 * showing. If you switch to a different tab, the animation stops.
 *
 * @param {any} timeStamp
 */
function AnimationLoopJS(timeStamp)
{
    framecount++;

    DotNetSide.invokeMethodAsync('Draw', timeStamp);
    if ( animating )
    {
        animationFrameId = window.requestAnimationFrame(AnimationLoopJS);
    }
}

/**
 * This is a resize event handler that allows the screen
 * to be resized and gives C# the opportunity to do something
 * about it (if you wish);
 */
function resizeCanvasToFitWindow()
{
    // OLD CODE:  
    // var holder = document.getElementById('myCanvas');
    // var canvas = holder.querySelector('canvas');
    // if (canvas)
    // {
    //     var sideCar = document.getElementsByClassName('sidebar');
    //     var main = document.getElementsByTagName('main');
    //
    //     //var width = window.innerWidth - sideCar[0].getBoundingClientRect().width;
    //     let width = document.getElementsByTagName('main')[0].offsetWidth;
    //     width = Math.min(width - 100, 1000);
    //     canvas.width = width;
    //
    //     DotNetSide.invokeMethodAsync('ResizeInBlazor', width, width );
    
    // CHAT GPT SUGGESTED CODE: 
    // var holder = document.getElementById('myCanvas');
    // var canvas = holder ? holder.querySelector('canvas') : null;
    // if (canvas) {
    //     // Prefer the main content width; fall back to holder width
    //     const mainEl = document.getElementsByTagName('main')[0];
    //     let width = (mainEl && mainEl.offsetWidth) ? mainEl.offsetWidth : holder.clientWidth;
    //     // account for padding/margins around the canvas container
    //     width = Math.max(0, width - 32);
    //
    //     // Set both the intrinsic drawing buffer size and the CSS size
    //     canvas.width = width;
    //     canvas.height = width; // keep it square like the world
    //     canvas.style.width = width + 'px';
    //     canvas.style.height = width + 'px';
    //
    //     DotNetSide.invokeMethodAsync('ResizeInBlazor', width, width);   // }
    // }
}

/**
 * Setup the JavaScript side so:
 * 1) It knows about the C# side --> DotNetSide
 * 2) It has a resize window handler
 *
 * @param {any} DotNetSide - the C# instance from the razor code.
 */
export function initJS( DotNetSide )
{
    window.DotNetSide = DotNetSide;
    // window.addEventListener("resize", resizeCanvasToFitWindow);
    // resizeCanvasToFitWindow();
}

document.addEventListener('keydown', function (event)
{
    // Optionally log the key for testing
    console.log('Key pressed:', event.key);

    // Call the C# method and pass the key pressed
    // DotNetSide.invokeMethodAsync('HandleKeyPress', event.key);
});

/**
 * Stop the animation when we leave the page.
 */
window.addEventListener("unload", () =>
{
    console.log("Networked JS: leaving page.");
    animating = false;
});
