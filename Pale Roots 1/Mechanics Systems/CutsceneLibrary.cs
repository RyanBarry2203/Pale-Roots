using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This static helper class acts as the game's scriptwriter. 
    // It loads the raw image files, pairs them with your narrative text, sets the timings, 
    // and packages them up for the CutsceneManager to actually play.
    public static class CutsceneLibrary
    {
        public static void LoadAllCutscenes(CutsceneManager manager, Game game)
        {
            // --- LOAD INTRO ---

            // 1. Initialize an array to hold the raw texture data. 
            // We make it size 9 so we can use 1-based indexing (1 through 8) to perfectly match your file names.
            Texture2D[] introSlides = new Texture2D[9];
            for (int i = 1; i < 9; i++) introSlides[i] = game.Content.Load<Texture2D>("cutscene_image_" + i);

            Cutscene intro = new Cutscene();

            // Set a baseline duration of 7 seconds per slide.
            float dur = 7000f;

            // 2. Build out the sequence slide by slide.
            // The parameters here dictate the visual flair: 
            // (Texture, Text, Duration, Starting Zoom, Ending Zoom, Starting Pan Vector, Ending Pan Vector)
            intro.AddSlide(new CutsceneSlide(introSlides[1], "Decades ago Scholars discovered that the universe was not forged from nothing,\n it was brought to fruition by beings greater than our comprehension", dur + 2000, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));

            // Notice the pan vectors here: starting at X: -30 and sliding to X: 30 creates a slow pan to the right.
            intro.AddSlide(new CutsceneSlide(introSlides[2], "One of these beings known as Atun created humanity, in hopes in return he would get their devoted undying love.", dur, 1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));

            // Zooming from 1.1f down to 1.0f creates a slow pull-back effect.
            intro.AddSlide(new CutsceneSlide(introSlides[3], "But soon after humanity discovered it was he who made civilisation the cruel unforgiving reality it was,\n a rancorous feeling was left souring their tounge.", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            // Panning from Y: 30 to Y: -30 creates a slow pan upwards.
            intro.AddSlide(new CutsceneSlide(introSlides[4], "Insulted, Atun withdrew any power he was yeilding to the world he once held so precious.", dur, 1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));
            intro.AddSlide(new CutsceneSlide(introSlides[5], "The Roots of his power went Pale, and the love his people had for him turned to blaising rage as\n they were forsaken further.", dur, 1.0f, 1.08f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[6], "In news of a weakened civilisation, Predatory colonys smelled blood in the waters of the Galaxy.", dur, 1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));
            intro.AddSlide(new CutsceneSlide(introSlides[7], "Led by Nivellin, a war Hero with inexplicable power. He had came to take back ther land that was once his to Rule.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[8], "War was set in Motion, as was the Justice for Humanity", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            // 3. Register the completed sequence with the manager under the key "Intro".
            manager.AddCutscene("Intro", intro);


            // --- LOAD OUTRO ---

            // Works exactly the same as the Intro, pulling 7 final images for the game's conclusion.
            Texture2D[] outroSlides = new Texture2D[8];
            for (int i = 1; i < 8; i++) outroSlides[i] = game.Content.Load<Texture2D>("outro_image_" + i);

            Cutscene outro = new Cutscene();
            float outroDur = 7000f;

            outro.AddSlide(new CutsceneSlide(outroSlides[1], "Nivellins attack was Victorious thanks to his power to absorb the power of\n the dead and alternate the physical world with magic", outroDur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[2], "The specticle of his inexplicable power led to a undisputed devotion to his Leadership.", outroDur, 1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));
            outro.AddSlide(new CutsceneSlide(outroSlides[3], "However, he soon came to learn, there was an explanation to his Power.", outroDur, 1.2f, 1.0f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[4], "Nivellin was born a demi god, of imaculate conception. The son of Atun.\n But not a soul in the Galaxy new except his Mother and his Uncle.", outroDur, 1.1f, 1.1f, new Vector2(0, 50), new Vector2(0, -50)));
            outro.AddSlide(new CutsceneSlide(outroSlides[5], "The battle has been won, but not the war. Nivellin had to prepare his infantry\n for the wraith of Atun", outroDur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));

            // Adding extra time to the final slides to ensure the player has enough time to read the longer concluding thoughts.
            outro.AddSlide(new CutsceneSlide(outroSlides[6], "Nivellin had to wonder, what was this all for. He had won a dying planet in his feat of reclaiming his Throne.\n And he was about to challenge the wrath of the most powerful being in exsistance, his father.", outroDur + 2000f, 1.05f, 1.15f, Vector2.Zero, Vector2.Zero));

            // A dynamic diagonal pan for the final shot.
            outro.AddSlide(new CutsceneSlide(outroSlides[7], "Nivellin knew but one thing to be true. This was his home, and to defend it would be his responsibility. ", outroDur + 3000, 1.1f, 1.1f, new Vector2(-30, -30), new Vector2(30, 30)));

            manager.AddCutscene("Outro", outro);
        }
    }
}