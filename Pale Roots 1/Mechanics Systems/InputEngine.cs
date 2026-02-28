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
    // This is the universal input wrapper for the entire game.
    // Instead of gameplay classes talking directly to the keyboard or mouse, they ask this Engine what actions occurred.
    // It also smoothly handles compiling for different platforms (like Windows vs Android) using preprocessor directives.
    public class InputEngine : GameComponent
    {
        // --- ACTION MAPPING ---
        // These dictionaries map a plain-English string (like "Jump" or "CastSpell1") to a physical hardware button.
        // This abstraction is what allows for custom keybindings in video games.
        private static Dictionary<string, Keys> _keyBindings = new Dictionary<string, Keys>();
        private static Dictionary<string, int> _mouseBindings = new Dictionary<string, int>();

        // We track both the "Current" and "Previous" frame states for all hardware.
        // Comparing these two states is how we determine if a button was *just* pressed, or if it is being held down.
        private static GamePadState previousPadState;
        private static GamePadState currentPadState;

        private static KeyboardState previousKeyState;
        private static KeyboardState currentKeyState;

        private static Vector2 previousMousePos;
        private static Vector2 currentMousePos;
        private static MouseState previousMouseState;
        private static MouseState currentMouseState;

#if ANDROID
        // These variables and methods are entirely ignored by the compiler unless you are building for Android.
        private static Vector2 previousAccelerometerReading;
        private static Accelerometer _acceleromter;
        private static Vector2 currentAcceleromoterReading;
        private static Point touchPoint;
        private static GestureType currentGestureType;

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
            
            // Define exactly which types of mobile swipes and taps the engine should listen for.
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

        public static void ClearState()
        {
            // This is a crucial safety function used by the GameStateManager.
            // When transitioning from a Menu into Gameplay, we call this to wipe the input memory.
            // Otherwise, the same mouse click used to click "Play" might accidentally fire a spell the second the level loads!
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
            // Push the current states from the last frame into the "previous" bucket, 
            // then poll the hardware for fresh data.
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

        private void CheckForTextInput()
        {
            // Loops through every possible keyboard key to see if any were pressed this exact frame.
            // Useful for typing in player names or debug consoles.
            foreach (var key in Enum.GetValues(typeof(Keys)) as Keys[])
            {
                if (IsKeyPressed(key))
                {
                    KeysPressedInLastFrame.Add(key.ToString());
                    break;
                }
            }
        }

        // --- ACTION API METHODS ---

        public static void RegisterBinding(string actionName, Keys key)
        {
            // Links a string action to a keyboard key. If the action already exists, it overwrites it (Remapping).
            if (_keyBindings.ContainsKey(actionName)) _keyBindings[actionName] = key;
            else _keyBindings.Add(actionName, key);
        }

        public static void RegisterMouseBinding(string actionName, int mouseButtonIndex)
        {
            if (_mouseBindings.ContainsKey(actionName)) _mouseBindings[actionName] = mouseButtonIndex;
            else _mouseBindings.Add(actionName, mouseButtonIndex);
        }

        public static bool IsActionPressed(string actionName)
        {
            // Checks if the hardware button associated with this action was tapped THIS EXACT FRAME.
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

        public static bool IsActionHeld(string actionName)
        {
            // Checks if the hardware button associated with this action is currently being held down continuously.
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

        // --- RAW HARDWARE HELPERS ---
        // These methods perform the actual edge-detection math comparing the Current and Previous states.

        public static bool IsButtonPressed(Buttons buttonToCheck)
        {
            // It is only a "Press" if the button is down right now, but was UP on the previous frame.
            if (currentPadState.IsButtonUp(buttonToCheck) && previousPadState.IsButtonDown(buttonToCheck))
                return true;
            else
                return false;
        }

        public static bool IsButtonHeld(Buttons buttonToCheck)
        {
            return currentPadState.IsButtonDown(buttonToCheck);
        }

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

        private void HandleTouchInput()
        {
            // Mobile gesture parsing logic. Translates raw screen touches into actionable gameplay events like Taps and Drags.
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

        public static Vector2 MousePosition
        {
            get { return currentMousePos; }
        }
#endif
    }
}