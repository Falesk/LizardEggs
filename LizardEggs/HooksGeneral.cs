using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using UnityEngine;

namespace LizardEggs
{
    public static class HooksGeneral
    {
        public static void Init()
        {
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.Player.DirectIntoHoles += Player_DirectIntoHoles;
            IL.WorldLoader.GeneratePopulation += WorldLoader_GeneratePopulation;
        }

        private static void WorldLoader_GeneratePopulation(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                c.GotoNext(
                    MoveType.Before,
                    x => x.MatchLdfld(typeof(World.CreatureSpawner).GetField(nameof(World.CreatureSpawner.nightCreature))),
                    x => x.MatchStfld(typeof(AbstractCreature).GetField(nameof(AbstractCreature.nightCreature))),
                    x => x.MatchLdloc(11),
                    x => x.MatchCallOrCallvirt(typeof(AbstractCreature).GetMethod(nameof(AbstractCreature.setCustomFlags))),
                    x => x.MatchLdloc(9),
                    x => x.MatchLdloc(11),
                    x => x.MatchCallOrCallvirt(typeof(AbstractRoom).GetMethod(nameof(AbstractRoom.MoveEntityToDen)))
                    );
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 11);
                c.EmitDelegate<Action<WorldLoader, AbstractCreature>>((self, abstr) =>
                {
                    if (ModManager.MSC && self.game.rainWorld.safariMode)
                        return;
                    if (abstr.creatureTemplate.IsLizard && !abstr.preCycle)
                        FDataManager.AddToDens(abstr, self.playerCharacter);
                });
            }
            catch (Exception e) { Plugin.logger.LogError(e); }
        }

        private static void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            orig(self, newRoom, cameraPosition);
            foreach (var den in FDataManager.Dens)
                if (newRoom.abstractRoom.index == den.Key.room && den.Value.Item2 > 0)
                    newRoom.AddObject(new Indicator(den.Key, self.room));
        }

        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            try
            {
                string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);
                if (array[1] == Register.LizardEgg.value)
                {
                    int birthday = int.Parse(array[6]);
                    int bites = int.Parse(array[8]);
                    int openTime = int.Parse(array[9]);
                    if (world.regionState.saveState.cycleNumber - birthday == Options.eggGrowthTime.Value && openTime == -1)
                        return SpawnLizard(world, array);
                    return new AbstractLizardEgg(world, WorldCoordinate.FromString(array[2]), EntityID.FromString(array[0]), EntityID.FromString(array[5]), float.Parse(array[4]), FCustom.HEX2ARGB(uint.Parse(array[3])), array[7], birthday, bites, openTime)
                    { unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 10) };
                }
            }
            catch { Plugin.logger.LogWarning($"Exception in SaveState.AbstractPhysicalObjectFromString"); }
            return orig(world, objString);
        }

        private static void Player_DirectIntoHoles(On.Player.orig_DirectIntoHoles orig, Player self)
        {
            orig(self);
            IntVector2 tile = self.room.GetTilePosition(self.mainBodyChunk.pos + new Vector2(40f * self.input[0].x, 40f * self.input[0].y));
            if (self.room.shortcutData(tile).shortCutType != ShortcutData.Type.CreatureHole)
                return;
            WorldCoordinate den = self.room.GetWorldCoordinate(tile);
            den.abstractNode = FCustom.GetAbstractNode(den, self.room);
            den.Tile = new IntVector2(-1, -1);
            if (FDataManager.Dens.TryGetValue(den, out var val) && val.Item2 > 0 && self.FreeHand() != -1 && ((self.input[0].pckp && Options.takeButton.Value == KeyCode.None) || Input.GetKey(Options.takeButton.Value)))
            {
                Lizard liz = (val.Item1?.realizedCreature as Lizard) ?? new Lizard(val.Item1, self.room.world);
                float size = Mathf.Lerp(1, 10, Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value) * Mathf.InverseLerp(0, 7, liz.lizardParams.bodyMass));
                Color color = liz.effectColor + FCustom.RandomGray(0.15f);
                color.a = 1f;
                AbstractLizardEgg abstractEgg = new AbstractLizardEgg(
                    self.room.world,
                    self.room.GetWorldCoordinate(self.firstChunk.pos),
                    self.room.game.GetNewID(), liz.abstractCreature.ID,
                    size,
                    FCustom.Clamp01Color(color),
                    liz.Template.name,
                    self.room.world.regionState.saveState.cycleNumber);
                self.abstractCreature.Room.AddEntity(abstractEgg);
                abstractEgg.RealizeInRoom();
                self.SlugcatGrab(abstractEgg.realizedObject, self.FreeHand());
                FDataManager.ChangeDensValue(den, -1);
                if (--val.Item2 == 0 && self.room.drawableObjects.Find(x => x is Indicator i && i.den == den) is Indicator ind)
                    ind.Destroy();
            }
        }

        private static AbstractCreature SpawnLizard(World world, string[] array)
        {
            string parentType = array[7];
            WorldCoordinate pos = WorldCoordinate.FromString(array[2]);

            UnityEngine.Random.State rstate = UnityEngine.Random.state;
            UnityEngine.Random.InitState(EntityID.FromString(array[0]).RandomSeed);
            EntityID lizID = world.game.GetNewID(EntityID.FromString(array[5]).spawner);

            if (ModManager.MSC && Options.trLizOpport.Value && parentType == StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.RedLizard).name && UnityEngine.Random.value < 0.1f)
                parentType = StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard).name;
            parentType = string.IsNullOrEmpty(parentType) ? FDataManager.RandomLizard() : parentType;
            UnityEngine.Random.state = rstate;

            AbstractCreature abstr = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(Register.BabyLizard), null, pos, lizID);
            (abstr.state as BabyLizardState).parent = StaticWorld.GetCreatureTemplate(parentType).type;
            (abstr.state as BabyLizardState).hexColor = uint.Parse(array[3]);
            (abstr.state as BabyLizardState).LimbFix();
            return abstr;
        }
    }
}