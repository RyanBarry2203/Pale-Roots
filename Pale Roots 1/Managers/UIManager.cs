using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This is the master drawing class for all interface elements in the game.
    // Instead of each GameState handling its own text and box math, they all pass their data here 
    // so the UI remains completely consistent across menus, levels, and transitions.
    public class UIManager
    {
        // Core drawing assets passed in from Game1.
        private Texture2D _uiPixel;
        private SpriteFont _uiFont;

        // A centralized palette for the UI. Keeping these as variables means you can easily tweak 
        // the entire game's color scheme from one spot without hunting down hardcoded values.
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

        public void DrawHUD(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ChaseAndFireEngine gameEngine, Texture2D[] spellIcons, Texture2D dashIcon, Texture2D heavyAttackIcon, int winConditionKills, float levelProgress)
        {
            Player p = gameEngine.GetPlayer();
            if (p == null) return;

            int screenW = graphicsDevice.Viewport.Width;

            // level XP bar
            // Draw a thin progress bar across the entire top edge of the screen to track level progression.
            int xpBarHeight = 12;
            spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, screenW, xpBarHeight), Color.Black * 0.8f);

            // Calculate how far across the screen the filled portion should reach based on the passed-in percentage.
            int filledWidth = (int)(screenW * levelProgress);
            spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, filledWidth, xpBarHeight), Color.DeepSkyBlue);

            // Draw a tiny lip under the XP bar to give it some separation from the gameplay.
            spriteBatch.Draw(_uiPixel, new Rectangle(0, xpBarHeight, screenW, 2), Color.DarkBlue * 0.8f);

            // --- PLAYER VITALS ---
            // Draw the Health and Stamina bars in the top left corner of the screen.
            int padding = 20;
            int startY = padding + xpBarHeight;
            int barHeight = 25;

            // Dynamically scale the width of the health bar based on the player's MaxHealth stat.
            int barWidth = (int)(300 * (p.MaxHealth / 100f));

            // Draw a semi-transparent backing plate for both vitals to sit on.
            spriteBatch.Draw(_uiPixel, new Rectangle(padding, startY, barWidth + 4, (barHeight * 2) + 15), _hudColor);

            // Calculate and draw the active health percentage.
            float hpPercent = (float)p.Health / p.MaxHealth;
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, startY + 2, barWidth, barHeight), Color.Black * 0.5f);
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, startY + 2, (int)(barWidth * hpPercent), barHeight), _healthColor);

            // Calculate the stamina/dash cooldown percentage.
            float dashRatio = 0f;
            if (p.DashDuration > 0) dashRatio = 1.0f - (p.DashTimer / p.DashDuration);
            else dashRatio = 1.0f;

            dashRatio = MathHelper.Clamp(dashRatio, 0f, 1f);
            int stamY = (padding + barHeight + 8) + 10;
            int stamH = barHeight / 2;

            // Draw the stamina bar underneath the health bar. If it's fully charged, color it bright green, otherwise keep it orange.
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, barWidth, stamH), Color.Black * 0.5f);
            Color currentStaminaColor = (dashRatio >= 0.99f) ? _staminaColor : Color.Orange;
            spriteBatch.Draw(_uiPixel, new Rectangle(padding + 2, stamY, (int)(barWidth * dashRatio), stamH), currentStaminaColor);

            // game win bar
            // Only draw this massive progress bar if we aren't in a Boss Arena (which sets winConditionKills to 999).
            if (winConditionKills < 999)
            {
                int barW = 600;
                int barH = 20;

                // Perfectly center the objective bar at the top of the screen just below the XP bar.
                int barX = (screenW / 2) - (barW / 2);
                int barY = 20;

                spriteBatch.Draw(_uiPixel, new Rectangle(barX - 2, barY - 2, barW + 4, barH + 4), Color.Black);
                spriteBatch.Draw(_uiPixel, new Rectangle(barX, barY, barW, barH), Color.DarkGray * 0.5f);

                // Track total kills against the final victory condition.
                float progress = (float)gameEngine.EnemiesKilled / winConditionKills;
                if (progress > 1f) progress = 1f;

                spriteBatch.Draw(_uiPixel, new Rectangle(barX, barY, (int)(barW * progress), barH), Color.Purple);

                // Add the menacing text right under the bar.
                string warText = "WAR DOMINANCE";
                Vector2 textSize = _uiFont.MeasureString(warText);
                spriteBatch.DrawString(_uiFont, warText, new Vector2(screenW / 2 - textSize.X / 2, barY + barH + 5), Color.White);
            }

            // ability bar
            // Dynamically build out the player's spell bar at the bottom center of the screen based on what they have unlocked.
            int iconSize = 64;
            int spacing = 20;
            int startingY = graphicsDevice.Viewport.Height - iconSize - 45;

            // First, figure out exactly how many items we need to draw so we can calculate the total width and center the entire bar.
            int unlockedSpells = 0;
            for (int i = 0; i < 6; i++) if (gameEngine.GetSpellManager().IsSpellUnlocked(i)) unlockedSpells++;

            bool showDash = p.IsDashUnlocked;
            bool showHeavy = p.IsHeavyAttackUnlocked;
            int totalItems = unlockedSpells + (showDash ? 1 : 0) + (showHeavy ? 1 : 0);

            if (totalItems > 0)
            {
                int totalWidth = (totalItems * iconSize) + ((totalItems - 1) * spacing);
                int currentX = (screenW / 2) - (totalWidth / 2);

                // Draw a dark semi-transparent box spanning the entire width of all unlocked abilities.
                Rectangle bgRect = new Rectangle(currentX - 10, startingY - 10, totalWidth + 20, iconSize + 50);
                spriteBatch.Draw(_uiPixel, bgRect, Color.Black * 0.6f);

                // Draw the Dash ability if it's unlocked.
                if (showDash)
                {
                    Rectangle dest = new Rectangle(currentX, startingY, iconSize, iconSize);
                    spriteBatch.Draw(dashIcon, dest, Color.White);

                    // Add the tiny keybinding text beneath the icon.
                    Vector2 txtSize = _uiFont.MeasureString("SHFT") * 0.7f;
                    spriteBatch.DrawString(_uiFont, "SHFT", new Vector2(dest.Center.X - txtSize.X / 2, dest.Bottom + 2), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);

                    // If the ability is on cooldown, draw a dark overlay that shrinks as the timer ticks down.
                    if (p.DashTimer > 0)
                    {
                        float ratio = p.DashTimer / p.DashDuration;
                        int h = (int)(iconSize * ratio);
                        Rectangle cdRect = new Rectangle(dest.X, dest.Bottom - h, iconSize, h);
                        spriteBatch.Draw(_uiPixel, cdRect, Color.Black * 0.7f);
                    }

                    // Nudge the X position over for the next ability in the list.
                    currentX += iconSize + spacing;
                }

                // Draw the Heavy Attack ability if it's unlocked.
                if (showHeavy)
                {
                    Rectangle dest = new Rectangle(currentX, startingY, iconSize, iconSize);
                    spriteBatch.Draw(heavyAttackIcon, dest, Color.White);

                    Vector2 txtSize = _uiFont.MeasureString("R-CLK") * 0.7f;
                    spriteBatch.DrawString(_uiFont, "R-CLK", new Vector2(dest.Center.X - txtSize.X / 2, dest.Bottom + 2), Color.Gold, 0f, Vector2.Zero, 0.7f, SpriteEffects.None, 0f);
                    currentX += iconSize + spacing;
                }

                // Loop through all 6 potential spells and draw them if they are unlocked.
                for (int i = 0; i < 6; i++)
                {
                    if (gameEngine.GetSpellManager().IsSpellUnlocked(i))
                    {
                        Rectangle dest = new Rectangle(currentX, startingY, iconSize, iconSize);

                        // Grab the specific texture for this spell index from the array passed in by Game1.
                        if (spellIcons[i] != null)
                            spriteBatch.Draw(spellIcons[i], dest, Color.White);

                        // Use the spell index to label the keybinding (1, 2, 3, etc).
                        string key = (i + 1).ToString();
                        Vector2 txtSize = _uiFont.MeasureString(key);
                        spriteBatch.DrawString(_uiFont, key, new Vector2(dest.Center.X - txtSize.X / 2, dest.Bottom + 2), Color.Gold);

                        // Grab the actual spell object to check its internal cooldown timers.
                        Spell s = gameEngine.GetSpellManager().GetSpell(i);
                        if (s != null && s.CurrentCooldown > 0)
                        {
                            // Draw the dark overlay clipping upward as the cooldown refreshes.
                            float ratio = s.CurrentCooldown / s.CooldownDuration;
                            int h = (int)(iconSize * ratio);
                            Rectangle cdRect = new Rectangle(dest.X, dest.Bottom - h, iconSize, h);
                            spriteBatch.Draw(_uiPixel, cdRect, Color.Black * 0.7f);

                            // If the cooldown is extremely long (over 1 second), draw a physical number overlaying the icon.
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

            // boss hbar
            // If the GameEngine flagged that we are in the Boss Arena, override the standard enemy floating health bars 
            // and draw a massive, menacing bar at the bottom of the screen instead.
            if (gameEngine.IsBossArena)
            {
                Enemy boss = gameEngine._enemies.Find(e => e is BlackHoleBoss);
                if (boss != null && boss.IsAlive)
                {
                    int bossBarW = 800;
                    int bossBarH = 25;

                    int bossBarX = (screenW / 2) - (bossBarW / 2);
                    int bossBarY = startingY - 60;

                    spriteBatch.Draw(_uiPixel, new Rectangle(bossBarX - 4, bossBarY - 4, bossBarW + 8, bossBarH + 8), Color.Black * 0.8f);
                    spriteBatch.Draw(_uiPixel, new Rectangle(bossBarX, bossBarY, bossBarW, bossBarH), Color.DarkRed * 0.5f);

                    float bossHpPercent = (float)boss.Health / boss.MaxHealth;
                    spriteBatch.Draw(_uiPixel, new Rectangle(bossBarX, bossBarY, (int)(bossBarW * bossHpPercent), bossBarH), Color.Red);

                    string bossName = boss.Name;
                    Vector2 nameSize = _uiFont.MeasureString(bossName);
                    spriteBatch.DrawString(_uiFont, bossName, new Vector2(screenW / 2 - nameSize.X / 2, bossBarY - nameSize.Y - 5), Color.White);
                }
            }
        }

        public void DrawMenu(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Rectangle playBtnRect, Rectangle tutorialBtnRect, Rectangle quitBtnRect, bool hasStarted)
        {
            int screenW = graphicsDevice.Viewport.Width;
            int screenH = graphicsDevice.Viewport.Height;

            // Draw a dark overlay so the menu buttons pop against whatever image or frozen game is sitting behind them.
            spriteBatch.Draw(_uiPixel, new Rectangle(0, 0, screenW, screenH), Color.Black * 0.6f);

            // Grab the mouse position so we can tell the buttons whether they should glow or not.
            Point mousePoint = new Point(Mouse.GetState().X, Mouse.GetState().Y);

            // A local helper function so we don't have to write the same 20 lines of drawing math for all three buttons.
            void DrawFancyButton(Rectangle rect, string text, bool isHovered)
            {
                Color fillColor = isHovered ? _btnHover : _btnNormal;
                Color borderColor = isHovered ? _borderHover : _borderNormal;

                spriteBatch.Draw(_uiPixel, rect, fillColor);
                int borderThickness = 2;

                // Draw four thin lines to create an inner border around the button.
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, rect.Width, borderThickness), borderColor);
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Bottom - borderThickness, rect.Width, borderThickness), borderColor);
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.X, rect.Y, borderThickness, rect.Height), borderColor);
                spriteBatch.Draw(_uiPixel, new Rectangle(rect.Right - borderThickness, rect.Y, borderThickness, rect.Height), borderColor);

                if (_uiFont != null)
                {
                    // Measure and center the actual text inside the button boundaries.
                    Vector2 size = _uiFont.MeasureString(text);
                    Vector2 pos = new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2);
                    spriteBatch.DrawString(_uiFont, text, pos, Color.White);
                }
            }

            // Draw all three buttons, passing in the hitboxes defined by the MenuState.
            bool playHover = playBtnRect.Contains(mousePoint);
            DrawFancyButton(playBtnRect, hasStarted ? "RESUME" : "PLAY", playHover);

            bool tutHover = tutorialBtnRect.Contains(mousePoint);
            DrawFancyButton(tutorialBtnRect, "TUTORIAL", tutHover);

            bool quitHover = quitBtnRect.Contains(mousePoint);
            DrawFancyButton(quitBtnRect, "QUIT", quitHover);

            // Finally, draw the massive, heavily-stylized game title at the top center of the screen.
            if (_uiFont != null)
            {
                string title = "PALE ROOTS";
                Vector2 titleSize = _uiFont.MeasureString(title);
                Vector2 titlePos = new Vector2(screenW / 2f, 200);
                Vector2 origin = titleSize / 2f;

                // Draw a black version first, offset slightly to act as a drop shadow...
                spriteBatch.DrawString(_uiFont, title, titlePos + new Vector2(3, 3), Color.Black * 0.8f, 0f, origin, 2.5f, SpriteEffects.None, 0f);
                // ...then draw the real colored title on top.
                spriteBatch.DrawString(_uiFont, title, titlePos, Color.PaleGoldenrod, 0f, origin, 2.5f, SpriteEffects.None, 0f);
            }
        }

        public void DrawLevelUpScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, List<UpgradeManager.UpgradeOption> currentUpgradeOptions, UpgradeManager upgradeManager)
        {
            // Darken the entire screen so the bright, glowing cards stand out.
            spriteBatch.Draw(_uiPixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.7f);

            string title = "ALTERNATE PHYSICS WITH THE STRENGTH OF THE DEAD";
            Vector2 size = _uiFont.MeasureString(title);
            spriteBatch.DrawString(_uiFont, title, new Vector2(graphicsDevice.Viewport.Width / 2 - size.X / 2, 100), Color.Gold);

            Rectangle screen = graphicsDevice.Viewport.Bounds;

            // Hardcode the dimensions of the cards. This must perfectly match the hitboxes generated by LevelUpState
            int cardWidth = 200;
            int cardHeight = 300;
            int spacing = 50;

            if (currentUpgradeOptions != null)
            {
                // Calculate the total width of all 3 cards so they are perfectly centered.
                int totalWidth = (currentUpgradeOptions.Count * cardWidth) + ((currentUpgradeOptions.Count - 1) * spacing);
                int startX = (screen.Width / 2) - (totalWidth / 2);
                int startY = (screen.Height / 2) - (cardHeight / 2);

                Point mousePos = Mouse.GetState().Position;

                for (int i = 0; i < currentUpgradeOptions.Count; i++)
                {
                    Rectangle cardRect = new Rectangle(startX + (i * (cardWidth + spacing)), startY, cardWidth, cardHeight);
                    bool hover = cardRect.Contains(mousePos);

                    // We hand the actual drawing logic for the cards over to the UpgradeManager, 
                    // since it already knows all the color schemes and icon data for every card type.
                    upgradeManager.DrawCard(spriteBatch, cardRect, currentUpgradeOptions[i], hover, _uiFont);
                }
            }
        }

        public void DrawEndScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, bool victory)
        {
            // Darken the background significantly.
            spriteBatch.Draw(_uiPixel, graphicsDevice.Viewport.Bounds, Color.Black * 0.85f);

            // Check the boolean passed by the EndGameState to figure out what message to show.
            string title = victory ? "VICTORY" : "YOU DIED";
            Color color = victory ? Color.Gold : Color.Red;
            Vector2 size = _uiFont.MeasureString(title);
            Vector2 center = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

            // Draw the massive message slightly above the center of the screen.
            spriteBatch.DrawString(_uiFont, title, new Vector2(center.X - size.X, center.Y - 150), color, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

            // If the player won, add an extra little bit of lore text under the title.
            if (victory)
            {
                string lore = "Your home is yours again\nYou have taken back your Throne.";
                Vector2 loreSize = _uiFont.MeasureString(lore);
                spriteBatch.DrawString(_uiFont, lore, new Vector2(center.X - loreSize.X / 2, center.Y - 80), Color.White);
            }

            // Define the visual locations for the buttons (matching the hitboxes in EndGameState).
            Rectangle btn1Rect = new Rectangle((int)center.X - 100, (int)center.Y, 200, 50);
            Rectangle btn2Rect = new Rectangle((int)center.X - 100, (int)center.Y + 70, 200, 50);
            Point mousePos = Mouse.GetState().Position;

            // A tiny local helper function to quickly draw and center these simple End Game buttons.
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

            string btn1Text = victory ? "END GAME" : "PLAY AGAIN";
            DrawBtn(btn1Rect, btn1Text);
            DrawBtn(btn2Rect, "QUIT");
        }
    }
}