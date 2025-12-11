using BepInEx;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Linq;

namespace LizardEggs
{
    [BepInPlugin(ID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ID = "falesk.lizardeggs";
        public const string Name = "Lizard Eggs";
        public const string Version = "1.3.0.3";
        public bool eggInShelter;
        private static bool loaded = false;
        public static ManualLogSource logger;

        public void Awake()
        {
            try
            {
                if (!loaded)
                {
                    // Mod Init / Deinit
                    On.RainWorld.OnModsInit += RainWorld_OnModsInit;
                    On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;
                    On.RainWorld.LoadModResources += delegate (On.RainWorld.orig_LoadModResources orig, RainWorld self)
                    {
                        orig(self);
                        Futile.atlasManager.LoadAtlas("assets/lizeggs_sprites");
                    };
                    On.RainWorld.UnloadResources += delegate (On.RainWorld.orig_UnloadResources orig, RainWorld self)
                    {
                        orig(self);
                        Futile.atlasManager.UnloadAtlas("assets/lizeggs_sprites");
                    };
                    // Other
                    IL.WinState.CycleCompleted += WinState_CycleCompleted;
                    On.World.ctor += (orig, self, game, region, name, singleRoomWorld) =>
                    {
                        orig(self, game, region, name, singleRoomWorld);
                        FDataManager.InitDens();
                    };
                    On.Player.SleepUpdate += (orig, self) =>
                    {
                        orig(self);
                        if (self.sleepCounter < 0)
                            eggInShelter = self.room.abstractRoom.shelter && self.room.physicalObjects[1].Exists(obj => obj is LizardEgg);
                    };
                    On.MultiplayerUnlocks.SandboxItemUnlocked += (orig, self, unlockID) =>
                        unlockID == Register.LizardEggUnlock || orig(self, unlockID);
                    On.MultiplayerUnlocks.SandboxUnlockForSymbolData += (orig, data) =>
                        (data.itemType == Register.LizardEgg) ? Register.LizardEggUnlock : orig(data);
                    On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += (orig, unlockID) =>
                    (unlockID == Register.LizardEggUnlock) ? new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, 0) : orig(unlockID);
                    loaded = true;
                }
            }
            catch (Exception e) { Logger.LogError(e); }
        }

        //Mother Passage
        private void WinState_CycleCompleted(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdsfld(typeof(MoreSlugcats.MoreSlugcatsEnums.EndgameID).GetField(nameof(MoreSlugcats.MoreSlugcatsEnums.EndgameID.Mother))),
                    x => x.MatchLdloc(0),
                    x => x.MatchLdcI4(0),
                    x => x.MatchCgt(),
                    x => x.MatchCallOrCallvirt(typeof(WinState).GetMethod(nameof(WinState.GetTracker))),
                    x => x.MatchIsinst(typeof(WinState.FloatTracker)),
                    x => x.MatchStloc(25),
                    x => x.MatchLdloc(25),
                    x => x.MatchBrfalse(out _)
                    );
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate<Func<int, int>>(num => (num == 0 && eggInShelter) ? 1 : num);
                c.Emit(OpCodes.Stloc_0);
            }
            catch (Exception e) { Logger.LogError(e); }
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            logger = Logger;
            Register.UnregisterValues();
            Register.RegisterValues();
            MachineConnector.SetRegisteredOI(ID, new Options());
            MultiplayerUnlocks.ItemUnlockList.Add(Register.LizardEggUnlock);
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
    }
}