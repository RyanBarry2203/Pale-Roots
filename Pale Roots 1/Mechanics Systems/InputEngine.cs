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
    // Central input polling for keyboard, gamepad, mouse, and touch.
    // Exposes an action-based input API used by gameplay and UI.
    public class InputEngine : GameComponent
    {
        // Action mapping for named inputs.
        // Maps action names to keyboard keys.
        private static Dictionary<string, Keys> _keyBindings = new Dictionary<string, Keys>();

        // Maps action names to mouse button indices (0=Left, 1=Right, 2=Middle).
        private static Dictionary<string, int> _mouseBindings = new Dictionary<string, int>();

        // GamePad state history used for pressed/held detection.
        private static GamePadState previousPadState;
        private static GamePadState currentPadState;

        // Keyboard state history used for pressed/held detection.
        private static KeyboardState previousKeyState;
        private static KeyboardState currentKeyState;

        // Mouse positions and states used for click and cursor queries.
        private static Vector2 previousMousePos;
        private static Vector2 currentMousePos;
        private static MouseState previousMouseState;
        private static MouseState currentMouseState;

#if ANDROID
        // Mobile-only input and accelerometer fields.
        private static Vector2 previousAccelerometerReading;
        private static Accelerometer _acceleromter;
        private static Vector2 currentAcceleromoterReading;
        private static Point touchPoint;
        private static GestureType currentGestureType;

        // Update the cached accelerometer reading when the sensor fires.
        private void _acceleromter_CurrentValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            previousAccelerometerReading = CurrentAcceleromoterReading;
            currentAcceleromoterReading.Y = -(float)e.SensorReading.Acceleration.Y;
            currentAcceleromoterReading.X = -(float)e.SensorReading.Acceleration.X;
        }
#endif

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

        // Reset input history to the current hardware state.
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

        public List<string> KeysPressedInLastFrame = new List<string>();

        // Detect which key was pressed this frame for simple text input usage.
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


        // Register a keyboard key for a named action.
        public static void RegisterBinding(string actionName, Keys key)
        {
            if (_keyBindings.ContainsKey(actionName)) _keyBindings[actionName] = key;
            else _keyBindings.Add(actionName, key);
        }

        // Register a mouse button index for a named action.
        public static void RegisterMouseBinding(string actionName, int mouseButtonIndex)
        {
            if (_mouseBindings.ContainsKey(actionName)) _mouseBindings[actionName] = mouseButtonIndex;
            else _mouseBindings.Add(actionName, mouseButtonIndex);
        }

        // Query if a named action was pressed this frame.
        public static bool IsActionPressed(string actionName)
        {
            if (_keyBindings.ContainsKey(actionName))
            {
                return IsKeyPressed(_keyBindings[actionName]);
            }
            if (_mouseBindings.ContainsKey(actionName))
            {
                int btn = _mouseBindings[actionName];
                if (btn == 0) return IsMouseLeftClick();
                if (btn == 1) return IsMouseRightClick();
            }
            return false;
        }

        // Query if a named action is currently held down.
        public static bool IsActionHeld(string actionName)
        {
            if (_keyBindings.ContainsKey(actionName))
            {
                return IsKeyHeld(_keyBindings[actionName]);
            }
            if (_mouseBindings.ContainsKey(actionName))
            {
                int btn = _mouseBindings[actionName];
                if (btn == 0) return IsMouseLeftHeld();
                if (btn == 1) return IsMouseRightHeld();
            }
            return false;
        }


        // GamePad helpers for pressed/held checks.
        public static bool IsButtonPressed(Buttons buttonToCheck)
        {
            if (currentPadState.IsButtonUp(buttonToCheck) && previousPadState.IsButtonDown(buttonToCheck))
                return true;
            else
                return false;
        }
        public static bool IsButtonHeld(Buttons buttonToCheck)
        {
            return currentPadState.IsButtonDown(buttonToCheck);
        }

        // Keyboard helpers for held/pressed checks.
        public static bool IsKeyHeld(Keys buttonToCheck)
        {
            return currentKeyState.IsKeyDown(buttonToCheck);
        }

        public static bool IsKeyPressed(Keys keyToCheck)
        {
            if (currentKeyState.IsKeyDown(keyToCheck) && previousKeyState.IsKeyUp(keyToCheck))
                return true;
            else
                return false;
        }

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
                currentGestureType = GestureType.None; 
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

        // Process touch gestures and update the stored touch point and gesture type.
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
                        break;
                }
            }
        }
#endif

#if WINDOWS
        // Mouse click and hold helpers for Windows builds.
        public static bool IsMouseLeftClick()
        {
            if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                return true;
            else
                return false;
        }

        public static bool IsMouseRightClick()
        {
            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
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

        // Current mouse position in screen coordinates.
        public static Vector2 MousePosition
        {
            get { return currentMousePos; }
        }
#endif
    }
}