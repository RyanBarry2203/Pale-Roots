using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Sprites;

namespace Engines
{
    class ChaseEngine
    {
        Player p;

        SpriteBatch spriteBatch;
        private PlatformEnemy eplatformer;
        private ChasingEnemy[] chasers;
        private Game _gameOwnedBy;

        private RandomEnemy[] randomEnemies;
        private Collectible[] collectibles;

        public ChaseEngine(Game game)
        {
            // Chase engine remembers reference to the game
            _gameOwnedBy = game;
            game.IsMouseVisible = true;
            SoundEffect[] _PlayerSounds = new SoundEffect[5];
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

            for (int i = 0; i < _PlayerSounds.Length; i++)
                _PlayerSounds[i] =
                    game.Content.Load<SoundEffect>(@"Audio/PlayerDirection/" + i.ToString());


            p = new Player(game, new Texture2D[] {game.Content.Load<Texture2D>(@"Images/left"),
                                                game.Content.Load<Texture2D>(@"Images/right"),
                                                game.Content.Load<Texture2D>(@"Images/up"),
                                                game.Content.Load<Texture2D>(@"Images/down"),
                                                game.Content.Load<Texture2D>(@"Images/stand")},
                _PlayerSounds,
                    new Vector2(200, 200), 8, 0, 5.0f);

            eplatformer = new PlatformEnemy(game,
                        game.Content.Load<Texture2D>(@"Images/chaser"), new Vector2(100, 100),
                        new Vector2(300, 100), 1);
            chasers = new ChasingEnemy[Utility.NextRandom(2, 5)];

            for (int i = 0; i < chasers.Count(); i++)
            {
                chasers[i] = new ChasingEnemy(game,
                        game.Content.Load<Texture2D>(@"Images/chaser"),
                        new Vector2(Utility.NextRandom(game.GraphicsDevice.Viewport.Width),
                            Utility.NextRandom(game.GraphicsDevice.Viewport.Height)),
                         1);
                chasers[i].Velocity = (float)Utility.NextRandom(2, 5);
                chasers[i].CollisionDistance = Utility.NextRandom(1, 3);
            }

            randomEnemies = new RandomEnemy[5];

            for (int i = 0; i < randomEnemies.Length; i++)
            {
                randomEnemies[i] = new RandomEnemy(
                    game,
                    game.Content.Load<Texture2D>(@"Images/chaser"),
                    new Vector2(
                        Utility.NextRandom(game.GraphicsDevice.Viewport.Width),
                        Utility.NextRandom(game.GraphicsDevice.Viewport.Height)
                    ),
                    1
                );
            }
            collectibles = new Collectible[5];
            for (int i = 0; i < collectibles.Length; i++)
            {
                collectibles[i] = new Collectible(
                    _gameOwnedBy,
                    _gameOwnedBy.Content.Load<Texture2D>(@"Images/Collectable"),
                    new Vector2(
                        Utility.NextRandom(_gameOwnedBy.GraphicsDevice.Viewport.Width),
                        Utility.NextRandom(_gameOwnedBy.GraphicsDevice.Viewport.Height)
                    ),
                    6

                );
            }


        }


        public void Update(GameTime gameTime)
        {
            p.Update(gameTime);

            // Chasers
            for (int i = 0; i < chasers.Length; i++)
            {
                var c = chasers[i];
                if (c == null) continue;

                c.follow(p);
                c.Update(gameTime);

                if (Vector2.Distance(c.position, p.position) < 30f)
                {
                    p.Healthbar.health -= 10;
                    p.ResetPosition();
                    chasers[i] = null;   // <-- becomes null
                }
            }

            // Random enemies
            for (int i = 0; i < randomEnemies.Length; i++)
            {
                var r = randomEnemies[i];
                if (r == null) continue;

                r.Update(gameTime);

                if (Vector2.Distance(r.position, p.position) < 30f)
                {
                    p.Healthbar.health -= 10;
                    p.ResetPosition();
                    randomEnemies[i] = null;   // <-- becomes null
                }
            }
            if (eplatformer != null)
            {
                eplatformer.Update(gameTime);

                if (Vector2.Distance(eplatformer.position, p.position) < 30f)
                {
                    p.Healthbar.health -= 10;
                    p.ResetPosition();
                    eplatformer = null; // destroy the platform enemy
                }
            }

            foreach (var c in collectibles)
                c?.Update(gameTime);

            // Collectibles
            for (int i = 0; i < collectibles.Length; i++)
            {
                var c = collectibles[i];
                if (c == null) continue;

                c.Update(gameTime);
            }


            for (int i = 0; i < collectibles.Length; i++)
            {
                var c = collectibles[i];
                if (c == null)
                    continue; // collectible already gone

                // Check collision using distance (adjust number if needed)
                if (Vector2.Distance(c.position, p.position) < 30f)
                {
                    // Increase player's health
                    p.Healthbar.health += c.HealthValue;

                    // Cap health at max
                    if (p.Healthbar.health > 100)
                        p.Healthbar.health = 100;

                    // Remove collectible from the world
                    collectibles[i] = null;
                }
            }
            
            for (int i = 0; i < collectibles.Length; i++)
            {
                var c = collectibles[i];
                if (c == null) continue;

                // Check collision with chasers
                foreach (var ch in chasers)
                {
                    if (ch == null) continue;

                    if (Vector2.Distance(ch.position, c.position) < 30f)
                    {
                        collectibles[i] = null;   // destroy collectible
                        break; // stop checking this collectible
                    }
                }

                // Check collision with random enemies
                foreach (var r in randomEnemies)
                {
                    if (r == null) continue;

                    if (Vector2.Distance(r.position, c.position) < 30f)
                    {
                        collectibles[i] = null;
                        break;
                    }
                }

                // Check collision with platform enemy
                if (eplatformer != null)
                {
                    if (Vector2.Distance(eplatformer.position, c.position) < 30f)
                    {
                        collectibles[i] = null;
                    }
                }
            }



            //eplatformer.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            p.Draw(spriteBatch);
            p.Healthbar.draw(spriteBatch);

            if (eplatformer != null)
                eplatformer.Draw(spriteBatch);

            foreach (var chaser in chasers)
                if (chaser != null)
                    chaser.Draw(spriteBatch);

            foreach (var r in randomEnemies)
                if (r != null)
                    r.Draw(spriteBatch);

            foreach (var c in collectibles)
                if (c != null)
                    c.Draw(spriteBatch);

            spriteBatch.End();




        }
    }
}
