using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#if ANDROID
using Microsoft.Devices.Sensors;
#endif

namespace Pale_Roots_1
{
    // InputEngine: centralizes input polling for keyboard, gamepad, mouse and (optionally) touch/accelerometer.
    // - Added as a GameComponent so it's automatically updated by Game1.
    // - Other systems call static helpers (IsKeyPressed, IsButtonHeld, MousePosition, etc.) instead of reading raw platform APIs.
    // - Keeps previous/current states to detect presses vs holds vs releases.
    public class InputEngine : GameComponent
    {
        // GamePad state tracking
        private static GamePadState previousPadState;
        private static GamePadState currentPadState;

        // Keyboard state tracking
        private static KeyboardState previousKeyState;
        private static KeyboardState currentKeyState;

        // Mouse position and state (Windows only)
        private static Vector2 previousMousePos;
        private static Vector2 currentMousePos;
        private static MouseState previousMouseState;
        private static MouseState currentMouseState;

#if ANDROID
        // Mobile-only input fields: accelerometer and touch gesture info
        private static Vector2 previousAccelerometerReading;
        private static Accelerometer _acceleromter;
        private static Vector2 currentAcceleromoterReading;
        private static Point touchPoint;
        private static GestureType currentGestureType;

        // Accelerometer event handler updates cached readings (orientation assumed landscape here).
        private void _acceleromter_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            previousAccelerometerReading = CurrentAcceleromoterReading;
            currentAcceleromoterReading.Y = -(float)e.SensorReading.Acceleration.Y;
            currentAcceleromoterReading.X = -(float)e.SensorReading.Acceleration.X;
        }
#endif

        // Register the component and initialize current states.
        public InputEngine(Game _game)
            : base(_game)
        {
            currentPadState = GamePad.GetState(PlayerIndex.One);
            currentKeyState = Keyboard.GetState();

#if ANDROID
            _acceleromter = new Accelerometer();
            _acceleromter.CurrentValueChanged += _acceleromter_CurrentValueChanged;
            _acceleromter.Start();
            TouchPanel.EnabledGestures =
                    GestureType.Hold |
                    GestureType.Tap |
                    GestureType.DoubleTap |
                    GestureType.FreeDrag |
                    GestureType.Flick |
                    GestureType.Pinch;
#endif

            _game.Components.Add(this);
        }

        // Reset cached states (useful when changing scenes or pausing).
        public static void ClearState()
        {
            previousMouseState = Mouse.GetState();
            currentMouseState = Mouse.GetState();
            previousKeyState = Keyboard.GetState();
            currentKeyState = Keyboard.GetState();
#if ANDROID
            currentGestureType = GestureType.None;
#endif
        }

        // Called each frame by the GameComponent system.
        // - Updates previous/current snapshots.
        // - Collects simple text-key presses for UI input.
        // - Handles touch input when compiled for Android.
        public override void Update(GameTime gametime)
        {
            previousPadState = currentPadState;
            previousKeyState = currentKeyState;

            currentPadState = GamePad.GetState(PlayerIndex.One);
            currentKeyState = Keyboard.GetState();

#if WINDOWS
            previousMouseState = currentMouseState;
            currentMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            currentMouseState = Mouse.GetState();
#endif

            KeysPressedInLastFrame.Clear();
            CheckForTextInput();
#if ANDROID
            HandleTouchInput();
#endif
            base.Update(gametime);
        }

        // Simple list used by UI or consoles to obtain the first textual key pressed this frame.
        public List<string> KeysPressedInLastFrame = new List<string>();

        // Scan all Keys and add the first pressed key (useful for single-key text input).
        private void CheckForTextInput()
        {
            foreach (var key in Enum.GetValues(typeof(Keys)) as Keys[])
            {
                if (IsKeyPressed(key))
                {
                    KeysPressedInLastFrame.Add(key.ToString());
                    break;
                }
            }
        }

        // GamePad helpers
        public static bool IsButtonPressed(Buttons buttonToCheck)
        {
            // true when transitioned from down -> up in the last frame
            if (currentPadState.IsButtonUp(buttonToCheck) && previousPadState.IsButtonDown(buttonToCheck))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsButtonHeld(Buttons buttonToCheck)
        {
            return currentPadState.IsButtonDown(buttonToCheck);
        }

        // Keyboard helpers
        public static bool IsKeyHeld(Keys buttonToCheck)
        {
            return currentKeyState.IsKeyDown(buttonToCheck);
        }

        public static bool IsKeyPressed(Keys keyToCheck)
        {
            // Note: this implementation treats pressed as previous down -> current up (edge detection).
            if (currentKeyState.IsKeyUp(keyToCheck) && previousKeyState.IsKeyDown(keyToCheck))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Expose raw current states to other systems when needed.
        public static GamePadState CurrentPadState
        {
            get { return currentPadState; }
            set { currentPadState = value; }
        }
        public static KeyboardState CurrentKeyState
        {
            get { return currentKeyState; }
        }

        public static MouseState CurrentMouseState
        {
            get { return currentMouseState; }
        }

        public static MouseState PreviousMouseState
        {
            get { return previousMouseState; }
        }

#if ANDROID
        // Android touch/accelerometer accessors
        public static Point TouchPoint
        {
            get { return touchPoint; }
            set { touchPoint = value; }
        }

        public static GestureType CurrentGestureType
        {
            get
            {
                GestureType ret = currentGestureType;
                currentGestureType = GestureType.None; // consume once
                return ret;
            }

            set
            {
                currentGestureType = value;
            }
        }

        public static Vector2 CurrentAcceleromoterReading
        {
            get { return currentAcceleromoterReading; }
            set { currentAcceleromoterReading = value; }
        }

        // Process queued touch gestures from TouchPanel and update touchPoint/currentGestureType.
        private void HandleTouchInput()
        {
            TouchCollection touches = TouchPanel.GetState();
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                if (touches.Count > 0 && touches[0].State == TouchLocationState.Pressed)
                {
                    touchPoint = new Point((int)touches[0].Position.X, (int)touches[0].Position.Y);
                }

                switch (gesture.GestureType)
                {
                    case GestureType.Tap:
                        currentGestureType = GestureType.DoubleTap;
                        touchPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);
                        break;
                    case GestureType.DoubleTap:
                        touchPoint = new Point((int)gesture.Position.X, (int)gesture.Position.Y);
                        currentGestureType = GestureType.DoubleTap;
                        break;
                    case GestureType.Hold:
                        break;
                    case GestureType.FreeDrag:
                        break;
                    case GestureType.Flick:
                        break;
                    case GestureType.Pinch:
                        // Example: compute scale change from pinch delta (not applied here).
                        Vector2 a = gesture.Position;
                        Vector2 aOld = gesture.Position - gesture.Delta;
                        Vector2 b = gesture.Position2;
                        Vector2 bOld = gesture.Position2 - gesture.Delta2;
                        float d = Vector2.Distance(a, b);
                        float dOld = Vector2.Distance(aOld, bOld);
                        float scaleChange = (d - dOld) * .01f;
                        break;
                }
            }

        }

#endif

#if WINDOWS
        // Mouse helpers (edge detection using previous/current MouseState)
        public static bool IsMouseLeftClick()
        {
            if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
                return true;
            else 
                return false;
        }

        public static bool IsMouseRightClick()
        {
            if (currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMouseRightHeld()
        {
            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMouseLeftHeld()
        {
            if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        // Current mouse position in screen coordinates (updated in Update).
        public static Vector2 MousePosition
        {
            get { return currentMousePos; }
        }
#endif



    }
}
