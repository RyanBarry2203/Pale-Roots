using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Loads and registers the project's cutscenes into the CutsceneManager.
    // *insert pen on fire gif*
    public static class CutsceneLibrary
    {
        public static void LoadAllCutscenes(CutsceneManager manager, Game game)
        {
            // Load intro slide textures from the content pipeline.
            Texture2D[] introSlides = new Texture2D[9];
            for (int i = 1; i < 9; i++) introSlides[i] = game.Content.Load<Texture2D>("cutscene_image_" + i);

            // Build the intro cutscene by creating slides with text, durations, zoom and pan settings.
            Cutscene intro = new Cutscene();
            float dur = 7000f;

            intro.AddSlide(new CutsceneSlide(introSlides[1], "Decades ago Scholars discovered that the universe was not forged from nothing,\n it was brought to fruition by beings greater than our comprehension", dur + 2000, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[2], "One of these beings known as Atun created humanity, in hopes in return he would get their devoted undying love.", dur, 1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));
            intro.AddSlide(new CutsceneSlide(introSlides[3], "But soon after humanity discovered it was he who made civilisation the cruel unforgiving reality it was,\n a rancorous feeling was left souring their tounge.", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[4], "Insulted, Atun withdrew any power he was yeilding to the world he once held so precious.", dur, 1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));
            intro.AddSlide(new CutsceneSlide(introSlides[5], "The Roots of his power went Pale, and the love his people had for him turned to blaising rage as\n they were forsaken further.", dur, 1.0f, 1.08f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[6], "In news of a weakened civilisation, Predatory colonys smelled blood in the waters of the Galaxy.", dur, 1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));
            intro.AddSlide(new CutsceneSlide(introSlides[7], "Led by Nivellin, a war Hero with inexplicable power. He had come to take back the land that was once his to Rule.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[8], "War was set in Motion, as was the Justice for Humanity", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            // Register the intro sequence with the manager for use by IntroState.
            manager.AddCutscene("Intro", intro);

            // Load outro slide textures from the content pipeline.
            Texture2D[] outroSlides = new Texture2D[8];
            for (int i = 1; i < 8; i++) outroSlides[i] = game.Content.Load<Texture2D>("outro_image_" + i);

            // Build the outro cutscene with slides and timing.
            Cutscene outro = new Cutscene();
            float outroDur = 7000f;

            outro.AddSlide(new CutsceneSlide(outroSlides[1], "Nivellins attack was Victorious thanks to his ability to absorb the power of\n the dead and alternate the physical world with magic", outroDur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[2], "The specticle of his inexplicable power led to a undisputed devotion to his Leadership.", outroDur, 1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));
            outro.AddSlide(new CutsceneSlide(outroSlides[3], "However, he soon came to learn, there was an explanation to his Power.", outroDur, 1.2f, 1.0f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[4], "Nivellin was born a demi god, of imaculate conception. The son of Atun.\n But not a soul in the Galaxy new except his Mother and his Uncle.", outroDur, 1.1f, 1.1f, new Vector2(0, 50), new Vector2(0, -50)));
            outro.AddSlide(new CutsceneSlide(outroSlides[5], "The battle has been won, but not the war. Nivellin had to prepare his infantry\n for the wraith of Atun", outroDur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[6], "Nivellin had to wonder, what was this all for. He had won a dying planet in his feat of reclaiming his Throne.\n And he was about to challenge the wrath of the most powerful being in exsistance, his father.", outroDur + 4000f, 1.05f, 1.15f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[7], "Nivellin knew but one thing to be true. This was his home, and to defend it would be his responsibility. ", outroDur + 3000, 1.1f, 1.1f, new Vector2(-30, -30), new Vector2(30, 30)));

            // Register the outro sequence with the manager for use by OutroState.
            manager.AddCutscene("Outro", outro);
        }
    }
}