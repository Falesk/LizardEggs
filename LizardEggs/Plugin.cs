using BepInEx;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;

namespace LizardEggs
{
    [BepInPlugin(GUID, Name, Version)]
    class Plugin : BaseUnityPlugin
    {
        public const string GUID = "falesk.lizardeggs";
        public const string Name = "Lizard Eggs";
        public const string Version = "1.1.4";
        public void Awake()
        {
            // Mod Init / Deinit
            On.RainWorld.OnModsInit += delegate (On.RainWorld.orig_OnModsInit orig, RainWorld self)
            {
                orig(self);
                Register.RegisterValues();
                MachineConnector.SetRegisteredOI(GUID, new Options());
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

            // Icon
            On.ItemSymbol.ColorForItem += delegate (On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
            {
                if (itemType == Register.LizardEgg)
                    return Color.Lerp((intData == 0) ? Color.green : FCustom.IntToColor(intData), Color.black, 0.4f);
                return orig(itemType, intData);
            };
            On.ItemSymbol.SymbolDataFromItem += delegate (On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
            {
                if (item is AbstractLizardEgg egg)
                    return new IconSymbol.IconSymbolData?(new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, FCustom.ColorToInt(egg.color)));
                if (item.type == Register.LizardEgg)
                    return new IconSymbol.IconSymbolData?(new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, 0));
                return orig(item);
            };
            On.ItemSymbol.SpriteNameForItem += (On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData) => itemType == Register.LizardEgg ? "HipsA" : orig(itemType, intData);

            //Baby Lizard
            On.WorldLoader.CreatureTypeFromString += (On.WorldLoader.orig_CreatureTypeFromString orig, string s) => s.ToLower() == "baby lizard" ? Register.BabyLizard : orig(s);
            On.LizardAI.ComfortableIdlePosition += (On.LizardAI.orig_ComfortableIdlePosition orig, LizardAI self) => self.lizard.Template.type == Register.BabyLizard ? self.lizard.room.GetTilePosition(self.lizard.bodyChunks[0].pos).x == self.lizard.room.GetTilePosition(self.lizard.bodyChunks[2].pos).x : orig(self);
            On.Lizard.ctor += delegate (On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
            {
                orig(self, abstractCreature, world);
                if (self.abstractCreature is AbstractBabyLizard baby)
                    self.effectColor = (baby.parent.breedParameters as LizardBreedParams).standardColor;
            };
            On.StaticWorld.InitCustomTemplates += StaticWorld_InitCustomTemplates;
            On.StaticWorld.InitStaticWorld += StaticWorld_InitStaticWorld;
            On.SaveState.AbstractCreatureFromString += SaveState_AbstractCreatureFromString;

            // Major
            On.Lizard.Update += Lizard_Update;
            On.RoomCamera.ChangeRoom += RoomCamera_ChangeRoom;
            On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            On.Player.DirectIntoHoles += Player_DirectIntoHoles;

            On.RoomPreprocessor.DecompressStringToAImaps += RoomPreprocessor_DecompressStringToAImaps;
        }

        private CreatureSpecificAImap[] RoomPreprocessor_DecompressStringToAImaps(On.RoomPreprocessor.orig_DecompressStringToAImaps orig, string s, AImap aimap)
        {
            CreatureSpecificAImap[] array = new CreatureSpecificAImap[StaticWorld.preBakedPathingCreatures.Length];
            if (s == null)
            {
                Custom.LogWarning(new string[]
                {
                "AI MAP STRING WAS NULL!"
                });
                for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
                {
                    array[i] = new CreatureSpecificAImap(aimap, StaticWorld.preBakedPathingCreatures[i]);
                }
                return array;
            }
            string[] array2 = Regex.Split(s, "<<DIV - A>>");
            aimap.SetVisibilityMapFromCompressedArray(RoomPreprocessor.StringToIntArray(array2[0]));
            for (int j = 0; j < StaticWorld.preBakedPathingCreatures.Length; j++)
            {
                array[j] = new CreatureSpecificAImap(aimap, StaticWorld.preBakedPathingCreatures[j]);
                try
                {
                    int[] intArray = RoomPreprocessor.StringToIntArray(Regex.Split(array2[j + 1], "<<DIV - B>>")[0]);
                    float[] floatArray = RoomPreprocessor.StringToFloatArray(Regex.Split(array2[j + 1], "<<DIV - B>>")[1]);
                    array[j].LoadFromCompressedIntGrid(intArray);
                    array[j].LoadFromCompressedFloatGrid(floatArray);
                }
                catch (FormatException)
                {
                    Custom.LogWarning(new string[]
                    {
                    "AI MAP STRING WAS IN THE WRONG FORMAT:",
                    array2[j + 1]
                    });
                }
            }
            return array;
        }

        private void StaticWorld_InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
        {
            orig();
            CreatureTemplate ancestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate);
            List<TileTypeResistance> tileResistances = new List<TileTypeResistance>()
            {
                new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Corridor, 1.2f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Climb, 0.8f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Wall, 1f, PathCost.Legality.Allowed),
                new TileTypeResistance(AItile.Accessibility.Ceiling, 1.2f, PathCost.Legality.Allowed)
            };
            List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>()
            {
                new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 20f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 2f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.ShortCut, 15f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.ReachOverGap, 1.1f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.ReachUp, 1.1f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.ReachDown, 1.1f, PathCost.Legality.Allowed),
                new TileConnectionResistance(MovementConnection.MovementType.CeilingSlope, 2f, PathCost.Legality.Allowed)
            };
            LizardBreedParams babyBreedParams = new LizardBreedParams(Register.BabyLizard)
            {
                bodyRadFac = 1f,
                pullDownFac = 1f,
                bodyLengthFac = 1f,
                biteDelay = 14,
                biteInFront = 20f,
                biteHomingSpeed = 1.4f,
                biteChance = 0.4f,
                attemptBiteRadius = 90f,
                getFreeBiteChance = 0.5f,
                biteDamage = 0.7f,
                biteDamageChance = 0.2f,
                toughness = 0.5f,
                stunToughness = 0.75f,
                regainFootingCounter = 4,
                baseSpeed = 3.2f,
                bodyMass = 1.4f,
                bodySizeFac = 0.9f,
                floorLeverage = 1f,
                maxMusclePower = 2f,
                danger = 0.35f,
                aggressionCurveExponent = 0.875f,
                wiggleSpeed = 1f,
                wiggleDelay = 15,
                bodyStiffnes = 0f,
                swimSpeed = 0.35f,
                idleCounterSubtractWhenCloseToIdlePos = 0,
                headShieldAngle = 100f,
                canExitLounge = true,
                canExitLoungeWarmUp = true,
                findLoungeDirection = 1f,
                loungeDistance = 80f,
                preLoungeCrouch = 35,
                preLoungeCrouchMovement = -0.3f,
                loungeSpeed = 2.5f,
                loungeMaximumFrames = 20,
                loungePropulsionFrames = 8,
                loungeJumpyness = 0.9f,
                loungeDelay = 310,
                riskOfDoubleLoungeDelay = 0.8f,
                postLoungeStun = 20,
                loungeTendensy = 0.01f,
                perfectVisionAngle = Mathf.Lerp(1f, -1f, 0.055555556f),
                periferalVisionAngle = Mathf.Lerp(1f, -1f, 0.45833334f),
                biteDominance = 0.1f,
                limbSize = 0.9f,
                limbThickness = 1f,
                stepLength = 0.4f,
                liftFeet = 0f,
                feetDown = 0f,
                noGripSpeed = 0.2f,
                limbSpeed = 6f,
                limbQuickness = 0.6f,
                limbGripDelay = 1,
                smoothenLegMovement = true,
                legPairDisplacement = 0f,
                standardColor = new Color(0f, 0.5f, 1f),
                walkBob = 0.4f,
                tailSegments = 4,
                tailStiffness = 400f,
                tailStiffnessDecline = 0.1f,
                tailLengthFactor = 1f,
                tailColorationStart = 0.1f,
                tailColorationExponent = 1.2f,
                headSize = 0.9f,
                neckStiffness = 0f,
                jawOpenAngle = 105f,
                jawOpenLowerJawFac = 0.55f,
                jawOpenMoveJawsApart = 20f,
                headGraphics = new int[5],
                framesBetweenLookFocusChange = 20,
                tamingDifficulty = 1.1f,
                terrainSpeeds = new LizardBreedParams.SpeedMultiplier[Enum.GetNames(typeof(AItile.Accessibility)).Length]
            };
            for (int i = 0; i < babyBreedParams.terrainSpeeds.Length; i++)
                babyBreedParams.terrainSpeeds[i] = new LizardBreedParams.SpeedMultiplier(0.1f, 1f, 1f, 1f);
            babyBreedParams.terrainSpeeds[1] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
            babyBreedParams.terrainSpeeds[2] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
            babyBreedParams.terrainSpeeds[3] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
            babyBreedParams.terrainSpeeds[4] = new LizardBreedParams.SpeedMultiplier(0.8f, 1f, 1f, 1f);
            babyBreedParams.terrainSpeeds[5] = new LizardBreedParams.SpeedMultiplier(0.6f, 1f, 1f, 1f);
            CreatureTemplate babyTemplate = new CreatureTemplate(Register.BabyLizard, ancestor, tileResistances, connectionResistances, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
            {
                name = "Baby Lizard",
                breedParameters = babyBreedParams,
                baseDamageResistance = babyBreedParams.toughness * 2f,
                meatPoints = 4,
                visualRadius = 850f,
                waterVision = 0.35f,
                throughSurfaceVision = 0.65f,
                movementBasedVision = 0.3f,
                waterPathingResistance = 5f,
                dangerousToPlayer = babyBreedParams.danger,
                virtualCreature = false,
                throwAction = "Hiss",
                pickupAction = "Bite",
                //doPreBakedPathing = true
            };
            babyTemplate.damageRestistances[(int)Creature.DamageType.Bite, 0] = 2.5f;
            babyTemplate.damageRestistances[(int)Creature.DamageType.Bite, 1] = 3f;
            try { StaticWorld.creatureTemplates[babyTemplate.type.Index] = babyTemplate; }
            catch (Exception ex) { Debug.LogException(ex); Debug.Log("Exception"); }
        }

