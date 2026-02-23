using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    public class UpgradeManager
    {
        public enum UpgradeType { HeavyAttack, Dash, Spell }

        public class UpgradeOption
        {
            public string Name;
            public string Description;
            public Texture2D Icon;
            public Action ApplyAction;
            public UpgradeType Type;
            public int SpellIndex = -1;
        }

        private Player _player;
        private SpellManager _spellManager;
        private Texture2D[] _spellIcons;
        private Texture2D _dashIcon;
        private Texture2D _heavyIcon;
        private Texture2D _pixel;

        private List<UpgradeOption> _allUpgrades = new List<UpgradeOption>();

        public UpgradeManager(Player p, SpellManager sm, Texture2D[] icons, Texture2D dashIcon, Texture2D heavyIcon, GraphicsDevice gd)
        {
            _player = p;
            _spellManager = sm;
            _spellIcons = icons;
            _dashIcon = dashIcon;
            _heavyIcon = heavyIcon;

            _pixel = new Texture2D(gd, 1, 1);
            _pixel.SetData(new[] { Color.White });

            InitializeUpgrades();
        }

        private void InitializeUpgrades()
        {
            // 1. Heavy Attack
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Heavy Attack",
                Description = "Right Click to deal double damage.",
                Type = UpgradeType.HeavyAttack,
                Icon = _heavyIcon,
                ApplyAction = () => _player.IsHeavyAttackUnlocked = true
            });

            // 2. Dash
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Dash",
                Description = "Press Shift to dodge attacks.",
                Type = UpgradeType.Dash,
                Icon = _dashIcon,
                ApplyAction = () => _player.IsDashUnlocked = true
            });

            // 3. DYNAMIC SPELL GENERATION (The Engine Flex!)
            var spells = _spellManager.AllSpells;
            for (int i = 0; i < spells.Count; i++)
            {
                int index = i; // Capture for lambda
                Spell s = spells[i];
                s.Icon = _spellIcons[i]; // Bind the UI icon directly to the spell object

                _allUpgrades.Add(new UpgradeOption
                {
                    Name = s.Name,
                    Description = s.Description,
                    Type = UpgradeType.Spell,
                    Icon = s.Icon,
                    SpellIndex = index,
                    ApplyAction = () => _spellManager.UnlockSpell(index)
                });
            }
        }

        public List<UpgradeOption> GetRandomOptions(int count)
        {
            var available = _allUpgrades.Where(u => IsUpgradeAvailable(u)).ToList();
            var rng = new Random();
            int n = available.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = available[k];
                available[k] = available[n];
                available[n] = value;
            }
            return available.Take(count).ToList();
        }

        private bool IsUpgradeAvailable(UpgradeOption u)
        {
            if (u.Type == UpgradeType.HeavyAttack) return !_player.IsHeavyAttackUnlocked;
            if (u.Type == UpgradeType.Dash) return !_player.IsDashUnlocked;

            // Look how brilliantly simple this is now! No more Array.IndexOf hacks!
            if (u.Type == UpgradeType.Spell) return !_spellManager.IsSpellUnlocked(u.SpellIndex);

            return true;
        }

        private string ParseText(string text, SpriteFont font, int width)
        {
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');
            foreach (string word in wordArray)
            {
                if (font.MeasureString(line + word).X > width)
                {
                    returnString = returnString + line + "\n";
                    line = string.Empty;
                }
                line = line + word + " ";
            }
            return returnString + line;
        }

        public void DrawCard(SpriteBatch sb, Rectangle rect, UpgradeOption option, bool isHovered, SpriteFont font)
        {
            Color bgColor = isHovered ? Color.DarkSlateBlue : Color.Black * 0.9f;
            Color borderColor = isHovered ? Color.Cyan : Color.Gray;

            sb.Draw(_pixel, rect, bgColor);
            int b = 2;
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, b), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - b, rect.Width, b), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, b, rect.Height), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.Right - b, rect.Y, b, rect.Height), borderColor);

            int iconSize = 64;
            int iconY = rect.Y + 30;

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
                if (titleSize.X > rect.Width - 20) titleScale = (rect.Width - 20) / titleSize.X;

                Vector2 titlePos = new Vector2(rect.Center.X - (titleSize.X * titleScale) / 2, iconY + iconSize + 15);
                sb.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                float startY = titlePos.Y + (titleSize.Y * titleScale) + 10;
                float endY = rect.Bottom - 10;
                float availableHeight = endY - startY;

                string wrappedDesc = ParseText(option.Description, font, rect.Width - 30);
                Vector2 descSize = font.MeasureString(wrappedDesc);
                float descY = startY + (availableHeight / 2) - (descSize.Y / 2);
                Vector2 descPos = new Vector2(rect.Center.X - descSize.X / 2, descY);

                sb.DrawString(font, wrappedDesc, descPos, Color.White);
            }
        }
    }
}