using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Manages all player spells and which spells the player has unlocked.
    public class SpellManager
    {
        private ChaseAndFireEngine _engine;
        private List<Spell> _spells = new List<Spell>();

        private bool[] _unlockedSpells;
        public List<Spell> AllSpells => _spells;

        // Create spell instances and prepare the unlocked flags.
        public SpellManager(ChaseAndFireEngine engine,
                            Texture2D smiteTx,
                            Texture2D novaTx,
                            Texture2D furyTx,
                            Texture2D shieldTx,
                            Texture2D electricTx,
                            Texture2D justiceTx)
        {
            _engine = engine;

            // Initialize Spells
            _spells.Add(new SmiteSpell(engine._gameOwnedBy, smiteTx));
            _spells.Add(new HolyNovaSpell(engine._gameOwnedBy, novaTx));
            _spells.Add(new HeavensFurySpell(engine._gameOwnedBy, furyTx));
            _spells.Add(new HolyShieldSpell(engine._gameOwnedBy, shieldTx));
            _spells.Add(new ElectricitySpell(engine._gameOwnedBy, electricTx));
            _spells.Add(new SwordOfJusticeSpell(engine._gameOwnedBy, justiceTx));

            _unlockedSpells = new bool[_spells.Count];
        }

        // Update each spell and handle player input for casting.
        public void Update(GameTime gameTime)
        {
            foreach (var spell in _spells)
            {
                spell.Update(gameTime);
            }
            HandleInput();
        }

        // Read mouse and action inputs, convert to world coordinates, and trigger casts.
        private void HandleInput()
        {
            // We still need mouse position for aiming, which is specific data, not just a button press.
            MouseState mState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mState.X, mState.Y);
            Matrix inverseTransform = Matrix.Invert(_engine._camera.CurrentCameraTranslation);
            Vector2 mousePos = Vector2.Transform(mouseScreenPos, inverseTransform);

            // Map action inputs to spell indices and cast at the world position.
            if (InputEngine.IsActionPressed("CastSpell1")) CastSpell(0, mousePos);
            if (InputEngine.IsActionPressed("CastSpell2")) CastSpell(1, mousePos);
            if (InputEngine.IsActionPressed("CastSpell3")) CastSpell(2, mousePos);
            if (InputEngine.IsActionPressed("CastSpell4")) CastSpell(3, mousePos);
            if (InputEngine.IsActionPressed("CastSpell5")) CastSpell(4, mousePos);
            if (InputEngine.IsActionPressed("CastSpell6")) CastSpell(5, mousePos);
        }

        // Attempt to cast the spell at the given index toward the target position.
        private void CastSpell(int index, Vector2 target)
        {
            if (index >= 0 && index < _spells.Count)
            {
                if (_unlockedSpells[index])
                {
                    _spells[index].Cast(_engine, target);
                }
            }
        }

        // Unlock a specific spell for use.
        public void UnlockSpell(int index)
        {
            if (index >= 0 && index < _unlockedSpells.Length)
            {
                _unlockedSpells[index] = true;
            }
        }

        // Lock every spell so none can be cast.
        public void LockAllSpells()
        {
            for (int i = 0; i < _unlockedSpells.Length; i++)
            {
                _unlockedSpells[i] = false;
            }
        }

        // Return whether a spell is unlocked.
        public bool IsSpellUnlocked(int index)
        {
            if (index >= 0 && index < _unlockedSpells.Length) return _unlockedSpells[index];
            return false;
        }

        // Get a reference to a spell by index or null if out of range.
        public Spell GetSpell(int index)
        {
            if (index >= 0 && index < _spells.Count)
            {
                return _spells[index];
            }
            return null;
        }

        // Draw each spell's visuals and icons as provided by the Spell implementations.
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var spell in _spells)
            {
                spell.Draw(spriteBatch);
            }
        }
    }
}