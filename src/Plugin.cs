using BepInEx;
using MoreSlugcats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LizardEggs
{
    [BepInPlugin(ID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ID = "falesk.lizardeggs";
        public const string Name = "Lizard Eggs";
        public const string Version = "1.2";
        public static string FilePath { get; private set; }
        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> EggsInDen { get; private set; }
        public static List<SavedLizard> lizards;
        public bool eggInShelter;
        public float eggMotherProgress = 0f;

        public void Awake()
        {
            // Mod Init / Deinit
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;
            // Other
            On.WinState.CycleCompleted += WinState_CycleCompleted;
            On.SaveState.SessionEnded += SaveState_SessionEnded;
            On.World.ctor += (On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld) =>
            {
                orig(self, game, region, name, singleRoomWorld);
                EggsInDen = new Dictionary<WorldCoordinate, (AbstractCreature, int)>();
            };
            On.Player.SleepUpdate += (On.Player.orig_SleepUpdate orig, Player self) =>
            {
                orig(self);
                if (self.sleepCounter < 0)
                    eggInShelter = self.room.abstractRoom.shelter && self.room.physicalObjects[1].Exists(obj => obj is LizardEgg);
            };
            On.MultiplayerUnlocks.SandboxItemUnlocked += (On.MultiplayerUnlocks.orig_SandboxItemUnlocked orig, MultiplayerUnlocks self, MultiplayerUnlocks.SandboxUnlockID unlockID) =>
                unlockID == Register.LizardEggUnlock || orig(self, unlockID);
            On.MultiplayerUnlocks.SandboxUnlockForSymbolData += (On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data) =>
                (data.itemType == Register.LizardEgg) ? Register.LizardEggUnlock : orig(data);
            On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += (On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID) =>
            (unlockID == Register.LizardEggUnlock) ? new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, 0) : orig(unlockID);
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            Register.UnregisterValues();
            Register.RegisterValues();
            MachineConnector.SetRegisteredOI(ID, new Options());
            MultiplayerUnlocks.ItemUnlockList.Add(Register.LizardEggUnlock);
            InitLizards();
            HooksGeneral.Init();
            HooksObject.Init();
            HooksLizard.Init();
            HooksBL.Init();
        }

        private void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
        {
            orig(self, newlyDisabledMods);
            if (newlyDisabledMods.Any(mod => mod.id == ID))
                Register.UnregisterValues();
        }

        private void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
        {
            if (!ModManager.MSC)
            {
                orig(self, game);
                return;
            }
            if (eggInShelter && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0 && self.GetTracker(MoreSlugcatsEnums.EndgameID.Mother, eggInShelter) is WinState.FloatTracker tracker)
            {
                eggMotherProgress = tracker.progress + 1f / 6f;
                tracker.SetProgress(eggMotherProgress);
            }
            else if (game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0)
                eggMotherProgress = 0f;
            orig(self, game);
            if (eggInShelter && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0 && self.GetTracker(MoreSlugcatsEnums.EndgameID.Mother, eggInShelter) is WinState.FloatTracker tracker1)
                tracker1.SetProgress(eggMotherProgress);
        }

        private void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
        {
            orig(self, game, survived, newMalnourished);
            File.WriteAllText(FilePath, string.Empty);
            for (int i = 0; i < lizards.Count; i++)
            {
                if (lizards[i].slatedForDeletion || lizards[i].stage - Options.lizGrowthTime.Value > 0)
                {
                    if (survived)
                    {
                        lizards.RemoveAt(i--);
                        continue;
                    }
                    lizards[i].slatedForDeletion = false;
                }
                else lizards[i].stage += survived ? 1 : 0;
                File.AppendAllText(FilePath, $"{lizards[i]}\n");
            }
        }

        private static void InitLizards()
        {
            lizards = new List<SavedLizard>();
            if (ModManager.ActiveMods.Find(x => x.id == ID) is ModManager.Mod mod)
                FilePath = AssetManager.ResolveFilePath("plugins/babyLizards.txt", true);
            if (File.Exists(FilePath))
                foreach (string line in File.ReadLines(FilePath))
                    lizards.Add(SavedLizard.FromString(line));
            else File.Create(FilePath);
        }

        public class SavedLizard
        {
            public SavedLizard(EntityID ID, CreatureTemplate.Type parent, int stage)
            {
                this.ID = ID;
                this.parent = parent;
                this.stage = stage;
                slatedForDeletion = false;
            }

            public override string ToString() => $"{ID}~{parent}~{stage}";

            public static SavedLizard FromString(string s)
            {
                string[] array = s.Split('~');
                EntityID ID = EntityID.FromString(array[0]);
                CreatureTemplate.Type parent = new CreatureTemplate.Type(array[1]);
                int stage = int.Parse(array[2]);
                return new SavedLizard(ID, parent, stage);
            }

            public EntityID ID;
            public CreatureTemplate.Type parent;
            public int stage;
            public bool slatedForDeletion;
        }
    }
}