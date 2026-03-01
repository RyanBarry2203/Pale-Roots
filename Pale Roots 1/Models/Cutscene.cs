using System.Collections.Generic;

namespace Pale_Roots_1
{
    // Container for a sequence of slides used in a cinematic cutscene.
    public class Cutscene
    {
        // Ordered list of slides that the CutsceneManager will play.
        public List<CutsceneSlide> Slides { get; private set; } = new List<CutsceneSlide>();

        // Append a slide to the end of the sequence.
        public void AddSlide(CutsceneSlide slide)
        {
            Slides.Add(slide);
        }
    }
}