        private void StaticWorld_InitStaticWorld(On.StaticWorld.orig_InitStaticWorld orig)
        {
            orig();
            StaticWorld.EstablishRelationship(Register.BabyLizard, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.6f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.2f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.BigEel, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.2f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.3f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.BrotherLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.2f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.33f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.CicadaA, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.CicadaB, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.DaddyLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.22f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.EggBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.45f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Hazer, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.31f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 0.1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.KingVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.LanternMouse, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.RedCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.5f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.SmallCentipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.SmallNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.7f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.TentaclePlant, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.15f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.TubeWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.45f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Vulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.8f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Fly, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Scavenger, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.7f));
            if (ModManager.MSC)
            {
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.4f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.05f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.85f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.45f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.12f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.35f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.6f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.Yeek, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.85f));
            }
        }

        private AbstractCreature SaveState_AbstractCreatureFromString(On.SaveState.orig_AbstractCreatureFromString orig, World world, string creatureString, bool onlyInCurrentRegion)
        {
            try
            {
                string[] array = creatureString.Split(new[] { "<cA>" }, StringSplitOptions.None);
                CreatureTemplate.Type type = new CreatureTemplate.Type(array[0]);
                if (type == Register.BabyLizard)
                {
                    CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(Register.BabyLizard);
                    WorldCoordinate pos = WorldCoordinate.FromString(array[1]);
                    EntityID ID = EntityID.FromString(array[2]);
                    CreatureTemplate parent = StaticWorld.GetCreatureTemplate(array[3]);
                    int stage = (world.rainCycle.timer < 40 && world.GetAbstractRoom(pos).shelter) ? int.Parse(array[4]) + 1 : int.Parse(array[4]);
                    return new AbstractBabyLizard(world, creatureTemplate, pos, ID, parent, stage)
                    { unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5) };
                }
            }
            catch(Exception ex) { Debug.LogException(ex); }
            return orig(world, creatureString, onlyInCurrentRegion);
        }

        private void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            if (self.abstractCreature.InDen)
                return;
            foreach (PhysicalObject obj in self.room.physicalObjects[1])
            {
                if (self.abstractCreature.GetData() is FCustom.LizardData data && !Options.tamedAggressiveness.Value)
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
            if (self.grasps[0]?.grabbed != null && self.grasps[0].grabbed is LizardEgg && self.enteringShortCut != null && self.room.shortcutData(self.enteringShortCut.Value).shortCutType == ShortcutData.Type.CreatureHole && self.abstractCreature.spawnDen.abstractNode == self.room.shortcutData(self.enteringShortCut.Value).destNode && self.abstractCreature.GetData() is FCustom.LizardData data1)
            {
                PhysicalObject egg = self.grasps[0].grabbed;
                self.LoseAllGrasps();
                egg.Destroy();
                self.AI.behavior = LizardAI.Behavior.Idle;
                data1.egg = null;
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
                    if (int.Parse(array[6]) > Options.eggGrowthTime.Value)
                        return null;
                    int stage = (world.rainCycle.timer < 40 && world.GetAbstractRoom(WorldCoordinate.FromString(array[2])).shelter) ? int.Parse(array[6]) + 1 : int.Parse(array[6]);
                    return new AbstractLizardEgg(world, WorldCoordinate.FromString(array[2]), EntityID.FromString(array[0]), EntityID.FromString(array[5]), float.Parse(array[4]), FCustom.IntToColor(int.Parse(array[3])), array[7], stage, true)
                    { unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 8) };
                }
            }
            catch(Exception ex) { Debug.LogException(ex); }
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
                            self.room.RemoveObject(ind);
            }
        }

        public bool AddToEggsDict(Room room, AbstractCreature abstr)
        {
            if (EggsInDen.ContainsKey(abstr.spawnDen) || !abstr.spawnDen.NodeDefined || abstr.creatureTemplate.name == "YoungLizard")
                return false;
            EggsInDen.Add(abstr.spawnDen, (abstr, 0));
            int amount = (room.world.GetSpawner(abstr.ID) as World.SimpleSpawner)?.amount ?? 1;
            float chance = FCustom.EggSpawnChance((room.world.game.Players[0].realizedCreature as Player).playerState.slugcatCharacter);
            if (Options.occurrenceFrequency.Value > 1f)
                chance += (1 - chance) * (1 - 1 / Options.occurrenceFrequency.Value);
            else chance *= Options.occurrenceFrequency.Value;
            for (int i = 0; i < amount; i++)
                if (UnityEngine.Random.value < chance)
                    FCustom.ChangeDictTuple(EggsInDen, abstr.spawnDen, 1);
            return true;
        }

        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> EggsInDen { get; private set; }
        public bool eggInShelter;
        public float eggMotherProgress = 0f;
    }
}