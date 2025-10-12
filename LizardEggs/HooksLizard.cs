using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace LizardEggs
{
    public static class HooksLizard
    {
        public static void Init()
        {
            //AI
            On.LizardAI.ctor += LizardAI_ctor;
            On.LizardAI.DetermineBehavior += LizardAI_DetermineBehavior;
            On.LizardAI.Update += LizardAI_Update;
            On.YellowAI.Pack += YellowAI_Pack;
            //Lizard
            On.Lizard.Update += Lizard_Update;
            On.Lizard.ctor += Lizard_ctor;
            On.Lizard.CarryObject += Lizard_CarryObject;
            //Graphics
            On.LizardGraphics.HeadColor += LizardGraphics_HeadColor;
            On.LizardGraphics.ctor += LizardGraphics_ctor;
            On.LizardCosmetics.SpineSpikes.ctor += SpineSpikes_ctor;
            //Creature
            On.AbstractCreature.ctor += AbstractCreature_ctor1;
            On.SaveState.AbstractCreatureFromString += SaveState_AbstractCreatureFromString;
        }

        private static AbstractCreature SaveState_AbstractCreatureFromString(On.SaveState.orig_AbstractCreatureFromString orig, World world, string creatureString, bool onlyInCurrentRegion, WorldCoordinate overrideCoord)
        {
            AbstractCreature creature = orig(world, creatureString, onlyInCurrentRegion, overrideCoord);
            if (creature.state is BabyLizardState babyState)
            {
                if (babyState.age >= Options.lizGrowthTime.Value)
                {
                    creature.creatureTemplate = StaticWorld.GetCreatureTemplate(babyState.parent);
                    creature.state = new LizardState(creature);
                    if (creature.GetData() is FDataMananger.LizardData data)
                        data.playerIsParent = true;
                }
                else babyState.LimbFix();
            }
            return creature;
        }

        private static void AbstractCreature_ctor1(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            if (creatureTemplate.type == Register.BabyLizard)
                self.state = new BabyLizardState(self);
        }

        private static void SpineSpikes_ctor(On.LizardCosmetics.SpineSpikes.orig_ctor orig, LizardCosmetics.SpineSpikes self, LizardGraphics lGraphics, int startSprite)
        {
            orig(self, lGraphics, startSprite);
            if (ModManager.MSC && lGraphics.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard && lGraphics.lizard.State is BabyLizardState)
            {
                self.sizeRangeMin = Mathf.Lerp(self.sizeRangeMin, 1.1f, 0.1f);
                self.sizeRangeMax = Mathf.Lerp(self.sizeRangeMax, 1.1f, 0.4f);
            }
        }

        private static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            if ((ow as Lizard).State is BabyLizardState state)
            {
                (ow as Lizard).Template.type = state.parent;
                orig(self, ow);
                (ow as Lizard).Template.type = Register.BabyLizard;
            }
            else orig(self, ow);
        }

        private static void LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            if (creature.state is BabyLizardState state)
            {
                creature.creatureTemplate.type = state.parent;
                orig(self, creature, world);
                creature.creatureTemplate.type = Register.BabyLizard;
            }
            else orig(self, creature, world);
        }

        private static YellowAI.YellowPack YellowAI_Pack(On.YellowAI.orig_Pack orig, YellowAI self, Creature liz)
        {
            if ((liz.abstractCreature.abstractAI.RealAI as LizardAI)?.yellowAI?.pack == null)
                return self.pack;
            return orig(self, liz);
        }

        private static LizardAI.Behavior LizardAI_DetermineBehavior(On.LizardAI.orig_DetermineBehavior orig, LizardAI self)
        {
            if (self.lizard.grasps[0]?.grabbed.abstractPhysicalObject is AbstractLizardEgg egg && egg.parentID == self.creature.ID)
            {
                self.currentUtility = 1f;
                return LizardAI.Behavior.ReturnPrey;
            }
            return orig(self);
        }

        private static void LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
        {
            orig(self);
            if (self.creature.GetData() is FDataMananger.LizardData data && data.egg != null && self.behavior != LizardAI.Behavior.ReturnPrey && self.pathFinder.CoordinateReachableAndGetbackable(data.egg.pos))
            {
                self.creature.abstractAI.SetDestination(data.egg.pos);
                self.runSpeed = Mathf.Lerp(self.runSpeed, 1.2f, 0.75f);
            }
        }

        private static void Lizard_CarryObject(On.Lizard.orig_CarryObject orig, Lizard self, bool eu)
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
        }

        private static void Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            if (abstractCreature.state is BabyLizardState state)
            {
                abstractCreature.creatureTemplate.type = state.parent;
                orig(self, abstractCreature, world);
                abstractCreature.creatureTemplate.type = Register.BabyLizard;
                if (self.effectColor == (abstractCreature.creatureTemplate.breedParameters as LizardBreedParams).standardColor)
                    self.effectColor = (StaticWorld.GetCreatureTemplate(state.parent).breedParameters as LizardBreedParams).standardColor;
            }
            else orig(self, abstractCreature, world);
        }

        private static Color LizardGraphics_HeadColor(On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
        {
            try
            {
                if (self.lizard != null && self.lizard.abstractCreature.GetData() is FDataMananger.LizardData data && data.egg != null && data.egg.Room == self.lizard.room.abstractRoom && data.egg.realizedObject != null && self.lizard.Consious)
                    return Color.Lerp(self.HeadColor1, self.HeadColor2, (data.egg.realizedObject as LizardEgg).Luminance);
            }
            catch { }
            return orig(self, timeStacker);
        }

        private static void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            if (self.abstractCreature.InDen)
                return;
            if (self.State is BabyLizardState || (self.abstractCreature.GetData() is FDataMananger.LizardData lisData && lisData.playerIsParent))
            {
                if (self.AI.friendTracker.friend == null && self.room.PlayersInRoom.Count > 0 && self.room.game.FirstAlivePlayer.realizedCreature is Player player && player != null && self.Consious)
                {
                    SocialMemory.Relationship relationship = self.abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
                    relationship.InfluenceKnow(Mathf.Abs(0.85f) * 0.25f);
                    relationship.tempLike = 1f;
                    relationship.like = 1f;
                    self.AI.friendTracker.friend = player;
                    self.AI.friendTracker.friendRel = relationship;
                }
            }
            if ((Options.tamedAggressiveness.Value || !(self.AI.friendTracker.friend is Player)) && self.abstractCreature.GetData() is FDataMananger.LizardData data)
            {
                foreach (PhysicalObject obj in self.room.physicalObjects[1])
                {
                    if (!data.sawPlayerWithEgg && obj is Player player && self.AI.VisualContact(player.mainBodyChunk) && player.grasps[0]?.grabbed is LizardEgg egg && egg.AbstractLizardEgg.parentID == self.abstractCreature.ID)
                    {
                        data.sawPlayerWithEgg = true;
                        self.AI.LizardPlayerRelationChange(-Options.lizAggressiveness.Value, player.abstractCreature);
                        var personality = self.abstractCreature.personality;
                        if (Mathf.Lerp(personality.sympathy, personality.aggression, personality.nervous) > 2f * personality.sympathy * personality.bravery)
                        {
                            self.AI.behavior = LizardAI.Behavior.Hunt;
                            self.AI.preyTracker.currentPrey = new PreyTracker.TrackedPrey(self.AI.preyTracker, self.AI.CreateTrackerRepresentationForCreature(player.abstractCreature));
                            self.animation = Lizard.Animation.HearSound;
                            self.timeToRemainInAnimation = 40;
                            self.bubbleIntensity = 0.3f;
                        }
                    }
                    else if (obj is LizardEgg eggg && eggg.AbstractLizardEgg.parentID == self.abstractCreature.ID)
                    {
                        if (data.egg == null)
                            data.egg = eggg.abstractPhysicalObject as AbstractLizardEgg;
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
            if (self.grasps[0]?.grabbed is LizardEgg && self.enteringShortCut != null && self.room.shortcutData(self.enteringShortCut.Value).shortCutType == ShortcutData.Type.CreatureHole && self.abstractCreature.spawnDen.abstractNode == self.room.shortcutData(self.enteringShortCut.Value).destNode && self.abstractCreature.GetData() is FDataMananger.LizardData lData)
            {
                PhysicalObject egg = self.grasps[0].grabbed;
                self.LoseAllGrasps();
                self.room.RemoveObject(egg);
                egg.Destroy();
                self.AI.behavior = LizardAI.Behavior.Idle;
                lData.egg = null;
                if (FDataMananger.Dens.ContainsKey(self.abstractCreature.spawnDen))
                {
                    FDataMananger.ChangeDensValue(self.abstractCreature.spawnDen, 1);
                    self.room.AddObject(new Indicator(self.abstractCreature.spawnDen, self.room));
                }
            }
            orig(self, eu);
        }
    }
}
