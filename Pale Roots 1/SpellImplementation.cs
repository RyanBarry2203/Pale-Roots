using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // ==========================================
    // SPELL 1: SMITE
    // ==========================================
    public class SmiteSpell : Spell
    {
        public SmiteSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Smite";
            CooldownDuration = 5000f;
            ActiveDuration = 1100f;
            Scale = 2.0f; // Slightly smaller than default

            _animManager.AddAnimation("Heal", new Animation(sheet, 11, 0, 100f, false, 1, 64));
            _animManager.Play("Heal");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            Player p = engine.GetPlayer();
            p.Health += 100;
            if (p.Health > p.MaxHealth) p.Health = p.MaxHealth;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 30;
            }
        }
    }

    // ==========================================
    // SPELL 2: HOLY NOVA
    // ==========================================
    public class HolyNovaSpell : Spell
    {
        private float _radius = 250f;

        public HolyNovaSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Nova";
            CooldownDuration = 10000f;
            ActiveDuration = 1000f;
            Scale = 3.0f; // Keep big

            _animManager.AddAnimation("Explode", new Animation(sheet, 10, 0, 100f, false, 1, 128));
            _animManager.Play("Explode");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            List<Enemy> toKill = new List<Enemy>();
            foreach (var enemy in engine._enemies)
            {
                if (Vector2.Distance(enemy.Position, _position) < _radius)
                {
                    toKill.Add(enemy);
                }
            }
            foreach (var enemy in toKill)
            {
                CombatSystem.DealDamage(engine.GetPlayer(), enemy, 9999);
            }
        }
    }

    // ==========================================
    // SPELL 3: HEAVENS FURY
    // ==========================================
    public class HeavensFurySpell : Spell
    {
        private List<Vector2> _strikeLocations = new List<Vector2>();

        public HeavensFurySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Heaven's Fury";
            CooldownDuration = 30000f;
            ActiveDuration = 15000f;
            Scale = 3.0f; // Keep big

            _animManager.AddAnimation("Cast", new Animation(sheet, 12, 0, 100f, true, 1, 128));
            _animManager.Play("Cast");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _strikeLocations.Clear();
            Vector2 center = engine.GetPlayer().Position;

            for (int i = 0; i < 10; i++)
            {
                float offX = CombatSystem.RandomInt(-900, 900);
                float offY = CombatSystem.RandomInt(-500, 500);
                _strikeLocations.Add(center + new Vector2(offX, offY));
            }

            foreach (var enemy in engine._enemies)
            {
                if (enemy.IsAlive) enemy.Health /= 2;
            }
            engine.GlobalEnemyHealthMult = 0.5f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                foreach (Vector2 loc in _strikeLocations)
                {
                    _animManager.Draw(spriteBatch, loc, Scale, SpriteEffects.None);
                }
            }
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null) _engineRef.GlobalEnemyHealthMult = 1.0f;
        }
    }

    // ==========================================
    // SPELL 4: HOLY SHIELD (FIXED SIZE)
    // ==========================================
    public class HolyShieldSpell : Spell
    {
        public HolyShieldSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Shield";
            CooldownDuration = 20000f;
            ActiveDuration = 15000f;

            // FIX: Reduced Scale so it isn't massive
            Scale = 1.5f;

            // FIX: Ensure width is 64 (standard tile)
            _animManager.AddAnimation("Shield", new Animation(sheet, 11, 0, 100f, true, 1, 64));
            _animManager.Play("Shield");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            foreach (var ally in engine._allies)
            {
                if (ally.IsAlive) ally.Health *= 2;
            }
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 30;
            }
        }
    }

    // ==========================================
    // SPELL 5: ELECTRICITY
    // ==========================================
    public class ElectricitySpell : Spell
    {
        private List<Vector2> _strikeLocations = new List<Vector2>();

        public ElectricitySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Electricity";
            CooldownDuration = 40000f;
            ActiveDuration = 15000f;
            Scale = 3.0f;

            _animManager.AddAnimation("Storm", new Animation(sheet, 5, 0, 100f, true, 1, 128));
            _animManager.Play("Storm");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _strikeLocations.Clear();
            Vector2 center = engine.GetPlayer().Position;

            for (int i = 0; i < 10; i++)
            {
                float offX = CombatSystem.RandomInt(-900, 900);
                float offY = CombatSystem.RandomInt(-500, 500);
                _strikeLocations.Add(center + new Vector2(offX, offY));
            }

            engine.SpawningBlocked = true;
            foreach (var enemy in engine._enemies) enemy.IsStunned = true;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            if (_engineRef != null)
            {
                foreach (var enemy in _engineRef._enemies) enemy.IsStunned = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                foreach (Vector2 loc in _strikeLocations)
                {
                    _animManager.Draw(spriteBatch, loc, Scale, SpriteEffects.None);
                }
            }
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null)
            {
                _engineRef.SpawningBlocked = false;
                foreach (var enemy in _engineRef._enemies) enemy.IsStunned = false;
            }
        }
    }

    // ==========================================
    // SPELL 6: SWORD OF JUSTICE (FIXED DOUBLE IMAGE)
    // ==========================================
    public class SwordOfJusticeSpell : Spell
    {
        public SwordOfJusticeSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Sword of Justice";
            CooldownDuration = 25000f;
            ActiveDuration = 15000f;

            // FIX 1: Reduced Scale
            Scale = 1.5f;

            // FIX 2: Changed width from 128 to 64. 
            // This stops it from grabbing 2 frames at once (the double image issue).
            _animManager.AddAnimation("Justice", new Animation(sheet, 5, 0, 200f, true, 1, 64));
            _animManager.Play("Justice");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            engine.GlobalPlayerDamageMult = 2.0f;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 50;
            }
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null) _engineRef.GlobalPlayerDamageMult = 1.0f;
        }
    }
}