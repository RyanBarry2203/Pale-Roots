using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // ==========================================
    // SPELL 1: SMITE (The Lifeline)
    // A simple, immediate heal that attaches an animation to the player.
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
            // Cap health at Max so the player doesn't become "over-healed."
            if (p.Health > p.MaxHealth) p.Health = p.MaxHealth;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            // Lock the sparkling heal animation to the player's current position as they move.
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 30; // Offset so it centers on the body, not the feet.
            }
        }
    }

    // ==========================================
    // SPELL 2: HOLY NOVA (The Panic Button)
    // A massive AOE blast. Note the Boss Arena scaling—it's a tactical nuke against 
    // minions but balanced damage against the Boss.
    // ==========================================
    public class HolyNovaSpell : Spell
    {
        private float _radius = 300f;

        public HolyNovaSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Nova";
            Description = "Instakill AOE on your cursor";
            CooldownDuration = 10000f;
            ActiveDuration = 1000f;
            Scale = 5.0f;
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Explode", new Animation(sheet, 10, 0, 100f, false, 1, 128));
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _animManager.Play("Explode");
            List<Enemy> toKill = new List<Enemy>();

            // 1. Proximity Detection: Find every enemy within the 300px radius.
            foreach (var enemy in engine._enemies)
            {
                if (Vector2.Distance(enemy.Position, _position) < _radius)
                {
                    toKill.Add(enemy);
                }
            }

            // 2. Dynamic Balancing: If we are fighting the BlackHoleBoss, do high damage. 
            // If we're in the overworld, just instakill (9999).
            int damage = engine.IsBossArena ? 200 : 9999;
            CooldownDuration = engine.IsBossArena ? 15000f : 10000f;

            foreach (var enemy in toKill)
            {
                CombatSystem.DealDamage(engine.GetPlayer(), enemy, damage);
            }
        }
    }

    // ==========================================
    // SPELL 3: HEAVENS FURY (The Screen Clear)
    // This spell manipulates the global 'GlobalEnemyHealthMult' in the engine.
    // ==========================================
    public class HeavensFurySpell : Spell
    {
        private List<Vector2> _strikeLocations = new List<Vector2>();

        public HeavensFurySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Heaven's Fury";
            Description = "Half Enemy Healthbars";
            CooldownDuration = 30000f;
            ActiveDuration = 15000f;
            Scale = 3.0f;
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Cast", new Animation(sheet, 12, 0, 100f, true, 1, 128));
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _animManager.Play("Cast");
            _strikeLocations.Clear();
            Vector2 center = engine.GetPlayer().Position;

            // Generate 10 random visual points for the lightning strikes.
            for (int i = 0; i < 10; i++)
            {
                float offX = CombatSystem.RandomInt(-900, 900);
                float offY = CombatSystem.RandomInt(-500, 500);
                _strikeLocations.Add(center + new Vector2(offX, offY));
            }

            if (engine.IsBossArena)
            {
                // Against the boss, it weakens his gravitational pull rather than halving health.
                foreach (var enemy in engine._enemies)
                {
                    if (enemy is BlackHoleBoss boss) boss.GravityMultiplier = 0.5f;
                }
            }
            else
            {
                // Globally halve the health of every enemy on the map.
                foreach (var enemy in engine._enemies)
                {
                    if (enemy.IsAlive) enemy.Health /= 2;
                }
                engine.GlobalEnemyHealthMult = 0.5f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // We override the base Draw because this spell draws in 10 places at once!
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
                // REVERT: Critical cleanup so the next enemies spawned have full health again.
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
    // SPELL 4: HOLY SHIELD (The Tank)
    // Buffs both the Player and their Allies.
    // ==========================================
    public class HolyShieldSpell : Spell
    {
        public HolyShieldSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Shield";
            Description = "Double Healthbar";
            CooldownDuration = 20000f;
            ActiveDuration = 15000f;
            Scale = 1.5f;
            ThemeColor = Color.DeepSkyBlue;

            _animManager.AddAnimation("Shield", new Animation(sheet, 11, 0, 100f, true, 1, 64));
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            _animManager.Play("Shield");

            // Buff all active allies.
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

        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null)
            {
                // REVERT: Bring MaxHealth back to normal.
                Player p = _engineRef.GetPlayer();
                p.MaxHealth /= 2;
                if (p.Health > p.MaxHealth) p.Health = p.MaxHealth;

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
    // SPELL 5: ELECTRICITY (Crowd Control)
    // This spell freezes the game's spawning logic entirely.
    // ==========================================
    public class ElectricitySpell : Spell
    {
        private List<Vector2> _strikeLocations = new List<Vector2>();

        public ElectricitySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Electricity";
            Description = "Stun Enemies and stop spawning";
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

            // Tell the engine to stop producing new enemies while this is active.
            engine.SpawningBlocked = true;
            foreach (var enemy in engine._enemies) enemy.IsStunned = true;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            // Keep existing enemies stunned for the full duration.
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
                // REVERT: Let the spawning resume and let enemies move again.
                _engineRef.SpawningBlocked = false;
                foreach (var enemy in _engineRef._enemies) enemy.IsStunned = false;
            }
        }
    }

    // ==========================================
    // SPELL 6: SWORD OF JUSTICE (The Damage Buff)
    // A simple multiplier that makes the player's basic attacks hit much harder.
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
            // Set the global engine multiplier to 2.0x
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
            // REVERT: Bring player damage back to 1.0x
            if (_engineRef != null) _engineRef.GlobalPlayerDamageMult = 1.0f;
        }
    }
}