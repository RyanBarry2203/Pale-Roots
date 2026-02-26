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
            Description = "Heal yourself for 100HP.";
            CooldownDuration = 5000f;
            ActiveDuration = 1100f;
            Scale = 2.0f;
            ThemeColor = Color.Gold;

            _animManager.AddAnimation("Heal", new Animation(sheet, 11, 0, 100f, false, 1, 64));
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {

            _animManager.Play("Heal");

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
        private float _radius = 300f;

        public HolyNovaSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Nova";
            Description = "Instakill AOE on your cursor"; // NEW: Data-Driven UI
            CooldownDuration = 10000f;
            ActiveDuration = 1000f;
            Scale = 5.0f; 
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Explode", new Animation(sheet, 10, 0, 100f, false, 1, 128));
            
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {

            _animManager.Play("Explode");
            // Restored the generic <Enemy> type!
            List<Enemy> toKill = new List<Enemy>();

            // 1. Find enemies in range
            foreach (var enemy in engine._enemies)
            {
                if (Vector2.Distance(enemy.Position, _position) < _radius)
                {
                    toKill.Add(enemy);
                }
            }

            int damage = engine.IsBossArena ? 200 : 9999;
            CooldownDuration = engine.IsBossArena ? 15000f : 10000f;

            foreach (var enemy in toKill)
            {
                CombatSystem.DealDamage(engine.GetPlayer(), enemy, damage);
            }
        }
    }

    // ==========================================
    // SPELL 3: HEAVENS FURY
    // ==========================================
    public class HeavensFurySpell : Spell
    {
        // Restored the generic <Vector2> type!
        private List<Vector2> _strikeLocations = new List<Vector2>();

        public HeavensFurySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Heaven's Fury";
            Description = "Half Enemy Healthbars"; // NEW: Data-Driven UI
            CooldownDuration = 30000f;
            ActiveDuration = 15000f;
            Scale = 3.0f; // Keep big
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Cast", new Animation(sheet, 12, 0, 100f, true, 1, 128));
            
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {

            _animManager.Play("Cast");

            _strikeLocations.Clear();
            Vector2 center = engine.GetPlayer().Position;

            for (int i = 0; i < 10; i++)
            {
                float offX = CombatSystem.RandomInt(-900, 900);
                float offY = CombatSystem.RandomInt(-500, 500);
                _strikeLocations.Add(center + new Vector2(offX, offY));
            }

            if (engine.IsBossArena)
            {
                foreach (var enemy in engine._enemies)
                {
                    if (enemy is BlackHoleBoss boss) boss.GravityMultiplier = 0.5f;
                }
            }
            else
            {
                foreach (var enemy in engine._enemies)
                {
                    if (enemy.IsAlive) enemy.Health /= 2;
                }
                engine.GlobalEnemyHealthMult = 0.5f;
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
                _engineRef.GlobalEnemyHealthMult = 1.0f;

                if (_engineRef.IsBossArena)
                {
                    foreach (var enemy in _engineRef._enemies)
                    {
                        if (enemy is BlackHoleBoss boss) boss.GravityMultiplier = 1.0f;
                    }
                }
            }
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
            Description = "Double Healthbar"; // NEW: Data-Driven UI
            CooldownDuration = 20000f;
            ActiveDuration = 15000f;
            Scale = 1.5f;
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Shield", new Animation(sheet, 11, 0, 100f, true, 1, 64));
            
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _animManager.Play("Shield");

            foreach (var ally in engine._allies)
            {
                if (ally.IsAlive) ally.Health *= 2;
            }


            Player p = engine.GetPlayer();
            p.MaxHealth *= 2;
            p.Health *= 2;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 30;
            }
        }
        // ENGINE POLISH: Clean up the buff when the spell duration ends!
        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null)
            {
                // 1. Revert Player Health
                Player p = _engineRef.GetPlayer();
                p.MaxHealth /= 2;

                // Cap current health so they don't have 200/100 HP
                if (p.Health > p.MaxHealth) p.Health = p.MaxHealth;

                // 2. Revert Ally Health (ensure it doesn't drop below 1 and kill them)
                foreach (var ally in _engineRef._allies)
                {
                    if (ally.IsAlive)
                    {
                        ally.Health /= 2;
                        if (ally.Health < 1) ally.Health = 1;
                    }
                }
            }
        }
    }

    // ==========================================
    // SPELL 5: ELECTRICITY
    // ==========================================
    public class ElectricitySpell : Spell
    {
        // Restored the generic <Vector2> type!
        private List<Vector2> _strikeLocations = new List<Vector2>();

        public ElectricitySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Electricity";
            Description = "Stun Enemies and stop spawning"; // NEW: Data-Driven UI
            CooldownDuration = 40000f;
            ActiveDuration = 15000f;
            Scale = 3.0f;
            ThemeColor = Color.Gold;

            _animManager.AddAnimation("Storm", new Animation(sheet, 5, 0, 100f, true, 1, 128));
            
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {

            _animManager.Play("Storm");

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
            Description = "Double Damage";
            CooldownDuration = 25000f;
            ActiveDuration = 15000f;
            Scale = 1f;
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Justice", new Animation(sheet, 5, 0, 200f, true, 1, 64));
            
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _animManager.Play("Justice");
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