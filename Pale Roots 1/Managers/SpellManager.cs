using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class sits inside the GameEngine and acts as the central hub for all magical abilities.
    // It handles the input for casting, tracks which spells the player has unlocked, 
    // and loops through the active spells to update their individual timers and draw their particles.
    public class SpellManager
    {
        private ChaseAndFireEngine _engine;

        // A master list of all the instantiated spell objects in the game.
        private List<Spell> _spells = new List<Spell>();

        // A simple parallel array tracking whether the player is currently allowed to cast the spell at the corresponding index.
        private bool[] _unlockedSpells;
        public List<Spell> AllSpells => _spells;

        // The constructor expects the raw textures for every spell so it can build them all exactly once when the engine starts.
        public SpellManager(ChaseAndFireEngine engine,
                            Texture2D smiteTx,
                            Texture2D novaTx,
                            Texture2D furyTx,
                            Texture2D shieldTx,
                            Texture2D electricTx,
                            Texture2D justiceTx)
        {
            _engine = engine;

            // Instantiate each specific spell class, passing in the core game reference and its visual asset.
            _spells.Add(new SmiteSpell(engine._gameOwnedBy, smiteTx));
            _spells.Add(new HolyNovaSpell(engine._gameOwnedBy, novaTx));
            _spells.Add(new HeavensFurySpell(engine._gameOwnedBy, furyTx));
            _spells.Add(new HolyShieldSpell(engine._gameOwnedBy, shieldTx));
            _spells.Add(new ElectricitySpell(engine._gameOwnedBy, electricTx));
            _spells.Add(new SwordOfJusticeSpell(engine._gameOwnedBy, justiceTx));

            // Initialize the tracking array to the same length as the spell list. 
            // Booleans default to false, so all spells start locked.
            _unlockedSpells = new bool[_spells.Count];
        }

        public void Update(GameTime gameTime)
        {
            // First, step the internal logic of every single spell forward.
            // Even if a spell isn't actively firing, it needs to update its cooldown timers.
            foreach (var spell in _spells)
            {
                spell.Update(gameTime);
            }

            // Next, listen for the player's casting inputs.
            HandleInput();
        }

        private void HandleInput()
        {
            // Grab the raw X/Y position of the mouse on the player's physical monitor.
            MouseState mState = Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mState.X, mState.Y);

            // Because the camera is moving around a massive level, we have to translate those raw screen pixels 
            // into actual coordinates within the game world so the spell knows where to spawn.
            Matrix inverseTransform = Matrix.Invert(_engine._camera.CurrentCameraTranslation);
            Vector2 mousePos = Vector2.Transform(mouseScreenPos, inverseTransform);

            // Check our custom input wrapper to see if the player pressed any of the mapped spell casting keys.
            // If they did, pass the index of that spell and the calculated world position of the mouse.
            if (InputEngine.IsActionPressed("CastSpell1")) CastSpell(0, mousePos);
            if (InputEngine.IsActionPressed("CastSpell2")) CastSpell(1, mousePos);
            if (InputEngine.IsActionPressed("CastSpell3")) CastSpell(2, mousePos);
            if (InputEngine.IsActionPressed("CastSpell4")) CastSpell(3, mousePos);
            if (InputEngine.IsActionPressed("CastSpell5")) CastSpell(4, mousePos);
            if (InputEngine.IsActionPressed("CastSpell6")) CastSpell(5, mousePos);
        }

        private void CastSpell(int index, Vector2 target)
        {
            // Safety check: ensure the requested index actually exists in our list.
            if (index >= 0 && index < _spells.Count)
            {
                // Check if the player has actually found the upgrade card for this spell yet.
                if (_unlockedSpells[index])
                {
                    // If everything is good, tell the specific spell object to execute its casting logic.
                    _spells[index].Cast(_engine, target);
                }
            }
        }

        // These are called by the UpgradeManager when the player selects a spell card during a level up.

        public void UnlockSpell(int index)
        {
            if (index >= 0 && index < _unlockedSpells.Length)
            {
                _unlockedSpells[index] = true;
            }
        }

        public void LockAllSpells()
        {
            // Used when hard-resetting the game session after a death or victory.
            for (int i = 0; i < _unlockedSpells.Length; i++)
            {
                _unlockedSpells[i] = false;
            }
        }

        public bool IsSpellUnlocked(int index)
        {
            // Used by the UIManager to figure out if it should draw the spell icon or a blank box.
            if (index >= 0 && index < _unlockedSpells.Length) return _unlockedSpells[index];
            return false;
        }

        public Spell GetSpell(int index)
        {
            // Used by the UIManager to fetch the specific spell object so it can check its cooldown timers.
            if (index >= 0 && index < _spells.Count)
            {
                return _spells[index];
            }
            return null;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Loop through all instantiated spells and ask them to draw any active visual effects 
            // (like explosions or floating shields) to the screen.
            foreach (var spell in _spells)
            {
                spell.Draw(spriteBatch);
            }
        }
    }
}