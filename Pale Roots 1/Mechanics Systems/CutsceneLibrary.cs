using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Pale_Roots_1
{
    public static class CutsceneLibrary
    {
        public static void LoadAllCutscenes(CutsceneManager manager, Game game)
        {
            // --- LOAD INTRO ---
            Texture2D[] introSlides = new Texture2D[9];
            for (int i = 1; i < 9; i++) introSlides[i] = game.Content.Load<Texture2D>("cutscene_image_" + i);

            Cutscene intro = new Cutscene();
            float dur = 5500f;

            intro.AddSlide(new CutsceneSlide(introSlides[1], "Decades ago Scholars discovered that the universe was not forged from nothing,\n it was brought to fruition by beings greater than our comprehension", dur + 2000, 1.0f, 1.05f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[2], "One of these beings known as Atun created humanity, in hopes in return he would get their devoted undying love.", dur, 1.05f, 1.05f, new Vector2(-30, 0), new Vector2(30, 0)));
            intro.AddSlide(new CutsceneSlide(introSlides[3], "But soon after humanity discovered it was he who made civilisation the cruel unforgiving reality it was,\n a rancorous feeling was left souring their tounge.", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[4], "Insulted, Atun withdrew any power he was yeilding to the world he once held so precious.", dur, 1.05f, 1.05f, new Vector2(0, 30), new Vector2(0, -30)));
            intro.AddSlide(new CutsceneSlide(introSlides[5], "The Roots of his power went Pale, and the love his people had for him turned to blaising rage as\n they were forsaken further.", dur, 1.0f, 1.08f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[6], "In news of a weakened civilisation, Predatory colonys smelled blood in the waters of the Galaxy.", dur, 1.05f, 1.05f, new Vector2(30, 0), new Vector2(-30, 0)));
            intro.AddSlide(new CutsceneSlide(introSlides[7], "Led by Nivellin, a war Hero with inexplicable power. He had came to take back ther land that was once his to Rule.", dur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            intro.AddSlide(new CutsceneSlide(introSlides[8], "War was set in Motion, as was the Justice for Humanity", dur, 1.1f, 1.0f, Vector2.Zero, Vector2.Zero));

            manager.AddCutscene("Intro", intro);

            // --- LOAD OUTRO ---
            Texture2D[] outroSlides = new Texture2D[8];
            for (int i = 1; i < 8; i++) outroSlides[i] = game.Content.Load<Texture2D>("outro_image_" + i);

            Cutscene outro = new Cutscene();
            float outroDur = 6000f;

            outro.AddSlide(new CutsceneSlide(outroSlides[1], "The Skeleton King crumbles, his reign of bone and ash finally at an end.", outroDur, 1.0f, 1.15f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[2], "Without his dark magic, the Pale Roots begin to wither and retreat.", outroDur, 1.1f, 1.1f, new Vector2(-50, 0), new Vector2(50, 0)));
            outro.AddSlide(new CutsceneSlide(outroSlides[3], "Where death once choked the land, the first sprouts of green life return.", outroDur, 1.2f, 1.0f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[4], "The survivors emerge from the ruins, looking up at a clear sky for the first time in years.", outroDur, 1.1f, 1.1f, new Vector2(0, 50), new Vector2(0, -50)));
            outro.AddSlide(new CutsceneSlide(outroSlides[5], "We will rebuild. Not as subjects of a tyrant, but as free people.", outroDur, 1.0f, 1.1f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[6], "The scars of this war will remain, a reminder of what was lost.", outroDur, 1.05f, 1.15f, Vector2.Zero, Vector2.Zero));
            outro.AddSlide(new CutsceneSlide(outroSlides[7], "But today... today we celebrate the dawn.", outroDur + 3000, 1.1f, 1.1f, new Vector2(-30, -30), new Vector2(30, 30)));

            manager.AddCutscene("Outro", outro);
        }
    }
}