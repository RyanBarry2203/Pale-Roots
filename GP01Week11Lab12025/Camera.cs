using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CameraNS
{
    class Camera
    {
        Vector2 _camPos = Vector2.Zero;
        Vector2 _worldBound;

        public float Zoom { get; set; } = 1.0f;

        public Matrix CurrentCameraTranslation
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-CamPos, 0)) *
                       Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));
            }
        }

        public Vector2 CamPos
        {
            get { return _camPos; }
            set { _camPos = value; }
        }

        public Camera(Vector2 startPos, Vector2 bound)
        {
            CamPos = startPos;
            _worldBound = bound;
        }

        public void follow(Vector2 followPos, Viewport v)
        {

            Vector2 viewSize = new Vector2(v.Width, v.Height) / Zoom;


            _camPos = followPos - (viewSize / 2);

            _camPos = Vector2.Clamp(_camPos, Vector2.Zero, _worldBound - viewSize);
        }
    }
}