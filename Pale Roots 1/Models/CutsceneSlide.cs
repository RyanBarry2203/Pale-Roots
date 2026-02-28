using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // This class acts as the director's script for a single shot in a cinematic sequence.
    // It holds the image, the subtitles, how long the shot lasts, and the exact camera movements.
    public class CutsceneSlide
    {
        // The raw image file to display on screen.
        public Texture2D Texture;

        // The lore or dialogue text that will be rendered in the subtitle box at the bottom of the screen.
        public string Text;

        // How long (in milliseconds) this specific slide should stay on screen before fading to the next one.
        public float Duration;

        // --- CAMERA MATH ---
        // By explicitly defining a "Start" and "End" point for both zoom and pan, the CutsceneManager 
        // can smoothly transition between these two numbers over the lifespan of the 'Duration' timer.

        // Zoom controls scale. 1.0f is normal size. 1.1f is zoomed in 10%.
        // Setting ZoomStart to 1.1f and ZoomEnd to 1.0f creates a slow, dramatic pull-back effect.
        public float ZoomStart;
        public float ZoomEnd;

        // Pan controls the X/Y offset. Vector2.Zero means perfectly centered. 
        // Setting PanStart to (-50, 0) and PanEnd to (50, 0) slowly slides the camera across the image from left to right.
        public Vector2 PanStart;
        public Vector2 PanEnd;

        // The constructor simply scoops up all this data when the CutsceneLibrary builds the scene,
        // packaging it neatly so the manager can read it frame-by-frame during playback.
        public CutsceneSlide(Texture2D texture, string text, float duration, float zStart, float zEnd, Vector2 pStart, Vector2 pEnd)
        {
            Texture = texture;
            Text = text;
            Duration = duration;
            ZoomStart = zStart;
            ZoomEnd = zEnd;
            PanStart = pStart;
            PanEnd = pEnd;
        }
    }
}