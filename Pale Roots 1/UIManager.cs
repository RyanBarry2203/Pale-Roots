using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class UIManager
    {
        private Texture2D _uiPixel;
        private SpriteFont _uiFont;

        // UI Colors (Medieval Sci-Fantasy Theme)
        private Color _hudColor = new Color(10, 10, 15, 200);
        private Color _healthColor = new Color(180, 20, 20);
        private Color _staminaColor = new Color(50, 205, 50);
        private Color _btnNormal = new Color(20, 20, 30, 220);
        private Color _btnHover = new Color(40, 60, 100, 240);
        private Color _borderNormal = new Color(60, 60, 80);
        private Color _borderHover = Color.Cyan;

        public UIManager(Texture2D uiPixel, SpriteFont uiFont)
        {
            _uiPixel = uiPixel;
            _uiFont = uiFont;
        }

        public void DrawHUD(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ChaseAndFireEngine gameEngine, Texture2D[] spellIcons, Texture2D dashIcon, Texture2D heavyAttackIcon, int winConditionKills)
        {
            Player p = gameEngine.GetPlayer();
            if (p == null) return;

            int padding = 20;
            int barHeight = 25;
            int barWidth = 300;

            spriteBatch.Draw(_uiPixel, new Rectangle(padding, padding, barWidth + 4, (barHeight * 2) + 15), _hudColor);

            float hpPercent = (float)p.Health / p.MaxHealth;
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, barWidth, barHeight), Color.Black * 0.5f);
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, padding + 2, (int)(barWidth * hpPercent), barHeight), _healthColor);

            float dashRatio = 0f;
            if (p.DashDuration > 0) dashRatio = 1.0f - (p.DashTimer / p.DashDuration);
            else dashRatio = 1.0f;

            dashRatio = MathHelper.Clamp(dashRatio, 0f, 1f);
            int stamY = padding + barHeight + 8;
            int stamH = barHeight / 2;

            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, barWidth, stamH), Color.Black * 0.5f);
            Color currentStaminaColor = (dashRatio >= 0.99f) ? _staminaColor : Color.Orange;
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, (int)(barWidth * dashRatio), stamH), currentStaminaColor);

            int screenW = graphicsDevice.Viewport.Width;
            int barW = 600;
            int barH = 20;
            int barX = (screenW / 2) - (barW / 2);
            int barY = 20;

            spriteBatch.Draw(_uiPixel, new Rectangle(barX - 2, barY - 2, barW + 4, barH + 4), Color.Black);
            spriteBatch.Draw(_uiPixel, new Rectangle(barX, barY, barW, barH), Color.DarkGray * 0.5f);

            float progress = (float)gameEngine.EnemiesKilled / winConditionKills;
            if (progress > 1f) progress = 1f;

            spriteBatch.Draw(_uiPixel, new Rectangle(barX, barY, (int)(barW * progress), barH), Color.Purple);

            string warText = "WAR DOMINANCE";
            Vector2 textSize = _uiFont.MeasureString(warText);
            spriteBatch.DrawString(_uiFont, warText, new Vector2(screenW / 2 - textSize.X / 2, barY + barH + 5), Color.White);

            int iconSize = 64;
            int spacing = 20;
            int startY = graphicsDevice.Viewport.Height - iconSize - 20;
            int unlockedSpells = 0;

            for (int i = 0; i < 6; i++) if (gameEngine.GetSpellManager().IsSpellUnlocked(i)) unlockedSpells++;

            bool showDash = p.IsDashUnlocked;
            bool showHeavy = p.IsHeavyAttackUnlocked;
            int totalItems = unlockedSpells + (showDash ? 1 : 0) + (showHeavy ? 1 : 0);

            if (totalItems > 0)
            {
                int totalWidth = (totalItems * iconSize) + ((totalItems - 1) * spacing);
                int currentX = (screenW / 2) - (totalWidth / 2);

                Rectangle bgRect = new Rectangle(currentX - 10, startY - 10, totalWidth + 20, iconSize + 20);
                spriteBatch.Draw(_uiPixel, bgRect, Color.Black * 0.6f);

                if (showDash)
                {
                    Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);
                    spriteBatch.Draw(dashIcon, dest, Color.White);
                    spriteBatch.DrawString(_uiFont, "SHFT", new Vector2(dest.X + 2, dest.Y + 2), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);

                    if (p.DashTimer > 0)
                    {
                        float ratio = p.DashTimer / p.DashDuration;
                        int h = (int)(iconSize * ratio);
                        Rectangle cdRect = new Rectangle(dest.X, dest.Bottom - h, iconSize, h);
                        spriteBatch.Draw(_uiPixel, cdRect, Color.Black * 0.7f);
                    }
                    currentX += iconSize + spacing;
                }

                if (showHeavy)
                {
                    Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);
                    spriteBatch.Draw(heavyAttackIcon, dest, Color.White);
                    spriteBatch.DrawString(_uiFont, "R-CLK", new Vector2(dest.X + 2, dest.Y + 2), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    currentX += iconSize + spacing;
                }

                for (int i = 0; i < 6; i++)
                {
                    if (gameEngine.GetSpellManager().IsSpellUnlocked(i))
                    {
                        Rectangle dest = new Rectangle(currentX, startY, iconSize, iconSize);
                        if (spellIcons[i] != null)
                            spriteBatch.Draw(spellIcons[i], dest, Color.White);

                        string key = (i + 1).ToString();
                        spriteBatch.DrawString(_uiFont, key, new Vector2(dest.X + 2, dest.Y + 2), Color.Gold);

                        Spell s = gameEngine.GetSpellManager().GetSpell(i);
                        if (s != null && s.CurrentCooldown > 0)
                        {
                            float ratio = s.CurrentCooldown / s.CooldownDuration;
                            int h = (int)(iconSize * ratio);
                            Rectangle cdRect = new Rectangle(dest.X, dest.Bottom - h, iconSize, h);
                            spriteBatch.Draw(_uiPixel, cdRect, Color.Black * 0.7f);

                            if (s.CurrentCooldown > 1000)
                            {
                                string sec = (s.CurrentCooldown / 1000).ToString("0");
                                Vector2 sz = _uiFont.MeasureString(sec);
                                spriteBatch.DrawString(_uiFont, sec, new Vector2(dest.Center.X - sz.X / 2, dest.Center.Y - sz.Y / 2), Color.White);
                            }
                        }
                        currentX += iconSize + spacing;
                    }
                }
            }
        }

        public void DrawMenu(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Rectangle playBtnRect, Rectangle quitBtnRect, bool hasStarted)
        {
            int screenW = graphicsDevice.Viewport.Width;
            int screenH = graphicsDevice.Viewport.Height;

            spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.6f);
            Point mousePoint = new Point(Mouse.GetState().X, Mouse.GetState().Y);

            void DrawFancyButton(Rectangle rect, string text, bool isHovered)
            {
                Color fillColor = isHovered ? _btnHover : _btnNormal;
                Color borderColor = isHovered ? _borderHover : _borderNormal;

                spriteBatch.Draw(_uiPixel, rect, fillColor);
                int borderThickness = 2;

                spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);

                if (_uiFont != null)
                {
                    Vector2 size = _uiFont.MeasureString(text);
                    Vector2 pos = new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2);
                    spriteBatch.DrawString(_uiFont, text, pos, Color.White);
                }
            }

            bool playHover = playBtnRect.Contains(mousePoint);
            DrawFancyButton(playBtnRect, hasStarted ? "RESUME" : "PLAY", playHover);

            bool quitHover = quitBtnRect.Contains(mousePoint);
            DrawFancyButton(quitBtnRect, "QUIT", quitHover);

            if (_uiFont != null)
            {
                string title = "PALE ROOTS";
                Vector2 titleSize = _uiFont.MeasureString(title);
                Vector2 titlePos = new Vector2(screenW / 2f, 200);
                Vector2 origin = titleSize / 2f;

                spriteBatch.DrawString(_uiFont, title, titlePos + new Vector2(3, 3), Color.Black * 0.8f, 0f, origin, 2.5f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(_uiFont, title, titlePos, Color.PaleGoldenrod, 0f, origin, 2.5f, SpriteEffects.None, 0f);
            }
        }

        public void DrawLevelUpScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<UpgradeManager.UpgradeOption> currentUpgradeOptions, UpgradeManager upgradeManager)
        {
            spriteBatch.Draw(_uiPixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.7f);

            string title = "ALTERNATE PHYSICS WITH THE STRENGTH OF THE DEAD";
            Vector2 size = _uiFont.MeasureString(title);
            spriteBatch.DrawString(_uiFont, title, new Vector2(graphicsDevice.Viewport.Width / 2 - size.X / 2, 100), Color.Gold);

            Rectangle screen = graphicsDevice.Viewport.Bounds;
            int cardWidth = 200;
            int cardHeight = 300;
            int spacing = 50;

            if (currentUpgradeOptions != null)
            {
                int totalWidth = (currentUpgradeOptions.Count * cardWidth) + ((currentUpgradeOptions.Count - 1) * spacing);
                int startX = (screen.Width / 2) - (totalWidth / 2);
                int startY = (screen.Height / 2) - (cardHeight / 2);
                Point mousePos = Mouse.GetState().Position;

                for (int i = 0; i < currentUpgradeOptions.Count; i++)
                {
                    Rectangle cardRect = new Rectangle(startX + (i * (cardWidth + spacing)), startY, cardWidth, cardHeight);
                    bool hover = cardRect.Contains(mousePos);
                    upgradeManager.DrawCard(spriteBatch, cardRect, currentUpgradeOptions[i], hover, _uiFont);
                }
            }
        }

        public void DrawEndScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, bool victory)
        {
            spriteBatch.Draw(_uiPixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.85f);

            string title = victory ? "VICTORY" : "YOU DIED";
            Color color = victory ? Color.Gold : Color.Red;
            Vector2 size = _uiFont.MeasureString(title);
            Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

            spriteBatch.DrawString(_uiFont, title, new Vector2(center.X - size.X, center.Y - 150), color, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

            if (victory)
            {
                string lore = "The Skeleton King is defeated.\nThe Pale Roots recede.";
                Vector2 loreSize = _uiFont.MeasureString(lore);
                spriteBatch.DrawString(_uiFont, lore, new Vector2(center.X - loreSize.X / 2, center.Y - 80), Color.White);
            }

            Rectangle btn1Rect = new Rectangle((int)center.X - 100, (int)center.Y, 200, 50);
            Rectangle btn2Rect = new Rectangle((int)center.X - 100, (int)center.Y + 70, 200, 50);
            Point mousePos = Mouse.GetState().Position;

            void DrawBtn(Rectangle r, string t)
            {
                bool hover = r.Contains(mousePos);
                spriteBatch.Draw(_uiPixel, r, hover ? Color.Gray : Color.DarkGray);

                int b = 2;
                Color bc = hover ? Color.White : Color.Black;

                spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Y, r.Width, b), bc);
                spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Bottom - b, r.Width, b), bc);
                spriteBatch.Draw(_uiPixel, new Rectangle(r.X, r.Y, b, r.Height), bc);
                spriteBatch.Draw(_uiPixel, new Rectangle(r.Right - b, r.Y, b, r.Height), bc);

                Vector2 ts = _uiFont.MeasureString(t);
                spriteBatch.DrawString(_uiFont, t, new Vector2(r.Center.X - ts.X / 2, r.Center.Y - ts.Y / 2), Color.White);
            }

            string btn1Text = victory ? "FINISH GAME" : "PLAY AGAIN";
            DrawBtn(btn1Rect, btn1Text);
            DrawBtn(btn2Rect, "QUIT");
        }
    }
}