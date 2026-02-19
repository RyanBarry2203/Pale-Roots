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
        }

        private Player _player;
        private SpellManager _spellManager;
        private Texture2D[] _spellIcons;
        private Texture2D _pixel; // For card backgrounds

        // All possible upgrades in the game
        private List<UpgradeOption> _allUpgrades = new List<UpgradeOption>();

        public UpgradeManager(Player p, SpellManager sm, Texture2D[] icons, GraphicsDevice gd)
        {
            _player = p;
            _spellManager = sm;
            _spellIcons = icons;

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
                Icon = null, // You can add a specific icon later if you want
                ApplyAction = () => _player.IsHeavyAttackUnlocked = true
            });

            // 2. Dash
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Dash",
                Description = "Press Shift to dodge attacks.",
                Type = UpgradeType.Dash,
                Icon = null,
                ApplyAction = () => _player.IsDashUnlocked = true
            });

            // 3. Spells (Indices 0 to 5 based on SpellManager)
            string[] spellNames = { "Smite", "Holy Nova", "Heaven's Fury", "Holy Shield", "Electricity", "Sword of Justice" };
            string[] spellDescs = {
                "Heal yourself to full HP.",
                "Instakill AOE on your cursor",
                "Half Enemy Healthbars",
                "Double Ally Health",
                "Stun Enemies and stop spawning",
                "Double Damage"
            };

            for (int i = 0; i < 6; i++)
            {
                int index = i; // Capture for lambda
                _allUpgrades.Add(new UpgradeOption
                {
                    Name = spellNames[i],
                    Description = spellDescs[i],
                    Type = UpgradeType.Spell,
                    Icon = _spellIcons[i],
                    ApplyAction = () => _spellManager.UnlockSpell(index)
                });
            }
        }

        public List<UpgradeOption> GetRandomOptions(int count)
        {
            // Filter out upgrades we already have
            var available = _allUpgrades.Where(u => IsUpgradeAvailable(u)).ToList();

            // Shuffle
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

            // Take top 3 (or fewer if we are running out)
            return available.Take(count).ToList();
        }

        private bool IsUpgradeAvailable(UpgradeOption u)
        {
            if (u.Type == UpgradeType.HeavyAttack) return !_player.IsHeavyAttackUnlocked;
            if (u.Type == UpgradeType.Dash) return !_player.IsDashUnlocked;

            // For spells, we need to check if the specific spell index is unlocked
            // We can infer the index from the name or store it. 
            // Since we initialized them in order 0-5:
            if (u.Type == UpgradeType.Spell)
            {
                // Find index in our spell list
                string[] spellNames = { "Smite", "Holy Nova", "Heaven's Fury", "Holy Shield", "Electricity", "Sword of Justice" };
                int idx = Array.IndexOf(spellNames, u.Name);
                return !_spellManager.IsSpellUnlocked(idx);
            }
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
            // 1. Background & Border
            Color bgColor = isHovered ? Color.DarkSlateBlue : Color.Black * 0.9f;
            Color borderColor = isHovered ? Color.Cyan : Color.Gray;

            sb.Draw(_pixel, rect, bgColor);

            int b = 2;
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, b), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - b, rect.Width, b), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, b, rect.Height), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.Right - b, rect.Y, b, rect.Height), borderColor);

            // 2. Icon (Fixed Position at top)
            int iconSize = 64;
            int iconY = rect.Y + 30;
            if (option.Icon != null)
            {
                Rectangle iconRect = new Rectangle(rect.Center.X - (iconSize / 2), iconY, iconSize, iconSize);
                sb.Draw(option.Icon, iconRect, Color.White);
            }

            if (font != null)
            {
                // 3. Title (Centered below icon)
                string title = option.Name;
                Vector2 titleSize = font.MeasureString(title);
                // Ensure title fits width, scale down if massive
                float titleScale = 1.0f;
                if (titleSize.X > rect.Width - 20) titleScale = (rect.Width - 20) / titleSize.X;

                Vector2 titlePos = new Vector2(
                    rect.Center.X - (titleSize.X * titleScale) / 2,
                    iconY + iconSize + 15
                );

                sb.DrawString(font, title, titlePos, Color.Gold, 0f, Vector2.Zero, titleScale, SpriteEffects.None, 0f);

                // 4. Description (Perfectly Centered in remaining space)
                // Calculate remaining vertical space
                float startY = titlePos.Y + (titleSize.Y * titleScale) + 10;
                float endY = rect.Bottom - 10;
                float availableHeight = endY - startY;

                // Wrap text
                string wrappedDesc = ParseText(option.Description, font, rect.Width - 30);
                Vector2 descSize = font.MeasureString(wrappedDesc);

                // Center vertically in the available space
                float descY = startY + (availableHeight / 2) - (descSize.Y / 2);

                // Center horizontally
                Vector2 descPos = new Vector2(rect.Center.X - descSize.X / 2, descY);

                sb.DrawString(font, wrappedDesc, descPos, Color.White);
            }
        }
    }
}