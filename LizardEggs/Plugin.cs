using BepInEx;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace LizardEggs
{
    [BepInPlugin(GUID, Name, Version)]
    class Plugin : BaseUnityPlugin
    {
        public const string GUID = "falesk.lizardeggs";
        public const string Name = "Lizard Eggs";
        public const string Version = "1.1.1";
        public void Awake()
        {
            // Mod Init / Deinit
            On.RainWorld.OnModsInit += delegate (On.RainWorld.orig_OnModsInit orig, RainWorld self)
            {
                orig(self);
                Register.RegisterValues();
                Register.InitConfig();
            };
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

            // Slugpups stuff
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
                    if (math.abs(num) > 0.4f)
                        self.foodReaction += (int)(num * 120f);
                    if (math.abs(num) > 0.85f && self.FunStuff)
                        self.cat.Stun((int)math.lerp(10f, 25f, Mathf.InverseLerp(0.85f, 1f, math.abs(num))));
                    return;
                }
                orig(self, food);
            };

            // Other
            On.World.ctor += delegate (On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld)
            {
                orig(self, game, region, name, singleRoomWorld);
                EggsInDen = new Dictionary<WorldCoordinate, (AbstractCreature, int)>();
                FCustom.InitLizTypes();
            };
            On.WinState.CycleCompleted += delegate (On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
            {
                if (ModManager.MSC && eggInShelter && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0 && self.GetTracker(MoreSlugcatsEnums.EndgameID.Mother, eggInShelter) is WinState.FloatTracker tracker && tracker != null)
                {
                    eggMotherProgress = tracker.progress;
                    tracker.SetProgress(eggMotherProgress + 0.167f);
                    eggMotherProgress += 0.167f;
                }
                else if (ModManager.MSC)
                    eggMotherProgress = 0f;
                orig(self, game);
                if (ModManager.MSC && eggInShelter && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0 && self.GetTracker(MoreSlugcatsEnums.EndgameID.Mother, eggInShelter) is WinState.FloatTracker tracker1 && tracker1 != null)
                    tracker1.SetProgress(eggMotherProgress);
            };
            On.PlayerSessionRecord.AddEat += delegate (On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenObject)
            {
                orig(self, eatenObject);
                if (eatenObject is LizardEgg)
                {
                    self.vegetarian = false;
                    self.carnivorous = true;
                }
            };
            On.AbstractPhysicalObject.Realize += delegate (On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
            {
                orig(self);
                if (self.type == Register.LizardEgg)
                {
                    if (self is AbstractLizardEgg)
                        self.realizedObject = new LizardEgg(self);
                    else self.realizedObject = new LizardEgg(new AbstractLizardEgg(self));
                }
            };
            On.ScavengerAI.CollectScore_PhysicalObject_bool += delegate (On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
            {
                if (self.scavenger.room != null)
                {
                    SocialEventRecognizer.OwnedItemOnGround ownedItemOnGround = self.scavenger.room.socialEventRecognizer.ItemOwnership(obj);
                    if (ownedItemOnGround != null && ownedItemOnGround.offeredTo != null && ownedItemOnGround.offeredTo != self.scavenger)
                        return 0;
                }
                if (obj is LizardEgg)
                    return 2;
                return orig(self, obj, weaponFiltered);
            };

            // Player stuff
            On.Player.Grabability += delegate (On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
            {
                if (obj is LizardEgg egg)
                {
                    if (egg.firstChunk.mass < 0.25f)
                        return Player.ObjectGrabability.BigOneHand;
                    return Player.ObjectGrabability.TwoHands;
                }
                return orig(self, obj);
            };
            On.SlugcatStats.NourishmentOfObjectEaten += delegate (On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
            {
                if (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint && eatenobject is LizardEgg)
                    return -1;
                if ((slugcatIndex == SlugcatStats.Name.Red || (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && eatenobject is LizardEgg)
                    return 6 * eatenobject.FoodPoints;
                return orig(slugcatIndex, eatenobject);
            };
            On.Player.Update += delegate (On.Player.orig_Update orig, Player self, bool eu)
            {
                orig(self, eu);
                bool flag = false;
                if (self.room != null && self.room.abstractRoom.shelter)
                    foreach (PhysicalObject obj in self.room.physicalObjects[1])
                        if (obj is LizardEgg)
                        {
                            flag = true;
                            break;
                        }
                eggInShelter = flag;
            };

            // Lizard AI stuff
            On.LizardAI.DoIWantToHoldThisWithMyTongue += (On.LizardAI.orig_DoIWantToHoldThisWithMyTongue orig, LizardAI self, BodyChunk chunk) => orig(self, chunk) || (chunk != null && chunk.owner.room == self.lizard.room && (chunk.owner.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.creature.ID);
            On.LizardAI.DetermineBehavior += delegate (On.LizardAI.orig_DetermineBehavior orig, LizardAI self)
            {
                if (self.lizard.grasps[0] != null && (self.lizard.grasps[0].grabbed.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.creature.ID)
                {
                    self.currentUtility = 1f;
                    return LizardAI.Behavior.ReturnPrey;
                }
                return orig(self);
            };
            On.LizardAI.Update += delegate (On.LizardAI.orig_Update orig, LizardAI self)
            {
                orig(self);
                if (self.creature.GetData() is FCustom.CritData data && data.egg != null)
                {
                    if (self.pathFinder.CoordinateReachableAndGetbackable(data.egg.pos))
                    {
                        self.creature.abstractAI.SetDestination(data.egg.pos);
                        self.runSpeed = math.lerp(self.runSpeed, 1f, 0.75f);
                    }
                    else self.behavior = LizardAI.Behavior.Idle;
                }
            };
            On.Lizard.CarryObject += delegate (On.Lizard.orig_CarryObject orig, Lizard self, bool eu)
            {
                if (self?.grasps[0]?.grabbed == null)
                    return;
                if ((self.grasps[0].grabbed.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.abstractCreature.ID)
                {
                    self.grasps[0].grabbed.firstChunk.vel = self.mainBodyChunk.vel;
                    self.grasps[0].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * 25f * self.lizardParams.headSize);
                    return;
                }
                orig(self, eu);
            };

            // Lizard Graphics
            On.LizardGraphics.HeadColor += delegate (On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
            {
                try
                {
                    if (self.lizard.abstractCreature.GetData() is FCustom.CritData data && data.egg != null && data.egg.Room == self.lizard.room.abstractRoom && self.lizard.Consious)
                        return Color.Lerp(self.HeadColor1, self.HeadColor2, (data.egg.realizedObject as LizardEgg).Luminance);
                }
                catch { }
                return orig(self, timeStacker);
            };

            // Icon
            On.ItemSymbol.ColorForItem += delegate (On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
            {
                if (itemType == Register.LizardEgg)
                    return Color.Lerp((intData == 0) ? Color.green : FCustom.IntToColor(intData), Color.black, 0.4f);
                return orig(itemType, intData);
            };
            On.ItemSymbol.SymbolDataFromItem += delegate (On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
            {
                if (item.type == Register.LizardEgg)
                    return new IconSymbol.IconSymbolData?(new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, item.type, FCustom.ColorToInt((item as AbstractLizardEgg).color)));
                return orig(item);
            };
            On.ItemSymbol.SpriteNameForItem += delegate (On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
            {
                if (itemType == Register.LizardEgg)
                    return "HipsA";
                return orig(itemType, intData);
            };

            // Major delegates
            On.LizardGraphics.ctor += LizardGraphics_ctor;
            On.Lizard.Update += Lizard_Update;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.Player.DirectIntoHoles += Player_DirectIntoHoles;
        }

        private void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (smlLizards.TryGetValue(ow.abstractPhysicalObject.ID, out int stage) && stage < Register.lizGrowthTime.Value && (ow as Lizard).abstractCreature.GetData() is FCustom.CritData data && Register.youngLiz.Value)
            {
                if (data.isChild)
                {
                    self.iVars.fatness *= 0.5f;
                    self.iVars.headSize *= 0.7f;
                    self.iVars.tailLength *= 0.6f;
                    return;
                }
                stage++;
                data.isChild = stage < Register.lizGrowthTime.Value;
                if (data.isChild)
                {
                    self.iVars.fatness *= 0.5f;
                    self.iVars.headSize *= 0.7f;
                    self.iVars.tailLength *= 0.6f;
                }
                else
                {
                    smlLizards.Remove(ow.abstractPhysicalObject.ID);
                    return;
                }
                smlLizards[ow.abstractPhysicalObject.ID] = stage;
            }
        }

        private void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            if (self.abstractCreature.InDen)
                return;
            foreach (PhysicalObject obj in self.room.physicalObjects[1])
            {
                if (self.abstractCreature.GetData() is FCustom.CritData data && !Register.tamedAggressiveness.Value)
                {
                    if (!data.sawPlayerWithEgg && obj is Player player && self.AI.VisualContact(player.mainBodyChunk) && player.grasps[0] != null && player.grasps[0].grabbed is LizardEgg && (player.grasps[0].grabbed as LizardEgg).AbstractLizardEgg.parentID == self.abstractCreature.ID)
                    {
                        data.sawPlayerWithEgg = true;
                        self.AI.LizardPlayerRelationChange(-Register.lizAggressiveness.Value, player.abstractCreature);
                        var personality = self.abstractCreature.personality;
                        if (math.lerp(personality.sympathy, personality.aggression, personality.nervous) > 2f * personality.sympathy * personality.bravery)
                        {
                            self.AI.behavior = LizardAI.Behavior.Hunt;
                            self.AI.preyTracker.currentPrey = new PreyTracker.TrackedPrey(self.AI.preyTracker, self.AI.CreateTrackerRepresentationForCreature(player.abstractCreature));
                            self.animation = Lizard.Animation.HearSound;
                            self.timeToRemainInAnimation = 40;
                            self.bubbleIntensity = 0.3f;
                        }
                    }
                    if (obj is LizardEgg egg && egg.AbstractLizardEgg.parentID == self.abstractCreature.ID)
                    {
                        if (data.egg == null)
                            data.egg = egg.abstractPhysicalObject as AbstractLizardEgg;
                        if ((self.graphicsModule as LizardGraphics)?.lightSource != null)
                            (self.graphicsModule as LizardGraphics).lightSource.alpha = (data.egg.realizedObject as LizardEgg).Luminance;
                        if (self.grasps[0]?.grabbed == null && Vector2.Distance(self.firstChunk.pos, data.egg.realizedObject.firstChunk.pos) < 20f && self.Consious)
                        {
                            self.Bite(data.egg.realizedObject.firstChunk);
                            self.CarryObject(eu);
                        }
                        break;
                    }
                }
            }
            if (self.grasps[0]?.grabbed != null && self.grasps[0].grabbed is LizardEgg && self.enteringShortCut != null && self.room.shortcutData((IntVector2)self.enteringShortCut).shortCutType == ShortcutData.Type.CreatureHole && self.abstractCreature.GetData() is FCustom.CritData data1)
            {
                PhysicalObject egg = self.grasps[0].grabbed;
                self.LoseAllGrasps();
                egg.Destroy();
                self.AI.behavior = LizardAI.Behavior.Idle;
                data1.egg = null;
                FCustom.ChangeDictTuple(EggsInDen, self.abstractCreature.spawnDen, 1);
                Indicator indicator = new Indicator(self.abstractCreature.spawnDen, self.room);
                self.room.AddObject(indicator);
                indicators.Add(indicator);
            }
            orig(self, eu);
        }

        private void RoomCamera_ChangeRoom(On.RoomCamera.orig_ChangeRoom orig, RoomCamera self, Room newRoom, int cameraPosition)
        {
            orig(self, newRoom, cameraPosition);
            if (ModManager.MSC && newRoom.game.rainWorld.safariMode)
                return;
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
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = new AbstractPhysicalObject.AbstractObjectType(array[1]);
                if (abstractObjectType == Register.LizardEgg)
                {
                    if (int.Parse(array[6]) > Register.eggGrowthTime.Value)
                        return null;
                    int stage = (world.rainCycle.timer < 40 && world.GetAbstractRoom(WorldCoordinate.FromString(array[2])).shelter) ? int.Parse(array[6]) + 1 : int.Parse(array[6]);
                    AbstractPhysicalObject abstrEgg = new AbstractLizardEgg(world, WorldCoordinate.FromString(array[2]), EntityID.FromString(array[0]), EntityID.FromString(array[5]), float.Parse(array[4]), FCustom.IntToColor(int.Parse(array[3])), array[7], stage, true)
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
                WorldCoordinate den = new WorldCoordinate(shortCutData.startCoord.room, -1, -1, i);
                if (shortCutData.startCoord.CompareDisregardingNode(self.room.LocalCoordinateOfNode(i)) && EggsInDen.ContainsKey(den) && EggsInDen[den].Item2 > 0 && self.FreeHand() != -1 && self.input[0].pckp)
                {
                    Lizard liz = (EggsInDen[den].Item1?.realizedCreature as Lizard) ?? new Lizard(EggsInDen[den].Item1, self.room.world);
                    float size = (liz.lizardParams.bodyMass > 5f) ? 0.5f * liz.lizardParams.bodyMass : liz.lizardParams.bodyMass;
                    AbstractLizardEgg abstractEgg = new AbstractLizardEgg(self.room.world, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), liz.abstractCreature.ID, size, liz.effectColor, liz.Template.name);
                    self.abstractCreature.Room.AddEntity(abstractEgg);
                    abstractEgg.RealizeInRoom();
                    self.SlugcatGrab(abstractEgg.realizedObject, self.FreeHand());
                    FCustom.ChangeDictTuple(EggsInDen, den, -1);
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

        public bool AddToEggsDict(World world, AbstractCreature abstr)
        {
            if (EggsInDen.ContainsKey(abstr.spawnDen) || !abstr.spawnDen.NodeDefined || abstr.creatureTemplate.name == "YoungLizard")
                return false;
            EggsInDen.Add(abstr.spawnDen, (abstr, 0));
            int amount = (world.GetSpawner(abstr.ID) as World.SimpleSpawner)?.amount ?? 1;
            float chance = FCustom.EggSpawnChance((world.game.Players[0].realizedCreature as Player).playerState.slugcatCharacter);
            if (Register.occurrenceFrequency.Value > 1f)
                chance += (1 - chance) * (1 - 1 / Register.occurrenceFrequency.Value);
            else chance *= Register.occurrenceFrequency.Value;
            for (int i = 0; i < amount; i++)
                if (UnityEngine.Random.value < chance)
                    FCustom.ChangeDictTuple(EggsInDen, abstr.spawnDen, 1);
            return true;
        }

        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> EggsInDen { get; private set; }
        public static Dictionary<EntityID, int> smlLizards = new Dictionary<EntityID, int>();
        public List<Indicator> indicators;
        public Room lastRoom;
        public bool eggInShelter;
        public float eggMotherProgress = 0f;
    }
}
