using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class SpellManager
    {
        private ChaseAndFireEngine _engine;
        private List<Spell> _spells = new List<Spell>();

        private bool[] _unlockedSpells;

        // --- FIX IS HERE: NO "class" KEYWORD ---
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

        public void Update(GameTime gameTime)
        {
            foreach (var spell in _spells)
            {
                spell.Update(gameTime);
            }
            HandleInput();
        }

        private void HandleInput()
        {
            KeyboardState kState = Keyboard.GetState();
            MouseState mState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mState.X, mState.Y);
            Matrix inverseTransform = Matrix.Invert(_engine._camera.CurrentCameraTranslation);
            Vector2 mousePos = Vector2.Transform(mouseScreenPos, inverseTransform);


            if (kState.IsKeyDown(Keys.D1)) CastSpell(0, mousePos);
            if (kState.IsKeyDown(Keys.D2)) CastSpell(1, mousePos);
            if (kState.IsKeyDown(Keys.D3)) CastSpell(2, mousePos);
            if (kState.IsKeyDown(Keys.D4)) CastSpell(3, mousePos);
            if (kState.IsKeyDown(Keys.D5)) CastSpell(4, mousePos);
            if (kState.IsKeyDown(Keys.D6)) CastSpell(5, mousePos);
        }

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
        public void UnlockSpell(int index)
        {
            if (index >= 0 && index < _unlockedSpells.Length)
            {
                _unlockedSpells[index] = true;
            }
        }

        public bool IsSpellUnlocked(int index)
        {
            if (index >= 0 && index < _unlockedSpells.Length) return _unlockedSpells[index];
            return false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var spell in _spells)
            {
                spell.Draw(spriteBatch);
            }
        }
    }
}