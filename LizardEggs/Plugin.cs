using BepInEx;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace LizardEggs
{
    [BepInPlugin(GUID, Name, Version)]
    class Plugin : BaseUnityPlugin
    {
        public const string GUID = "falesk.lizardeggs";
        public const string Name = "Lizard Eggs";
        public const string Version = "1.0.0";
        public void Awake()
        {
            Register.RegisterValues();
            On.RainWorld.OnModsDisabled += delegate (On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
            {
                orig(self, newlyDisabledMods);
                foreach (ModManager.Mod mod in newlyDisabledMods)
                {
                    if (mod.id == GUID)
                    {
                        Register.UnregisterValues();
                        return;
                    }
                }
            };
            On.MoreSlugcats.SlugNPCAI.GetFoodType += delegate (On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
            {
                if (food is LizardEgg)
                    return Register.LizardEggNPCFood;
                return orig(self, food);
            };
            On.MoreSlugcats.SlugNPCAI.AteFood += delegate (On.MoreSlugcats.SlugNPCAI.orig_AteFood orig, SlugNPCAI self, PhysicalObject food)
            {
                if (food is LizardEgg)
                {
                    float num = self.foodPreference[7];
                    if (Mathf.Abs(num) > 0.4f)
                        self.foodReaction += (int)(num * 120f);
                    if (Mathf.Abs(num) > 0.85f && self.FunStuff)
                        self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(0.85f, 1f, Mathf.Abs(num))));
                    return;
                }
                orig(self, food);
            };
            On.World.ctor += delegate (On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld)
            {
                orig(self, game, region, name, singleRoomWorld);
                EggsInDen = new Dictionary<WorldCoordinate, (AbstractCreature, int)>();
                removedEggs = new List<AbstractLizardEgg>();
            };
            On.Player.Grabability += delegate (On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
            {
                if (obj is LizardEgg) return Player.ObjectGrabability.TwoHands;
                return orig(self, obj);
            };
            On.SlugcatStats.NourishmentOfObjectEaten += delegate (On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
            {
                if (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint && eatenobject is LizardEgg)
                    return -1;
                if ((slugcatIndex == SlugcatStats.Name.Red || (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && eatenobject is LizardEgg)
                    return 8 * eatenobject.FoodPoints;
                return orig(slugcatIndex, eatenobject);
            };

            On.LizardAI.DoIWantToHoldThisWithMyTongue += (On.LizardAI.orig_DoIWantToHoldThisWithMyTongue orig, LizardAI self, BodyChunk chunk) => orig(self, chunk) || (chunk != null && chunk.owner.room == self.lizard.room && (chunk.owner.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.creature.ID);
            On.Lizard.CarryObject += delegate (On.Lizard.orig_CarryObject orig, Lizard self, bool eu)
            {
                if ((self.grasps[0].grabbed.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.abstractCreature.ID)
                {
                    self.grasps[0].grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel = self.mainBodyChunk.vel;
                    self.grasps[0].grabbed.bodyChunks[self.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(eu, self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * 25f * self.lizardParams.headSize);
                    return;
                }
                orig(self, eu);
            };
            On.LizardAI.DetermineBehavior += delegate (On.LizardAI.orig_DetermineBehavior orig, LizardAI self)
            {
                if (self.lizard.grasps[0] != null && (self.lizard.grasps[0].grabbed.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.creature.ID)
                {
                    self.currentUtility = 1f;
                    return LizardAI.Behavior.ReturnPrey;
                }
                return orig(self);
            };
            On.LizardGraphics.ctor += delegate (On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
            {
                orig(self, ow);
                if ((ow as Lizard).abstractCreature.GetData() is FCustom.Data data && data.isChild) // Тут надо умножать!!
                {
                    self.iVars.fatness = 0.6f;
                    self.iVars.headSize = 0.7f;
                    self.iVars.tailFatness = 0.8f;
                    self.iVars.tailLength = 0.6f;
                }
            };

            On.ItemSymbol.ColorForItem += delegate (On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
            {
                if (itemType == Register.LizardEgg)
                    return Color.Lerp(FCustom.IntToColor(intData), Color.black, 0.4f);
                return orig(itemType, intData);
            };
            On.ItemSymbol.SymbolDataFromItem += delegate (On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
            {
                if (item.type == Register.LizardEgg)
                {
                    int data = FCustom.ColorToInt((item as AbstractLizardEgg).color);
                    return new IconSymbol.IconSymbolData?(new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, data));
                }
                return orig(item);
            };
            On.ItemSymbol.SpriteNameForItem += delegate (On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
            {
                if (itemType == Register.LizardEgg) return "HipsA";
                return orig(itemType, intData);
            };

            //On.Lizard.Update += Lizard_Update;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.Player.DirectIntoHoles += Player_DirectIntoHoles;
        }

        //private void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        //{
        //    if (!self.abstractCreature.InDen && self.grasps[0] != null && self.grasps[0].grabbed.abstractPhysicalObject is AbstractLizardEgg egg && Custom.Dist(self.room.MiddleOfTile(self.abstractCreature.pos.Tile), self.room.MiddleOfTile(self.abstractCreature.spawnDen.Tile)) < 40f)
        //    {
        //        var a = EggsInDen[self.abstractCreature.spawnDen];
        //        a.Item2++;
        //        EggsInDen[self.abstractCreature.spawnDen] = a;
        //        Indicator indicator = new Indicator(self.abstractCreature.spawnDen, self.room);
        //        self.LoseAllGrasps();
        //        self.room.AddObject(indicator);
        //        indicators.Add(indicator);
        //        egg.Destroy();
        //    }
        //    orig(self, eu);
        //}

        private void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            orig(self, newRoom, cameraPosition);
            foreach (AbstractRoom room in newRoom.world.abstractRooms)
                foreach (AbstractCreature abstr in room.creatures)
                {
                    if (abstr.creatureTemplate.IsLizard)
                        AddToEggsDict(newRoom.world, abstr);
                }
            if (indicators != null)
            {
                while (indicators.Count > 0)
                {
                    lastRoom.RemoveObject(indicators[0]);
                    indicators.Remove(indicators[0]);
                }
            }
            indicators = new List<Indicator>();
            lastRoom = newRoom;
            foreach (var den in EggsInDen)
            {
                if (newRoom.abstractRoom.index == den.Key.room && den.Value.Item2 > 0)
                {
                    Indicator indicator = new Indicator(den.Key, self.room);
                    newRoom.AddObject(indicator);
                    indicators.Add(indicator);
                }
            }
        }

        private AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            try
            {
                string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = new AbstractPhysicalObject.AbstractObjectType(array[1], false);
                if (abstractObjectType == Register.LizardEgg)
                {
                    AbstractPhysicalObject abstrEgg = new AbstractLizardEgg
                    (
                        world: world,
                        obj: null,
                        pos: WorldCoordinate.FromString(array[2]),
                        ID: EntityID.FromString(array[0]),
                        parentID: EntityID.FromString(array[5]),
                        size: float.Parse(array[4]),
                        color: FCustom.IntToColor(int.Parse(array[3])),
                        stage: int.Parse(array[6]) + 1,
                        parentType: array[7]
                    )
                    { unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8) };
                    return abstrEgg;
                }
            }
            catch { }
            return orig(world, objString);
        }

        private void Player_DirectIntoHoles(On.Player.orig_DirectIntoHoles orig, Player self)
        {
            orig(self);
            ShortcutData shortCutData = self.room.shortcutData(self.room.GetTilePosition(self.mainBodyChunk.pos + new Vector2(40f * self.input[0].x, 40f * self.input[0].y)));
            if (shortCutData.shortCutType != ShortcutData.Type.CreatureHole)
                return;
            for (int i = 0; i < self.room.abstractRoom.nodes.Length; i++)
            {
                if (shortCutData.startCoord.CompareDisregardingNode(self.room.LocalCoordinateOfNode(i)))
                {
                    WorldCoordinate den = new WorldCoordinate(shortCutData.startCoord.room, -1, -1, i);
                    if (EggsInDen.ContainsKey(den) && EggsInDen[den].Item2 > 0 && self.FreeHand() != -1 && self.input[0].pckp && !self.room.abstractRoom.shelter)
                    {
                        Lizard liz = (EggsInDen[den].Item1?.realizedCreature as Lizard) ?? new Lizard(EggsInDen[den].Item1, self.room.world);
                        float size = (EggsInDen[den].Item1.creatureTemplate.type == CreatureTemplate.Type.GreenLizard) ? liz.lizardParams.bodyMass - 3 : liz.lizardParams.bodyMass;
                        AbstractPhysicalObject abstractEgg = new AbstractLizardEgg(self.room.world, null, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), liz.abstractCreature.ID, size, liz.effectColor, liz.Template.type.value);
                        self.abstractCreature.Room.AddEntity(abstractEgg);
                        abstractEgg.RealizeInRoom();
                        self.SlugcatGrab(abstractEgg.realizedObject, self.FreeHand());
                        removedEggs.Add(abstractEgg as AbstractLizardEgg);
                        var a = EggsInDen[den];
                        a.Item2--;
                        EggsInDen[den] = a;
                        if (EggsInDen[den].Item2 == 0)
                        {
                            Indicator ind = null;
                            foreach (Indicator indicator in indicators)
                                if (indicator.den == den)
                                {
                                    ind = indicator;
                                    break;
                                }
                            self.room.RemoveObject(ind);
                            indicators.Remove(ind);
                        }
                        return;
                    }
                }
            }
        }
        public bool AddToEggsDict(World world, AbstractCreature abstr)
        {
            if (EggsInDen.ContainsKey(abstr.spawnDen))
                return false;
            if (!abstr.spawnDen.NodeDefined)
                return false;
            EggsInDen.Add(abstr.spawnDen, (abstr, 0));
            int amount = (world.GetSpawner(abstr.ID) as World.SimpleSpawner)?.amount ?? 1;
            for (int i = 0; i < amount; i++)
                if (UnityEngine.Random.value < 1f) //0.4!!!
                {
                    var a = EggsInDen[abstr.spawnDen];
                    a.Item2++;
                    EggsInDen[abstr.spawnDen] = a;
                }
            return true;
        }
        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> EggsInDen { get; private set; }
        public List<Indicator> indicators;
        public List<AbstractLizardEgg> removedEggs;
        public Room lastRoom;
    }
}
