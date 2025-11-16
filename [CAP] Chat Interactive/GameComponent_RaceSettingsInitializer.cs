using _CAP__Chat_Interactive.Utilities;
using CAP_ChatInteractive;
using Verse;

public class GameComponent_RaceSettingsInitializer : GameComponent
{
    private bool raceSettingsInitialized = false;
    private int initializationDelay = 0;

    public GameComponent_RaceSettingsInitializer(Game game) { }

    public override void LoadedGame()
    {
        Logger.Debug("RaceSettings Init Game Loaded");
        InitializeRaceSettingsWithDelay();
    }

    public override void StartedNewGame()
    {
        Logger.Debug("RaceSettings Init Started New Game");
        InitializeRaceSettingsWithDelay();
    }

    public override void GameComponentTick()
    {
        
        // Initialize after a small delay to ensure all mods are loaded
        if (!raceSettingsInitialized && initializationDelay < 60) // ~1 second delay
        {
            Logger.Debug("RaceSettings Init Game Tick no settings so init");
            initializationDelay++;
            if (initializationDelay >= 60)
            {
                InitializeRaceSettingsWithDelay();
            }
        }
    }

    private void InitializeRaceSettingsWithDelay()
    {
        if (!raceSettingsInitialized)
        {
            Logger.Debug("=== INITIALIZING RACE SETTINGS WITH DELAY ===");

            // Ensure AlienProvider is initialized FIRST
            //if (CAPChatInteractiveMod.Instance != null)
            //{
            //    Logger.Debug("Initializing AlienProvider...");
            //    CAPChatInteractiveMod.Instance.InitializeAlienCompatibilityProvider();
            //}

            // NOW initialize race settings (provider should be available)
            Logger.Debug("Initializing race settings...");
            InitializeRaceSettings();
            raceSettingsInitialized = true;

            Logger.Debug($"Race settings initialized with {RaceSettingsManager.RaceSettings.Count} races");
            Logger.Debug("=== FINISHED INITIALIZING RACE SETTINGS ===");
        }
    }

    private void InitializeRaceSettings()
    {
        // This will trigger the RaceSettingsManager to load and initialize settings
        var settings = RaceSettingsManager.RaceSettings;
        Logger.Debug($"Race settings loaded: {settings.Count} races");
    }
}