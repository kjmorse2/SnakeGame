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
 * Setup the JavaScript side so:
 * 1) It knows about the C# side --> DotNetSide
 * 2) It has a resize window handler
 *
 * @param {any} DotNetSide - the C# instance from the razor code.
 */
export function initJS( DotNetSide )
{
    window.DotNetSide = DotNetSide;
}

document.addEventListener('keydown', function (event)
{
    event.preventDefault();
    // Optionally log the key for testing
    console.log('Key pressed:', event.key);

    DotNetSide.invokeMethodAsync('HandleKeyPress', event.key);
});

/**
 * Stop the animation when we leave the page.
 */
window.addEventListener("unload", () =>
{
    console.log("Networked JS: leaving page.");
    animating = false;
});
