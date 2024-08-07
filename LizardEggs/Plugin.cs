﻿using BepInEx;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace LizardEggs
{
    [BepInPlugin(GUID, Name, Version)]
    class Plugin : BaseUnityPlugin
    {
        public const string GUID = "falesk.lizardeggs";
        public const string Name = "Lizard Eggs";
        public const string Version = "1.1.6.3";
        public static string FilePath { get; private set; }
        public void Awake()
        {
            // Mod Init / Deinit
            On.RainWorld.OnModsInit += delegate (On.RainWorld.orig_OnModsInit orig, RainWorld self)
            {
                orig(self);
                Register.RegisterValues();
                MachineConnector.SetRegisteredOI(GUID, new Options());
                lizards = new List<SavedLizard>();
                foreach (ModManager.Mod mod in ModManager.ActiveMods)
                {
                    if (mod.id == GUID)
                    {
                        FilePath = $"{mod.path}{Path.DirectorySeparatorChar}plugins{Path.DirectorySeparatorChar}babyLizards.txt";
                        break;
                    }
                }
                if (File.Exists(FilePath))
                    foreach (string line in File.ReadLines(FilePath))
                        lizards.Add(SavedLizard.FromString(line));
                else File.Create(FilePath);
            };
            On.RainWorld.OnModsDisabled += delegate (On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
            {
                orig(self, newlyDisabledMods);
                if (newlyDisabledMods.Any(mod => mod.id == GUID))
                    Register.UnregisterValues();
            };

            // Slugpups stuff
            On.MoreSlugcats.SlugNPCAI.GetFoodType += (On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food) => (food is LizardEgg) ? Register.LizardEggNPCFood : orig(self, food);
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
            };
            On.WinState.CycleCompleted += delegate (On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
            {
                if (ModManager.MSC && eggInShelter && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0 && self.GetTracker(MoreSlugcatsEnums.EndgameID.Mother, eggInShelter) is WinState.FloatTracker tracker)
                {
                    eggMotherProgress = tracker.progress + 0.167f;
                    tracker.SetProgress(eggMotherProgress);
                }
                else if (ModManager.MSC && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0)
                    eggMotherProgress = 0f;
                orig(self, game);
                if (ModManager.MSC && eggInShelter && game.GetStorySession.playerSessionRecords[0].pupCountInDen == 0 && self.GetTracker(MoreSlugcatsEnums.EndgameID.Mother, eggInShelter) is WinState.FloatTracker tracker1)
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
                    else
                    {
                        AbstractLizardEgg egg = new AbstractLizardEgg(self);
                        egg.Room.AddEntity(egg);
                        egg.RealizeInRoom();
                        self.Room.RemoveEntity(self);
                        self.Destroy();
                    }
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
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += delegate (On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
            {
                orig(self);
                if (self.id == Conversation.ID.Moon_Misc_Item && self.describeItem == Register.EggConv)
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It looks like the egg of some kind of creature. How interesting, it has a very hard shell,<LINE>and there are bioluminescent inclusions all over its surface! The indigenous fauna has adapted well."), 0));
            };
            On.SLOracleBehaviorHasMark.TypeOfMiscItem += (On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject testItem) => (testItem is LizardEgg) ? Register.EggConv : orig(self, testItem);
            On.SaveState.SessionEnded += delegate (On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
            {
                orig(self, game, survived, newMalnourished);
                File.WriteAllText(FilePath, string.Empty);
                for (int i = 0; i < lizards.Count; i++)
                {
                    if (lizards[i].stage - Options.lizGrowthTime.Value > 6)
                    {
                        lizards.RemoveAt(i--);
                        continue;
                    }
                    if (lizards[i].slatedForDeletion)
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
                eggInShelter = self.room != null && self.room.abstractRoom.shelter && self.room.physicalObjects[1].Any(obj => obj is LizardEgg);
            };

            // Lizard AI stuff
            On.LizardAI.DetermineBehavior += delegate (On.LizardAI.orig_DetermineBehavior orig, LizardAI self)
            {
                if (self.lizard.grasps[0] != null && self.lizard.grasps[0].grabbed.abstractPhysicalObject is AbstractLizardEgg egg && egg.parentID == self.creature.ID)
                {
                    self.currentUtility = 1f;
                    return LizardAI.Behavior.ReturnPrey;
                }
                return orig(self);
            };
            On.LizardAI.Update += delegate (On.LizardAI.orig_Update orig, LizardAI self)
            {
                orig(self);
                if (self.creature.GetData() is FCustom.LizardData data && data.egg != null && self.behavior != LizardAI.Behavior.ReturnPrey && self.pathFinder.CoordinateReachableAndGetbackable(data.egg.pos))
                {
                    self.creature.abstractAI.SetDestination(data.egg.pos);
                    self.runSpeed = math.lerp(self.runSpeed, 1f, 0.75f);
                }
            };
            On.Lizard.CarryObject += delegate (On.Lizard.orig_CarryObject orig, Lizard self, bool eu)
            {
                if (self?.grasps[0]?.grabbed == null)
                    return;
                if ((self.grasps[0].grabbed.abstractPhysicalObject as AbstractLizardEgg)?.parentID == self.abstractCreature.ID && self.Consious)
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
                    if (self.lizard.abstractCreature.GetData() is FCustom.LizardData data && data.egg != null && data.egg.Room == self.lizard.room.abstractRoom && self.lizard.Consious)
                        return Color.Lerp(self.HeadColor1, self.HeadColor2, (data.egg.realizedObject as LizardEgg).Luminance);
                }
                catch { }
                return orig(self, timeStacker);
            };
            On.Lizard.ctor += delegate (On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
            {
                orig(self, abstractCreature, world);
                foreach (SavedLizard saved in lizards)
                {
                    if (saved.ID == self.abstractCreature.ID && Options.colorInheritance.Value)
                    {
                        self.effectColor = saved.color;
                        break;
                    }
                }
            };
            On.LizardGraphics.ctor += delegate (On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
            {
                orig(self, ow);
                if (!Options.youngLiz.Value)
                    return;
                foreach (SavedLizard saved in lizards)
                {
                    if (saved.ID == ow.abstractPhysicalObject.ID && saved.stage < Options.lizGrowthTime.Value)
                    {
                        float num = (float)saved.stage / Options.lizGrowthTime.Value;
                        self.iVars.fatness = self.iVars.fatness * 0.4f + 0.6f * num;
                        self.iVars.headSize = self.iVars.headSize * 0.6f + 0.4f * num;
                        self.iVars.tailLength = self.iVars.tailLength * 0.5f + 0.5f * num;
                        break;
                    }
                }
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
                {
                    int data = (item is AbstractLizardEgg egg) ? ((FCustom.ColorToInt(egg.color) == 0) ? 1 : FCustom.ColorToInt(egg.color)) : 0;
                    return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, data);
                }
                return orig(item);
            };
            On.ItemSymbol.SpriteNameForItem += (On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData) => itemType == Register.LizardEgg ? "HipsA" : orig(itemType, intData);

            // Major
            On.Lizard.Update += Lizard_Update;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.Player.DirectIntoHoles += Player_DirectIntoHoles;
        }

        private void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            if (self.abstractCreature.InDen)
                return;
            foreach (SavedLizard saved in lizards)
            {
                if (saved.ID == self.abstractCreature.ID)
                {
                    if (self.AI.friendTracker.friend == null && self.room.PlayersInRoom.Count > 0 && self.Consious)
                    {
                        Player player = self.room.PlayersInRoom[0];
                        SocialMemory.Relationship relationship = self.abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
                        relationship.InfluenceKnow(Mathf.Abs(0.85f) * 0.25f);
                        relationship.tempLike = 1f;
                        relationship.like = 1f;
                        self.AI.friendTracker.friend = player;
                        self.AI.friendTracker.friendRel = relationship;
                    }
                    if (self.State.dead)
                        saved.slatedForDeletion = !self.room.abstractRoom.shelter;
                    break;
                }
            }
            if (Options.tamedAggressiveness.Value || !(self.AI.friendTracker.friend is Player))
            {
                foreach (PhysicalObject obj in self.room.physicalObjects[1])
                {
                    if (self.abstractCreature.GetData() is FCustom.LizardData data)
                    {
                        if (!data.sawPlayerWithEgg && obj is Player player && self.AI.VisualContact(player.mainBodyChunk) && player.grasps[0] != null && player.grasps[0].grabbed is LizardEgg && (player.grasps[0].grabbed as LizardEgg).AbstractLizardEgg.parentID == self.abstractCreature.ID)
                        {
                            data.sawPlayerWithEgg = true;
                            self.AI.LizardPlayerRelationChange(-Options.lizAggressiveness.Value, player.abstractCreature);
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
                        else if (obj is LizardEgg egg && egg.AbstractLizardEgg.parentID == self.abstractCreature.ID)
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
            }
            if (self.grasps[0]?.grabbed is LizardEgg && self.enteringShortCut != null && self.room.shortcutData(self.enteringShortCut.Value).shortCutType == ShortcutData.Type.CreatureHole && self.abstractCreature.spawnDen.abstractNode == self.room.shortcutData(self.enteringShortCut.Value).destNode && self.abstractCreature.GetData() is FCustom.LizardData lData)
            {
                PhysicalObject egg = self.grasps[0].grabbed;
                self.LoseAllGrasps();
                self.room.RemoveObject(egg);
                egg.Destroy();
                self.AI.behavior = LizardAI.Behavior.Idle;
                lData.egg = null;
                if (EggsInDen.ContainsKey(self.abstractCreature.spawnDen))
                {
                    FCustom.ChangeDictTuple(EggsInDen, self.abstractCreature.spawnDen, 1);
                    self.room.AddObject(new Indicator(self.abstractCreature.spawnDen, self.room));
                }
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
                    if (abstr.creatureTemplate.IsLizard)
                        AddToEggsDict(newRoom, abstr);
            foreach (var den in EggsInDen)
                if (newRoom.abstractRoom.index == den.Key.room && den.Value.Item2 > 0)
                    newRoom.AddObject(new Indicator(den.Key, self.room));
        }

        private AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            try
            {
                string[] array = objString.Split(new[] { "<oA>" }, StringSplitOptions.None);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = new AbstractPhysicalObject.AbstractObjectType(array[1]);
                if (abstractObjectType == Register.LizardEgg)
                {
                    int stage = ((world.rainCycle?.timer == null || world.rainCycle.timer < 40) && world.GetAbstractRoom(WorldCoordinate.FromString(array[2])).shelter) ? int.Parse(array[6]) + 1 : int.Parse(array[6]);
                    if (stage >= Options.eggGrowthTime.Value)
                        return SpawnLizard(world, array);
                    return new AbstractLizardEgg(world, WorldCoordinate.FromString(array[2]), EntityID.FromString(array[0]), EntityID.FromString(array[5]), float.Parse(array[4]), FCustom.IntToColor(int.Parse(array[3])), array[7], stage, true)
                    { unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8) };
                }
            }
            catch (Exception ex) { Debug.LogException(ex); }
            return orig(world, objString);
        }

        private void Player_DirectIntoHoles(On.Player.orig_DirectIntoHoles orig, Player self)
        {
            orig(self);
            IntVector2 tile = self.room.GetTilePosition(self.mainBodyChunk.pos + new Vector2(40f * self.input[0].x, 40f * self.input[0].y));
            if (self.room.shortcutData(tile).shortCutType != ShortcutData.Type.CreatureHole)
                return;
            WorldCoordinate den = self.room.GetWorldCoordinate(tile);
            den.abstractNode = FCustom.GetAbstractNode(den, self.room);
            den.Tile = new IntVector2(-1, -1);
            if (EggsInDen.TryGetValue(den, out var val) && val.Item2 > 0 && self.FreeHand() != -1 && self.input[0].pckp)
            {
                Lizard liz = (val.Item1?.realizedCreature as Lizard) ?? new Lizard(val.Item1, self.room.world);
                float size = (liz.lizardParams.bodyMass > 5f) ? 0.5f * liz.lizardParams.bodyMass : liz.lizardParams.bodyMass;
                AbstractLizardEgg abstractEgg = new AbstractLizardEgg(self.room.world, self.room.GetWorldCoordinate(self.firstChunk.pos), self.room.game.GetNewID(), liz.abstractCreature.ID, size, liz.effectColor, liz.Template.name);
                self.abstractCreature.Room.AddEntity(abstractEgg);
                abstractEgg.RealizeInRoom();
                self.SlugcatGrab(abstractEgg.realizedObject, self.FreeHand());
                FCustom.ChangeDictTuple(EggsInDen, den, -1);
                if (--val.Item2 == 0)
                    foreach (IDrawable drawable in self.room.drawableObjects)
                        if (drawable is Indicator ind && ind.den == den)
                        {
                            self.room.RemoveObject(ind);
                            return;
                        }
            }
        }

        public static AbstractCreature SpawnLizard(World world, string[] array)
        {
            FCustom.InitLizTypes();
            string parentType = array[7];
            WorldCoordinate pos = WorldCoordinate.FromString(array[2]);
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(EntityID.FromString(array[0]).RandomSeed);
            EntityID lizID = world.game.GetNewID(EntityID.FromString(array[5]).spawner);
            Color color = FCustom.IntToColor(int.Parse(array[3]));

            AbstractCreature abstrLizard;
            if (ModManager.MSC && Options.trLizOpport.Value && parentType == "Red Lizard" && UnityEngine.Random.value < 0.1f)
                abstrLizard = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard), null, pos, lizID);
            else abstrLizard = new AbstractCreature(world, (parentType == "") ? FCustom.lizTypes[UnityEngine.Random.Range(0, FCustom.lizTypes.Count)] : StaticWorld.GetCreatureTemplate(parentType), null, pos, lizID);
            UnityEngine.Random.state = state;

            if ((Options.colorInheritance.Value || Options.youngLiz.Value) && !lizards.Any(liz => liz.ID == lizID))
            {
                SavedLizard savedLizard = new SavedLizard(lizID, color, 0);
                File.AppendAllText(FilePath, $"{savedLizard}\n");
                lizards.Add(savedLizard);
            }
            return abstrLizard;
        }

        public static bool AddToEggsDict(Room room, AbstractCreature abstr)
        {
            if (EggsInDen.ContainsKey(abstr.spawnDen) || !abstr.spawnDen.NodeDefined || abstr.creatureTemplate.name == "YoungLizard")
                return false;
            EggsInDen.Add(abstr.spawnDen, (abstr, 0));
            int amount = (room.world.GetSpawner(abstr.ID) as World.SimpleSpawner)?.amount ?? 1;
            float chance = FCustom.EggSpawnChance((room.world.game.Players[0].realizedCreature as Player).playerState.slugcatCharacter);
            if (Options.occurrenceFrequency.Value > 1f)
                chance += (1f - chance) * (1f - 1f / Options.occurrenceFrequency.Value);
            else chance *= Options.occurrenceFrequency.Value;
            for (int i = 0; i < amount; i++)
                if (UnityEngine.Random.value < chance)
                    FCustom.ChangeDictTuple(EggsInDen, abstr.spawnDen, 1);
            return true;
        }

        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> EggsInDen { get; private set; }
        public static List<SavedLizard> lizards;
        public bool eggInShelter;
        public float eggMotherProgress = 0f;

        public class SavedLizard
        {
            public SavedLizard(EntityID ID, Color color, int stage)
            {
                this.ID = ID;
                this.color = color;
                this.stage = stage;
                slatedForDeletion = false;
            }

            public override string ToString() => $"{ID}~{FCustom.ColorToInt(color)}~{stage}";

            public static SavedLizard FromString(string s)
            {
                string[] array = s.Split('~');
                EntityID ID = EntityID.FromString(array[0]);
                Color color = FCustom.IntToColor(int.Parse(array[1]));
                int stage = int.Parse(array[2]);
                return new SavedLizard(ID, color, stage);
            }

            public EntityID ID;
            public Color color;
            public int stage;
            public bool slatedForDeletion;
        }
    }
}