using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // 1. HEAVENS FURY: Cuts enemy health in half, debuffs new enemies
    public class HeavensFurySpell : Spell
    {

        public HeavensFurySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Heaven's Fury";
            CooldownDuration = 30000f; // 30 sec cooldown
            ActiveDuration = 15000f;   // 15 sec duration

            // Load Animation (Assuming a lightning strike or beam from your sheet)
            // Adjust these coordinates!
            _animManager.AddAnimation("Cast", new Animation(sheet, 12, 0, 300f, true, 1, 128));
            _animManager.Play("Cast");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            // 1. Cut existing enemies health in half
            foreach (var enemy in engine._enemies) // You might need to make _enemies public or add a getter
            {
                if (enemy.IsAlive)
                {
                    enemy.Health = enemy.Health / 2;
                    // Visual flair: Apply knockback or particle here
                }
            }

            // 2. Set global modifier for NEW enemies
            engine.GlobalEnemyHealthMult = 0.5f;
        }

        // In HeavensFurySpell class
        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null)
            {
                _engineRef.GlobalEnemyHealthMult = 1.0f;
            }
        }

        // Fix for accessing engine in EndEffect:
        // Ideally, Spell should store a reference to Engine passed in Cast, 
        // but for now, we will rely on the Engine checking the spell state.
    }

    // 2. HOLY NOVA: Instakill AoE
    public class HolyNovaSpell : Spell
    {
        private float _radius = 400f;

        public HolyNovaSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Nova";
            CooldownDuration = 10000f;
            ActiveDuration = 1000f; // Short visual duration

            // Explosion animation
            _animManager.AddAnimation("Explode", new Animation(sheet, 10, 0, 300f, false, 1, 128));
            _animManager.Play("Explode");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            // Center on Player
            _position = engine.GetPlayer().Position;

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
                // Massive damage to ensure kill
                CombatSystem.DealDamage(engine.GetPlayer(), enemy, 9999);
            }
        }
    }

    // 3. HOLY SHIELD: Double Ally Health
    public class HolyShieldSpell : Spell
    {
        public HolyShieldSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Holy Shield";
            CooldownDuration = 20000f;
            ActiveDuration = 0f; // Instant effect

            _animManager.AddAnimation("Holy Shield", new Animation(sheet, 11, 0, 300f, false, 1, 64));
            _animManager.Play("Holy Shield");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            // Heal and Buff Allies
            foreach (var ally in engine._allies) // Make _allies public/accessible
            {
                if (ally.IsAlive)
                {
                    // "Double Health" interpretation: Heal to full + Overheal? 
                    // Or just multiply current health?
                    ally.Health *= 2;
                }
            }
        }
    }

    // 4. SMITE: Heal Player
    public class SmiteSpell : Spell
    {
        public SmiteSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Smite";
            CooldownDuration = 5000f;
            ActiveDuration = 500f;

            // Heal effect animation
            _animManager.AddAnimation("Heal", new Animation(sheet, 11, 0, 300f, false, 1, 64));
            _animManager.Play("Heal");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {
            Player p = engine.GetPlayer();
            _position = p.Position; // Draw effect on player

            p.Health += 100;
            if (p.Health > p.MaxHealth) p.Health = p.MaxHealth;
        }
    }

    // 5. ELECTRICITY: Stun + Stop Spawning
    public class ElectricitySpell : Spell
    {
       

        public ElectricitySpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Electricity";
            CooldownDuration = 40000f;
            ActiveDuration = 15000f;

            // Storm animation
            _animManager.AddAnimation("Storm", new Animation(sheet, 5, 0, 300f, true, 1, 128));
            _animManager.Play("Storm");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {

            engine.SpawningBlocked = true;

            foreach (var enemy in engine._enemies)
            {
                enemy.IsStunned = true;
            }
        }

        protected override void OnUpdateActive(GameTime gameTime)
        {
            // Keep stunning new enemies if they somehow appear, 
            // or ensure current ones stay stunned
            if (_engineRef != null)
            {
                foreach (var enemy in _engineRef._enemies)
                {
                    enemy.IsStunned = true;
                }
            }
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null)
            {
                _engineRef.SpawningBlocked = false;
                foreach (var enemy in _engineRef._enemies)
                {
                    enemy.IsStunned = false;
                }
            }
        }
    }

    // 6. SWORD OF JUSTICE: Double Damage
    public class SwordOfJusticeSpell : Spell
    {


        public SwordOfJusticeSpell(Game game, Texture2D sheet) : base(game)
        {
            Name = "Sword of Justice";
            CooldownDuration = 25000f;
            ActiveDuration = 15000f;

            _animManager.AddAnimation("Justice", new Animation(sheet, 5, 0, 300f, true, 1, 128));
            _animManager.Play("Justice");
        }

        protected override void OnCast(ChaseAndFireEngine engine)
        {

            engine.GlobalPlayerDamageMult = 2.0f;
        }

        protected override void EndEffect()
        {
            base.EndEffect();
            if (_engineRef != null)
            {
                _engineRef.GlobalPlayerDamageMult = 1.0f;
            }
        }
    }
}