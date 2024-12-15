using UnityEngine;
using RWCustom;
using MoreSlugcats;
using static LizardEggs.Plugin;

namespace LizardEggs
{
    public static class HooksGeneral
    {
        public static void Init()
        {
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.Player.DirectIntoHoles += Player_DirectIntoHoles;
        }

        private static void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            orig(self, newRoom, cameraPosition);
            if (ModManager.MSC && newRoom.game.rainWorld.safariMode)
                return;
            foreach (AbstractRoom room in newRoom.world.abstractRooms)
                foreach (AbstractCreature abstr in room.creatures)
                    if (abstr.creatureTemplate.IsLizard)
                        AddToEggsDict(newRoom.world, abstr);
            foreach (var den in EggsInDen)
                if (newRoom.abstractRoom.index == den.Key.room && den.Value.Item2 > 0)
                    newRoom.AddObject(new Indicator(den.Key, self.room));
        }

        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            try
            {
                string[] array = objString.Split(new[] { "<oA>" }, System.StringSplitOptions.None);
                if (array[1] == Register.LizardEgg.value)
                {
                    int stage = ((world.rainCycle?.timer == null || world.rainCycle.timer < 40) && world.GetAbstractRoom(WorldCoordinate.FromString(array[2])).shelter) ? int.Parse(array[6]) + 1 : int.Parse(array[6]);
                    if (stage >= Options.eggGrowthTime.Value)
                        return SpawnLizard(world, array);
                    return new AbstractLizardEgg(world, WorldCoordinate.FromString(array[2]), EntityID.FromString(array[0]), EntityID.FromString(array[5]), float.Parse(array[4]), FCustom.IntToColor(int.Parse(array[3])), array[7], stage, true)
                    { unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8) };
                }
            }
            catch { }
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
            if (EggsInDen.TryGetValue(den, out var val) && val.Item2 > 0 && self.FreeHand() != -1 && ((self.input[0].pckp && Options.takeButton.Value == KeyCode.None) || Input.GetKey(Options.takeButton.Value)))
            {
                Lizard liz = (val.Item1?.realizedCreature as Lizard) ?? new Lizard(val.Item1, self.room.world);
                float size = (liz.lizardParams.bodyMass > 5f) ? 0.5f * liz.lizardParams.bodyMass : liz.lizardParams.bodyMass;
                AbstractLizardEgg abstractEgg = new AbstractLizardEgg(self.room.world, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), liz.abstractCreature.ID, size, liz.effectColor, liz.Template.type.value);
                self.abstractCreature.Room.AddEntity(abstractEgg);
                abstractEgg.RealizeInRoom();
                self.SlugcatGrab(abstractEgg.realizedObject, self.FreeHand());
                FCustom.ChangeDictTuple(EggsInDen, den, -1);
                if (--val.Item2 == 0 && self.room.drawableObjects.Find(x => x is Indicator i && i.den == den) is Indicator ind)
                {
                    self.room.RemoveObject(ind);
                    return;
                }
            }
        }

        private static AbstractCreature SpawnLizard(World world, string[] array)
        {
            FCustom.InitLizTypes();
            string parentType = array[7];
            WorldCoordinate pos = WorldCoordinate.FromString(array[2]);

            Random.State state = Random.state;
            Random.InitState(EntityID.FromString(array[0]).RandomSeed);
            EntityID lizID = world.game.GetNewID(EntityID.FromString(array[5]).spawner);

            AbstractCreature abstrLizard;
            if (ModManager.MSC && Options.trLizOpport.Value && parentType == "RedLizard" && Random.value < 0.1f)
                parentType = MoreSlugcatsEnums.CreatureTemplateType.TrainLizard.value;
            abstrLizard = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(Register.BabyLizard), null, pos, lizID);
            parentType = (parentType == "") ? FCustom.lizTypes[Random.Range(0, FCustom.lizTypes.Count)].type.value : parentType;
            Random.state = state;

            if (Options.youngLiz.Value && lizards.Find(liz => liz.ID == lizID) == null)
                lizards.Add(new SavedLizard(lizID, new CreatureTemplate.Type(parentType), 0));
            return abstrLizard;
        }

        private static bool AddToEggsDict(World world, AbstractCreature abstr)
        {
            if (EggsInDen.ContainsKey(abstr.spawnDen) || !abstr.spawnDen.NodeDefined || abstr.creatureTemplate.name == "YoungLizard" || abstr.creatureTemplate.name == "BabyLizard")
                return false;
            EggsInDen.Add(abstr.spawnDen, (abstr, 0));
            int amount = (world.GetSpawner(abstr.ID) as World.SimpleSpawner)?.amount ?? 1;
            float chance = FCustom.EggSpawnChance((world.game.Players[0].realizedCreature as Player).playerState.slugcatCharacter);
            if (Options.occurrenceFrequency.Value > 1f)
                chance += (1f - chance) * (1f - 1f / Options.occurrenceFrequency.Value);
            else chance *= Options.occurrenceFrequency.Value;
            for (int i = 0; i < amount; i++)
                if (Random.value < chance)
                    FCustom.ChangeDictTuple(EggsInDen, abstr.spawnDen, 1);
            return true;
        }
    }
}
