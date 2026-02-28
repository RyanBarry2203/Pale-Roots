using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pale_Roots_1
{
    // The "blueprint" for all magic and abilities. 
    // Being 'abstract' means you can't create a generic 'Spell', 
    // you must create specific spells that inherit these rules.
    public abstract class Spell
    {
        // --- DATA & UI ---
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public Texture2D Icon { get; set; }
        public Color ThemeColor { get; protected set; } = Color.White;

        // --- TIMERS ---
        public float CooldownDuration { get; protected set; } // Total wait time
        public float CurrentCooldown { get; set; } = 0f;    // Time remaining until next cast
        public float ActiveDuration { get; protected set; }   // How long the spell lasts in the world
        public float CurrentActiveTimer { get; set; } = 0f;
        public bool IsActive { get; protected set; } = false;
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

            // 1. Tick down the cooldown even if the spell isn't currently firing.
            if (CurrentCooldown > 0) CurrentCooldown -= dt;

            // 2. Logic for spells that are currently "alive" in the game world.
            if (IsActive)
            {
                CurrentActiveTimer -= dt;
                _animManager.Update(gameTime);

                // This allows child spells (like a fire area) to do damage every frame.
                OnUpdateActive(gameTime);

                // 3. Self-destruct logic once the timer runs out.
                if (CurrentActiveTimer <= 0)
                {
                    EndEffect();
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // Only render the spell's visual effect if it is actually in use.
            if (IsActive)
            {
                _animManager.Draw(spriteBatch, _position, Scale, SpriteEffects.None);
            }
        }

        // The entry point called by the Player or Enemy.
        public bool Cast(ChaseAndFireEngine engine, Vector2 targetPos)
        {
            // Gatekeeping: You can't cast if it's on cooldown or already active.
            if (CurrentCooldown > 0 || IsActive) return false;

            _engineRef = engine;
            CurrentCooldown = CooldownDuration;
            CurrentActiveTimer = ActiveDuration;
            IsActive = true;
            _position = targetPos;

            _animManager.Reset(); // Start the animation from frame 0
            OnCast(engine);       // Trigger the unique logic of the specific spell
            return true;
        }

        // --- HOOKS FOR SUBCLASSES ---

        // MUST be implemented (e.g., spawn a fireball projectile).
        protected abstract void OnCast(ChaseAndFireEngine engine);

        // CAN be overridden (e.g., play a sound effect when a spell disappears).
        protected virtual void EndEffect() { IsActive = false; }

        // CAN be overridden (e.g., a healing aura checking for players nearby).
        protected virtual void OnUpdateActive(GameTime gameTime) { }
    }
}