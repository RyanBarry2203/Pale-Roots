using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Simple data holder for one frame of a cinematic sequence.
    // CutsceneManager reads these values to render and time each slide.
    public class CutsceneSlide
    {
        // Texture to draw full screen for this slide.
        public Texture2D Texture;

        // Subtitle text shown on the slide.
        public string Text;

        // How long this slide should stay on screen in milliseconds.
        public float Duration;

        // Starting zoom multiplier applied to the texture.
        public float ZoomStart;

        // Ending zoom multiplier applied to the texture.
        public float ZoomEnd;

        // Starting pan offset applied to the texture.
        public Vector2 PanStart;

        // Ending pan offset applied to the texture.
        public Vector2 PanEnd;

        // Initialize all values used by CutsceneManager for pan, zoom, text, and timing.
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