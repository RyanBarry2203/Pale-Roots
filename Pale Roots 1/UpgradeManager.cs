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
                "Heal yourself.",
                "Explosion around you.",
                "Random holy strikes.",
                "Buff allies.",
                "Stun enemies.",
                "Double damage buff."
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
            // Card Background
            Color bgColor = isHovered ? Color.DarkSlateBlue : Color.Black * 0.9f;
            Color borderColor = isHovered ? Color.Cyan : Color.Gray;

            sb.Draw(_pixel, rect, bgColor);

            // Border
            int border = 2;
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, rect.Width, border), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Bottom - border, rect.Width, border), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.X, rect.Y, border, rect.Height), borderColor);
            sb.Draw(_pixel, new Rectangle(rect.Right - border, rect.Y, border, rect.Height), borderColor);

            // --- LAYOUT ADJUSTMENTS ---

            // 1. Icon (Moved UP slightly)
            if (option.Icon != null)
            {
                Rectangle iconRect = new Rectangle(rect.Center.X - 32, rect.Y + 20, 64, 64);
                sb.Draw(option.Icon, iconRect, Color.White);
            }

            if (font != null)
            {
                // 2. Title (Moved DOWN to clear the icon)
                // Icon ends at Y+84 (20+64). Let's put text at Y+95.
                Vector2 nameSize = font.MeasureString(option.Name);
                Vector2 namePos = new Vector2(rect.Center.X - nameSize.X / 2, rect.Y + 95);
                sb.DrawString(font, option.Name, namePos, Color.Gold);

                // 3. Description (Moved DOWN to clear the title)
                // Title is roughly 20-30px high. Let's put description at Y+135.
                string wrappedDesc = ParseText(option.Description, font, rect.Width - 20);
                Vector2 descSize = font.MeasureString(wrappedDesc);
                Vector2 descPos = new Vector2(rect.Center.X - descSize.X / 2, rect.Y + 135);

                sb.DrawString(font, wrappedDesc, descPos, Color.White, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            }
        }
    }
}