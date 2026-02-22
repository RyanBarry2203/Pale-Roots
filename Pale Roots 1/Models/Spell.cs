using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    public abstract class Spell
    {
        public string Name { get; protected set; }
        public float CooldownDuration { get; protected set; }
        public float CurrentCooldown { get; set; } = 0f;

        public float ActiveDuration { get; protected set; }
        public float CurrentActiveTimer { get; set; } = 0f;
        public bool IsActive { get; protected set; } = false;

        // NEW: Allow spells to define their own size
        public float Scale { get; set; } = 3.0f;

        protected Game _game;
        protected AnimationManager _animManager;
        protected Vector2 _position;
        protected ChaseAndFireEngine _engineRef;

        public Spell(Game game)
        {
            _game = game;
            _animManager = new AnimationManager();
        }

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

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                // FIX: Use the variable Scale instead of hardcoded 3.0f
                _animManager.Draw(spriteBatch, _position, Scale, SpriteEffects.None);
            }
        }

        public bool Cast(ChaseAndFireEngine engine, Vector2 targetPos)
        {
            if (CurrentCooldown > 0 || IsActive) return false;

            _engineRef = engine;
            CurrentCooldown = CooldownDuration;
            CurrentActiveTimer = ActiveDuration;
            IsActive = true;
            _position = targetPos;

            OnCast(engine);
            return true;
        }

        protected abstract void OnCast(ChaseAndFireEngine engine);
        protected virtual void EndEffect() { IsActive = false; }
        protected virtual void OnUpdateActive(GameTime gameTime) { }
    }
}