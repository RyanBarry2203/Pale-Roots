using System.Collections.Generic;

namespace Pale_Roots_1
{
    // This class is a simple data container that holds an entire narrative sequence.
    // It acts as the "folder" that groups all the individual cinematic shots together.
    public class Cutscene
    {
        // The ordered list of images, text, and timings that make up this specific cinematic.
        // We make the 'set' private so outside classes can't accidentally overwrite the entire sequence.
        public List<CutsceneSlide> Slides { get; private set; } = new List<CutsceneSlide>();

        // A simple helper method used heavily by the CutsceneLibrary to build out the scene one shot at a time.
        public void AddSlide(CutsceneSlide slide)
        {
            Slides.Add(slide);
        }
    }
}