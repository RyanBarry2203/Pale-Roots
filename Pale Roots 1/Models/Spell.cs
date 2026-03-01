using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // Base class for spells. Manages cooldowns, active duration, animation, and casting lifecycle.
    public abstract class Spell
    {
        // Identification and visuals.
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public Texture2D Icon { get; set; }
        public Color ThemeColor { get; protected set; } = Color.White;

        // Timing and state.
        public float CooldownDuration { get; protected set; }
        public float CurrentCooldown { get; set; } = 0f;
        public float ActiveDuration { get; protected set; }
        public float CurrentActiveTimer { get; set; } = 0f;
        public bool IsActive { get; protected set; } = false;
        public float Scale { get; set; } = 3.0f;

        // Internal references used by spell implementations.
        protected Game _game;
        protected AnimationManager _animManager;
        protected Vector2 _position;
        protected ChaseAndFireEngine _engineRef;

        public Spell(Game game)
        {
            _game = game;
            _animManager = new AnimationManager();
        }

        // Tick cooldowns and active timers and update animation while active.
        public virtual void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (CurrentCooldown > 0) CurrentCooldown -= dt;

            if (IsActive)
            {
                CurrentActiveTimer -= dt;
                _animManager.Update(gameTime);
                OnUpdateActive(gameTime);

                if (CurrentActiveTimer <= 0)
                {
                    EndEffect();
                }
            }
        }

        // Draw the spell's animation when the effect is active.
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                _animManager.Draw(spriteBatch, _position, Scale, SpriteEffects.None);
            }
        }

        // Attempt to cast the spell at a world position using the provided engine reference.
        // Returns true when the cast succeeds and starts the cooldown and active timers.
        public bool Cast(ChaseAndFireEngine engine, Vector2 targetPos)
        {
            if (CurrentCooldown > 0 || IsActive) return false;
            _engineRef = engine;
            CurrentCooldown = CooldownDuration;
            CurrentActiveTimer = ActiveDuration;
            IsActive = true;
            _position = targetPos;
            _animManager.Reset();
            OnCast(engine);
            return true;
        }

        // Hook for derived classes to implement the immediate cast behavior.
        protected abstract void OnCast(ChaseAndFireEngine engine);

        // Ends the active effect and sets state accordingly.
        protected virtual void EndEffect() { IsActive = false; }

        // Hook for derived classes to update while the effect remains active.
        protected virtual void OnUpdateActive(GameTime gameTime) { }
    }
}