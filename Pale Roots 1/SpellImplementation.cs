using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Heals the player and shows a heal animation.
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
            // Play the heal animation and restore player health up to max.
            _animManager.Play("Heal");

            Player p = engine.GetPlayer();
            p.Health += 100;
            if (p.Health > p.MaxHealth) p.Health = p.MaxHealth;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            // Keep the animation positioned near the player while active.
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 30;
            }
        }
    }

    // Deals area damage around the spell position to enemies within radius.
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
            // Play explosion animation and damage enemies in the configured radius.
            _animManager.Play("Explode");
            List<Enemy> toKill = new List<Enemy>();

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

    // Spawn strike markers and reduce enemy health or alter boss behavior while active.
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
            // Choose random strike points and apply the health or gravity changes immediately.
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
            // Draw the strike animations at each chosen location while the spell is active.
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
            // Revert global enemy multipliers and restore boss gravity when the effect ends.
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

    // Double the health of allies and the player while active and revert it on end.
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
            // Play shield animation and double health values for player and allies.
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
            // Keep the shield animation positioned near the player while active.
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 30;
            }
        }

        protected override void EndEffect()
        {
            // Revert player and ally health changes and ensure health is clamped to valid values.
            base.EndEffect();
            if (_engineRef != null)
            {
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

    // Stun all enemies and block spawning while the spell is active.
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
            // Pick strike positions, block spawning, and set enemies to stunned.
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
            // Maintain the stunned state on all enemies while the spell is active.
            if (_engineRef != null)
            {
                foreach (var enemy in _engineRef._enemies) enemy.IsStunned = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw storm animations at each strike location while active.
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
            // Restore spawning and clear the stunned flag on enemies when the effect ends.
            base.EndEffect();
            if (_engineRef != null)
            {
                _engineRef.SpawningBlocked = false;
                foreach (var enemy in _engineRef._enemies) enemy.IsStunned = false;
            }
        }
    }

    // Double the player's damage multiplier for the duration and show a persistent animation.
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
            // Play the justice animation and apply the damage multiplier.
            _animManager.Play("Justice");
            engine.GlobalPlayerDamageMult = 2.0f;
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            // Keep the animation positioned above the player while the spell is active.
            if (_engineRef != null)
            {
                _position = _engineRef.GetPlayer().Position;
                _position.Y -= 50;
            }
        }

        protected override void EndEffect()
        {
            // Reset the player's damage multiplier when the spell ends.
            base.EndEffect();
            if (_engineRef != null) _engineRef.GlobalPlayerDamageMult = 1.0f;
        }
    }
}