using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pale_Roots_1
{
    public class UpgradeManager
    {

        public enum UpgradeType { HeavyAttack, Dash, Spell, BossTrigger }

        public class UpgradeOption
        {
            public string Name;
            public string Description;
            public Texture2D Icon;
            public Action ApplyAction;
            public UpgradeType Type;
            public int SpellIndex = -1;
            public Color CardHue = Color.Black;
        }

        private Player _player;
        private SpellManager _spellManager;
        private Texture2D[] _spellIcons;
        private Texture2D _dashIcon;
        private Texture2D _heavyIcon;
        private Texture2D _bossIcon;
        private Texture2D _pixel;
        public bool BossDefeated { get; set; } = false;

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
                CardHue = Color.DeepSkyBlue,
                ApplyAction = () => _player.IsHeavyAttackUnlocked = true
            });

            // 2. Dash
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Dash",
                Description = "Press Shift to dodge attacks.",
                Type = UpgradeType.Dash,
                Icon = _dashIcon,
                CardHue = Color.Gold,
                ApplyAction = () => _player.IsDashUnlocked = true
            });

            // THE CURSED CARD
            _allUpgrades.Add(new UpgradeOption
            {
                Name = "Strange Omen",
                Description = "I feel something's off...do i give in?",
                Type = UpgradeType.BossTrigger,
                Icon = _bossIcon,
                ApplyAction = () =>
                {
                    Action<bool> onBossResult = (won) =>
                    {
                        _player.Position = new Vector2(1920, 1080);
                        _player.ClearExternalForces();

                        if (won)
                        {
                            BossDefeated = true;
                            _player.IsDashUnlocked = true;
                            _player.IsHeavyAttackUnlocked = true;
                            for (int i = 0; i < 6; i++) _spellManager.UnlockSpell(i);
                        }
                        else
                        {
                            _player.IsDashUnlocked = false;
                            _player.IsHeavyAttackUnlocked = false;
                            _spellManager.LockAllSpells();

                            int randomSpellIndex = CombatSystem.RandomInt(0, 6);
                            _spellManager.UnlockSpell(randomSpellIndex);
                        }
                    };

                    if (_player.Game is Game1 g)
                    {
                        g.StateManager.ChangeState(new BossTransitionState(g, g.GameEngine, true, false, onBossResult));
                    }
                }
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
                    CardHue = s.ThemeColor,
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
            if (u.Type == UpgradeType.Spell) return !_spellManager.IsSpellUnlocked(u.SpellIndex);

            if (u.Type == UpgradeType.BossTrigger) return !BossDefeated;

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
        public List<UpgradeOption> GetAllUpgrades() => _allUpgrades;
        public void DrawCard(SpriteBatch sb, Rectangle rect, UpgradeOption option, bool isHovered, SpriteFont font)
        {
            Color baseHue = option.CardHue == Color.Black ? Color.Black : new Color(option.CardHue.R / 4, option.CardHue.G / 4, option.CardHue.B / 4, 220);
            Color bgColor = isHovered ? Color.Lerp(baseHue, Color.White, 0.2f) : baseHue;
            Color borderColor = isHovered ? Color.Cyan : (option.CardHue == Color.Black ? Color.Gray : option.CardHue);

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

                string wrappedDesc = ParseText(option.Description, font, rect.Width - 20);
                string[] lines = wrappedDesc.Split('\n');

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