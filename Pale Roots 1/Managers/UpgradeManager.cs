using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    // This class handles the player's progression system.
    // It defines every possible power-up in the game, tracks what the player is still allowed to unlock, 
    // and randomly deals a hand of options when a level-up occurs.
    public class UpgradeManager
    {
        // Categorizes the upgrades so the system knows what specific flags or arrays to check 
        // when validating if a player already owns an ability.
        public enum UpgradeType { HeavyAttack, Dash, Spell, BossTrigger }

        // A simple data structure to hold all the text, icons, and logic for a single upgrade card.
        public class UpgradeOption
        {
            public string Name;
            public string Description;
            public Texture2D Icon;

            // This delegate holds the specific function that will fire when the player clicks this card.
            public Action ApplyAction;

            public UpgradeType Type;
            public int SpellIndex = -1;
            public Color CardHue = Color.Black;
        }

        private Player _player;
        private SpellManager _spellManager;

        // Raw texture data passed in by Game1 when the system boots up.
        private Texture2D[] _spellIcons;
        private Texture2D _dashIcon;
        private Texture2D _heavyIcon;
        private Texture2D _bossIcon;
        private Texture2D _pixel;

        // A flag to ensure the final boss trigger card doesn't accidentally show up again if the player wins.
        public bool BossDefeated { get; set; } = false;

        // The master list of every single upgrade defined in the game.
        private List<UpgradeOption> _allUpgrades = new List<UpgradeOption>();

        public UpgradeManager(Player p, SpellManager sm, Texture2D[] icons, Texture2D dashIcon, Texture2D heavyIcon, Texture2D bossIcon, GraphicsDevice gd)
        {
            _player = p;
            _spellManager = sm;
            _spellIcons = icons;
            _dashIcon = dashIcon;
            _heavyIcon = heavyIcon;
            _bossIcon = bossIcon;

            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });

            // Build the master list the moment the manager is created.
            InitializeUpgrades();
        }

        private void InitializeUpgrades()
        {
            // Manually build the Heavy Attack card.
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Heavy Attack",
                Description = "Right Click to deal double damage.",
                Type = UpgradeType.HeavyAttack,
                Icon = _heavyIcon,
                CardHue = Color.DeepSkyBlue,

                // When clicked, explicitly flip the HeavyAttack flag on the player.
                ApplyAction = () => _player.IsHeavyAttackUnlocked = true
            });

            // Manually build the Dash card.
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Dash",
                Description = "Press Shift to dodge attacks.",
                Type = UpgradeType.Dash,
                Icon = _dashIcon,
                CardHue = Color.Gold,
                ApplyAction = () => _player.IsDashUnlocked = true
            });

            // Manually build the hidden Boss Trigger card.
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Strange Omen",
                Description = "I feel something's off...do i give in?",
                Type = UpgradeType.BossTrigger,
                Icon = _bossIcon,
                ApplyAction = () =>
                {
                    // This is a complex action. We define a callback function detailing exactly 
                    // what happens when the player wins or loses the boss fight...
                    Action<bool> onBossResult = (won) =>
                    {
                        // Instantly teleport the player to the center of the standard level so they don't fall off the map.
                        _player.Position = new Vector2(1920, 1080);
                        _player.ClearExternalForces();

                        // If they won the fight, flip the global boss flag and instantly unlock every ability in the game.
                        if (won)
                        {
                            BossDefeated = true;
                            _player.IsDashUnlocked = true;
                            _player.IsHeavyAttackUnlocked = true;
                            for (int i = 0; i < 6; i++) _spellManager.UnlockSpell(i);
                        }
                        // If they died in the boss fight, strip them of all their powers and give them exactly 1 random spell.
                        else
                        {
                            _player.IsDashUnlocked = false;
                            _player.IsHeavyAttackUnlocked = false;
                            _spellManager.LockAllSpells();

                            int randomSpellIndex = CombatSystem.RandomInt(0, 6);
                            _spellManager.UnlockSpell(randomSpellIndex);
                        }
                    };

                    // then we pass that callback into the BossTransitionState and force the game to switch to it.
                    if (_player.Game is Game1 g)
                    {
                        g.StateManager.ChangeState(new BossTransitionState(g, g.GameEngine, true, false, onBossResult));
                    }
                }
            });

            // Automatically generate a card for every single spell currently registered in the SpellManager.
            // This prevents us from having to manually hardcode a new card every time we design a new spell.
            var spells = _spellManager.AllSpells;
            for (int i = 0; i < spells.Count; i++)
            {
                // We must capture the index locally so the lambda expression (ApplyAction) remembers the correct number.
                int index = i;
                Spell s = spells[i];
                s.Icon = _spellIcons[i];

                _allUpgrades.Add(new UpgradeOption
                {
                    Name = s.Name,
                    Description = s.Description,
                    Type = UpgradeType.Spell,
                    Icon = s.Icon,
                    SpellIndex = index,

                    // Pull the specific color hue defined by the spell so the card visually matches the magic.
                    CardHue = s.ThemeColor,

                    // Tell the SpellManager to flip the boolean for this specific index when clicked.
                    ApplyAction = () => _spellManager.UnlockSpell(index)
                });
            }
        }

        public List<UpgradeOption> GetRandomOptions(int count)
        {
            // First, filter the master list down to only include upgrades the player hasn't unlocked yet.
            var available = _allUpgrades.Where(u => IsUpgradeAvailable(u)).ToList();
            var rng = new Random();
            int n = available.Count;

            // Perform a Fisher-Yates shuffle to randomly order the available list.
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = available[k];
                available[k] = available[n];
                available[n] = value;
            }

            // Return only the requested number of cards (usually 3) from the top of our newly shuffled deck.
            return available.Take(count).ToList();
        }

        private bool IsUpgradeAvailable(UpgradeOption u)
        {
            // Simple checks against the player and spell manager to ensure we don't offer an ability they already own.
            if (u.Type == UpgradeType.HeavyAttack) return !_player.IsHeavyAttackUnlocked;
            if (u.Type == UpgradeType.Dash) return !_player.IsDashUnlocked;
            if (u.Type == UpgradeType.Spell) return !_spellManager.IsSpellUnlocked(u.SpellIndex);

            if (u.Type == UpgradeType.BossTrigger) return !BossDefeated;

            return true;
        }

        private string ParseText(string text, SpriteFont font, int width)
        {
            // A helper function to force paragraph text to wrap within the physical boundaries of the card.
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');

            foreach (string word in wordArray)
            {
                // If adding the next word makes the line wider than our card, insert a line break (\n).
                if (font.MeasureString(line + word).X > width)
                {
                    returnString = returnString + line + "\n";
                    line = string.Empty;
                }
                line = line + word + " ";
            }
            return returnString + line;
        }

        public List<UpgradeOption> GetAllUpgrades() => _allUpgrades;

        // Called heavily by LevelUpState and TutorialState to render the visual cards.
        public void DrawCard(SpriteBatch sb, Rectangle rect, UpgradeOption option, bool isHovered, SpriteFont font)
        {
            // Dim the card's specific theme color so it doesn't blind the player, 
            // but brighten it back up if they hover their mouse over it.
            Color baseHue = option.CardHue == Color.Black ? Color.Black : new Color(option.CardHue.R / 4, option.CardHue.G / 4, option.CardHue.B / 4, 220);
            Color bgColor = isHovered ? Color.Lerp(baseHue, Color.White, 0.2f) : baseHue;
            Color borderColor = isHovered ? Color.Cyan : (option.CardHue == Color.Black ? Color.Gray : option.CardHue);

            // Draw the solid background of the card.
            sb.Draw(_pixel, rect, bgColor);
            int b = 2;

            // Draw a quick 2-pixel border around the edges.
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, b), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - b, rect.Width, b), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, b, rect.Height), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.Right - b, rect.Y, b, rect.Height), borderColor);

            int iconSize = 64;
            int iconY = rect.Y + 30;

            // Draw the ability icon centered near the top of the card.
            if (option.Icon != null)
            {
                Rectangle iconRect = new Rectangle(rect.Center.X - (iconSize / 2), iconY, iconSize, iconSize);
                sb.Draw(option.Icon, iconRect, Color.White);
            }

            if (font != null)
            {
                string title = option.Name;
                Vector2 titleSize = font.MeasureString(title);
                float titleScale = 1.0f;

                // If the title of the ability is physically wider than the card itself, 
                // calculate a scale modifier to shrink it down until it fits.
                if (titleSize.X > rect.Width - 20) titleScale = (rect.Width - 20) / titleSize.X;

                Vector2 titlePos = new Vector2(rect.Center.X - (titleSize.X * titleScale) / 2, iconY + iconSize + 15);
                sb.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // Calculate exactly how much empty space is left beneath the title to draw the description.
                float startY = titlePos.Y + (titleSize.Y * titleScale) + 10;
                float endY = rect.Bottom - 10;
                float availableHeight = endY - startY;

                // Call our helper function to insert the \n line breaks.
                string wrappedDesc = ParseText(option.Description, font, rect.Width - 20);
                string[] lines = wrappedDesc.Split('\n');

                // Figure out how tall the entire block of text is so we can vertically center it in the remaining space.
                float totalTextHeight = lines.Length * font.LineSpacing;
                float startTextY = startY + (availableHeight / 2) - (totalTextHeight / 2);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    Vector2 lineSize = font.MeasureString(lines[i]);
                    Vector2 linePos = new Vector2(rect.Center.X - (lineSize.X / 2), startTextY + (i * font.LineSpacing));
                    sb.DrawString(font, lines[i], linePos, Color.White);
                }
            }
        }
    }
}