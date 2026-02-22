using System.Collections.Generic;

namespace Pale_Roots_1
{
    public class Cutscene
    {
        public List<CutsceneSlide> Slides { get; private set; } = new List<CutsceneSlide>();

        public void AddSlide(CutsceneSlide slide)
        {
            Slides.Add(slide);
        }
    }
}