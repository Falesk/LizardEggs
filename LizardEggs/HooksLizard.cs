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
            if ((creature.state is BabyLizardState || (self.lizard?.abstractCreature.GetData() is FDataManager.LizardData data && data.playerIsParent)) && self.denFinder.denPosition == null && creature.Room.shelter)
                self.denFinder.denPosition = new WorldCoordinate(creature.Room.index, -1, -1, -1);
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
            if (self.creature.GetData() is FDataManager.LizardData data && data.egg != null && self.behavior != LizardAI.Behavior.ReturnPrey && self.pathFinder.CoordinateReachableAndGetbackable(data.egg.pos))
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
            if ((self.State is BabyLizardState || (self.abstractCreature.GetData() is FDataManager.LizardData data && data.playerIsParent)) && self.grasps[0]?.grabbed is Creature crt)
            {
                if (crt.State.alive && Random.value < 1 / 200f)
                    crt.Die();
                else if (Random.value < 1 / 200f)
                {
                    for (int i = 0; i < crt.bodyChunks.Length; i++)
                        crt.bodyChunks[i].rad /= 2;
                    self.room.PlaySound(SoundID.Slugcat_Bite_Slime_Mold, self.firstChunk.pos, 2.5f, 0.5f);
                    for (int i = 0; i < 4 + Random.Range(0, 5); i++)
                        self.room.AddObject(new WaterDrip(self.firstChunk.pos, -self.firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * self.firstChunk.vel.magnitude * Random.value * 0.5f, false));
                    if (Random.value < 1 / 3f)
                    {
                        crt.Destroy();
                        self.LoseAllGrasps();
                        for (int i = 0; i < 4 + Random.Range(0, 5); i++)
                            self.room.AddObject(new WaterDrip(self.firstChunk.pos, -self.firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * self.firstChunk.vel.magnitude * Random.value * 0.5f, false));
                    }
                }
            }
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
            if (self.lizard != null && self.lizard.abstractCreature.GetData() is FDataManager.LizardData data && data.egg != null && data.egg.Room == self.lizard.room?.abstractRoom && data.egg.realizedObject != null && self.lizard.Consious)
                return Color.Lerp(self.HeadColor1, self.HeadColor2, (data.egg.realizedObject as LizardEgg).Luminance);
            return orig(self, timeStacker);
        }

        private static void Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
        {
            if (self.abstractCreature.InDen)
                return;
            Lizard_Update_Relationships(self);
            Lizard_Update_EggDefence(self, eu);
            Lizard_Update_ReturningEgg(self);
            orig(self, eu);
        }

        private static void Lizard_Update_Relationships(Lizard self)
        {
            bool friendFlag = self.State is BabyLizardState || (self.abstractCreature.GetData() is FDataManager.LizardData lisData && lisData.playerIsParent);
            if (friendFlag && self.AI.friendTracker.friend == null && self.room.PlayersInRoom.Count > 0 && self.room.game.FirstAlivePlayer.realizedCreature is Player player && player != null && self.Consious)
            {
                SocialMemory.Relationship relationship = self.abstractCreature.state.socialMemory.GetOrInitiateRelationship(player.abstractCreature.ID);
                relationship.InfluenceKnow(Mathf.Abs(0.85f) * 0.25f);
                relationship.tempLike = 1f;
                relationship.like = 1f;
                self.AI.friendTracker.friend = player;
                self.AI.friendTracker.friendRel = relationship;
            }
        }

        private static void Lizard_Update_EggDefence(Lizard self, bool eu)
        {
            if ((Options.tamedAggressiveness.Value || !(self.AI.friendTracker.friend is Player)) && self.abstractCreature.GetData() is FDataManager.LizardData data)
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
        }

        private static void Lizard_Update_ReturningEgg(Lizard self)
        {
            if (self.grasps[0]?.grabbed is LizardEgg egg && self.enteringShortCut != null && self.room.shortcutData(self.enteringShortCut.Value).shortCutType == ShortcutData.Type.CreatureHole && self.abstractCreature.spawnDen.abstractNode == self.room.shortcutData(self.enteringShortCut.Value).destNode && self.abstractCreature.GetData() is FDataManager.LizardData data)
            {
                self.LoseAllGrasps();
                self.room.RemoveObject(egg);
                egg.Destroy();
                self.AI.behavior = LizardAI.Behavior.Idle;
                data.egg = null;
                if (FDataManager.Dens.ContainsKey(self.abstractCreature.spawnDen))
                {
                    FDataManager.ChangeDensValue(self.abstractCreature.spawnDen, 1);
                    self.room.AddObject(new Indicator(self.abstractCreature.spawnDen, self.room));
                }
            }
        }
    }
}
