namespace Pale_Roots_1
{
    // An Enum is a custom "list of options."
    // This GameState acts as the master brain for the engine, 
    // ensuring the game only runs the logic it needs at any given moment.
    public enum GameState
    {
        // The "Splash Screens" or logo sequences when the game first boots up.
        Intro,

        // The Title Screen where the player can select "Start Game" or "Options."
        Menu,

        // The core game loop where the player is actually moving, fighting, and exploring.
        Gameplay,

        // A pause state where the player selects new perks or upgrades.
        LevelUp,

        // The screen shown after successfully beating a boss or finishing a run.
        Victory,

        // A narrative cinematic or story sequence shown after winning.
        Outro,

        // The scrolling list of the talented people who made the game!
        Credits,

        // The failure state shown when the player's health hits zero.
        GameOver
    }
}