using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public class CutsceneSlide
    {
        public Texture2D Texture;
        public string Text;
        public float Duration;
        public float ZoomStart;
        public float ZoomEnd;
        public Vector2 PanStart;
        public Vector2 PanEnd;

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