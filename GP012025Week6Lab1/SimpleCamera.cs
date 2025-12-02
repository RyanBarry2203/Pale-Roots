using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Camera
{
    class SimpleCam
    {
        public float zoom; // Camera Zoom
        public Matrix transform; // Matrix Transform
        public Vector2 pos; // Camera Position
        public float rotation; // Camera Rotation


        public SimpleCam(Viewport v)
        {

            zoom = 1.0f;
            rotation = 0.0f;
            pos = Vector2.Zero;
        }


        public void Zoom(float ZoomAmount)
        {
            if (ZoomAmount < 0.1f) zoom += ZoomAmount; // Negative zoom will flip image
            else zoom += ZoomAmount;
        }

        public void Rotate(float amount)
        {
            rotation += amount;
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            pos += amount;
        }

        // Get set position
        public Vector2 Position()
        {
            return pos;

        }
        public Matrix get_transformation(GraphicsDevice graphicsDevice)
        {
            // Assumes origin of the Camera is in the middle of the screen
            transform =
              Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0)) *
                                         Matrix.CreateRotationZ(rotation) *
                                         Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                                         Matrix.CreateTranslation(new Vector3(0, 0, 0)
                                         //Matrix.CreateTranslation(new Vector3(
                                         //         graphicsDevice.Viewport.Width / 2,
                                         //         graphicsDevice.Viewport.Height / 2, 0)
                                         );


            //);
            return transform;
        }


    }
}
