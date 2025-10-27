using System;
using System.Collections.Generic;
using UnityEngine;
using MoreSlugcats;

namespace LizardEggs
{
    public static class HooksBL
    {
        public static void Init()
        {
            //Template
            On.StaticWorld.InitCustomTemplates += StaticWorld_InitCustomTemplates;
            On.StaticWorld.InitStaticWorldRelationships += StaticWorld_InitStaticWorldRelationships;
            On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate;
            //Creature icon
            On.CreatureSymbol.ColorOfCreature += (orig, iconData) => iconData.critType == Register.BabyLizard ? FCustom.HEX2ARGB((uint)iconData.intData) : orig(iconData);
            On.CreatureSymbol.SpriteNameOfCreature += (orig, iconData) => iconData.critType == Register.BabyLizard ? "Kill_Standard_Lizard" : orig(iconData);
            On.CreatureSymbol.SymbolDataFromCreature += (orig, creature) => creature.creatureTemplate.type == Register.BabyLizard ?
            new IconSymbol.IconSymbolData(creature.creatureTemplate.type, AbstractPhysicalObject.AbstractObjectType.Creature, (int)(creature.state as BabyLizardState).hexColor) : orig(creature);
            //Creature
            On.AbstractCreature.ctor += AbstractCreature_ctor1;
            On.SaveState.AbstractCreatureFromString += SaveState_AbstractCreatureFromString;
            //Other
            On.WorldLoader.CreatureTypeFromString += (orig, s) => s.ToLower() == "babylizard" ? Register.BabyLizard : orig(s);
            On.CreatureTemplate.ctor_Type_CreatureTemplate_List1_List1_Relationship += CreatureTemplate_ctor_Type_CreatureTemplate_List1_List1_Relationship;
        }

        private static AbstractCreature SaveState_AbstractCreatureFromString(On.SaveState.orig_AbstractCreatureFromString orig, World world, string creatureString, bool onlyInCurrentRegion, WorldCoordinate overrideCoord)
        {
            AbstractCreature creature = orig(world, creatureString, onlyInCurrentRegion, overrideCoord);
            if (creature != null && creature.state is BabyLizardState babyState)
            {
                if (babyState.age >= Options.lizGrowthTime.Value)
                {
                    creature.creatureTemplate = StaticWorld.GetCreatureTemplate(babyState.parent);
                    creature.state = new LizardState(creature);
                    if (creature.GetData() is FDataManager.LizardData data)
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

        private static void CreatureTemplate_ctor_Type_CreatureTemplate_List1_List1_Relationship(On.CreatureTemplate.orig_ctor_Type_CreatureTemplate_List1_List1_Relationship orig, CreatureTemplate self, CreatureTemplate.Type type, CreatureTemplate _, List<TileTypeResistance> _1, List<TileConnectionResistance> _2, CreatureTemplate.Relationship _3)
        {
            orig(self, type, _, _1, _2, _3);
            if (type == Register.BabyLizard)
                self.name = "BabyLizard";
        }

        private static void StaticWorld_InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
        {
            orig();
            CreatureTemplate template = LizardBreeds.BreedTemplate(Register.BabyLizard, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
            StaticWorld.creatureTemplates[template.type.Index] = template;
        }

        private static void StaticWorld_InitStaticWorldRelationships(On.StaticWorld.orig_InitStaticWorldRelationships orig)
        {
            orig();
            //BabyLizard Relationships to Creature
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.StayOutOfWay, 0.5f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Slugcat, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.4f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.7f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Scavenger, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.35f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.BigSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.05f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.JetFish, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Centipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.25f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.LizardTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.6f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.BigNeedleWorm, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.DropBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.05f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.MirosBird, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Snail, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.VultureGrub, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Fly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.PoleMimic, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Centiwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.25f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Leech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.4f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.SeaLeech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.45f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.Spider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.2f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.RedLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.3f));
            StaticWorld.EstablishRelationship(Register.BabyLizard, CreatureTemplate.Type.SpitterSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.2f));

            //Creature Relationships to BabyLizard
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.LizardTemplate, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Slugcat, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.BigSpider, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.4f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Centipede, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.SmallCentipede, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.25f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Centiwing, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.CicadaA, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Fly, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Hazer, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.JetFish, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Leech, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Scavenger, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.3f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.SeaLeech, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.1f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.SmallNeedleWorm, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.4f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.Snail, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
            StaticWorld.EstablishRelationship(CreatureTemplate.Type.TubeWorm, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));

            if (ModManager.DLCShared)
            {
                //BabyLizard Relationships to Creature
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.ScavengerElite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.6f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.Inspector, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.TerrorLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.JungleLeech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.AquaCenti, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.25f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, DLCSharedEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));

                //Creature Relationships to BabyLizard
                StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.AquaCenti, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(DLCSharedEnums.CreatureTemplateType.ScavengerElite, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
            }
            if (ModManager.MSC)
            {
                //BabyLizard Relationships to Creature
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.3f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.5f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.35f));

                //Creature Relationships to BabyLizard
                StaticWorld.EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
            }
        }

        private static CreatureTemplate LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
        {
            if (type == Register.BabyLizard)
            {
                List<TileTypeResistance> tileTypeResistance = new List<TileTypeResistance>();
                List<TileConnectionResistance> tileConnectionResistance = new List<TileConnectionResistance>();

                tileTypeResistance.Add(new TileTypeResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Allowed));
                tileTypeResistance.Add(new TileTypeResistance(AItile.Accessibility.Corridor, 1.2f, PathCost.Legality.Allowed));
                tileTypeResistance.Add(new TileTypeResistance(AItile.Accessibility.Climb, 2f, PathCost.Legality.Allowed));

                tileConnectionResistance.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToClimb, 40f, PathCost.Legality.Allowed));
                tileConnectionResistance.Add(new TileConnectionResistance(MovementConnection.MovementType.DropToFloor, 40f, PathCost.Legality.Allowed));

                LizardBreedParams lizardBreedParams = new LizardBreedParams(Register.BabyLizard)
                {
                    biteDelay = 15,
                    biteInFront = 30f,
                    biteHomingSpeed = 1f,
                    biteChance = 0.4f,
                    attemptBiteRadius = 70f,
                    getFreeBiteChance = 0.5f,
                    biteDamage = 0.6f,
                    biteDamageChance = 0.35f,
                    toughness = 0.5f,
                    stunToughness = 0.75f,
                    regainFootingCounter = 4,
                    baseSpeed = 2f,
                    bodyMass = 0.9f,
                    bodySizeFac = 0.55f,
                    floorLeverage = 1f,
                    maxMusclePower = 1.5f,
                    danger = 0.15f,
                    aggressionCurveExponent = 0.2f,
                    wiggleSpeed = 0.5f,
                    wiggleDelay = 15,
                    bodyStiffnes = 0.2f,
                    swimSpeed = 0.2f,
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
                    riskOfDoubleLoungeDelay = 0.1f,
                    postLoungeStun = 20,
                    loungeTendensy = 0.01f,
                    perfectVisionAngle = 1f,
                    periferalVisionAngle = 0.05f,
                    biteDominance = 0.1f,
                    limbSize = 0.65f,
                    limbThickness = 1f,
                    stepLength = 0.2f,
                    liftFeet = 0.1f,
                    feetDown = 0.2f,
                    noGripSpeed = 0.2f,
                    limbSpeed = 6f,
                    limbQuickness = 0.6f,
                    limbGripDelay = 1,
                    smoothenLegMovement = true,
                    legPairDisplacement = 0f,
                    standardColor = new Color(1f, 0.5f, 1f),
                    walkBob = 0.4f,
                    tailSegments = 5,
                    tailStiffness = 250f,
                    tailStiffnessDecline = 0.1f,
                    tailLengthFactor = 1.3f,
                    tailColorationStart = 0.1f,
                    tailColorationExponent = 1.2f,
                    headSize = 0.8f,
                    neckStiffness = 0f,
                    jawOpenAngle = 80f,
                    jawOpenLowerJawFac = 0.55f,
                    jawOpenMoveJawsApart = 20f,
                    headGraphics = new int[5],
                    framesBetweenLookFocusChange = 20,
                    tongue = true,
                    tongueAttackRange = 100f,
                    tongueWarmUp = 20,
                    tongueSegments = 4,
                    tongueChance = 0.15f,
                    tamingDifficulty = 0.5f,
                    terrainSpeeds = new LizardBreedParams.SpeedMultiplier[Enum.GetNames(typeof(AItile.Accessibility)).Length],
                    bodyRadFac = 1f,
                    pullDownFac = 1f,
                    bodyLengthFac = 1f
                };
                for (int i = 0; i < lizardBreedParams.terrainSpeeds.Length; i++)
                    lizardBreedParams.terrainSpeeds[i] = new LizardBreedParams.SpeedMultiplier(0.1f, 1f, 1f, 1f);
                lizardBreedParams.terrainSpeeds[1] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
                lizardBreedParams.terrainSpeeds[2] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
                lizardBreedParams.terrainSpeeds[3] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);
                lizardBreedParams.terrainSpeeds[4] = new LizardBreedParams.SpeedMultiplier(1f, 1f, 1f, 1f);

                CreatureTemplate template = new CreatureTemplate(Register.BabyLizard, lizardAncestor, tileTypeResistance, tileConnectionResistance, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
                {
                    breedParameters = lizardBreedParams,
                    baseDamageResistance = lizardBreedParams.toughness * 2f,
                    baseStunResistance = lizardBreedParams.toughness,
                    meatPoints = 4,
                    visualRadius = 900f,
                    waterVision = 0.3f,
                    throughSurfaceVision = 0.85f,
                    movementBasedVision = 0.3f,
                    waterPathingResistance = 5f,
                    dangerousToPlayer = lizardBreedParams.danger / 2f,
                    virtualCreature = false,
                    preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard),
                    throwAction = "Hiss",
                    pickupAction = "Bite",
                    jumpAction = "Tongue"
                };
                template.damageRestistances[(int)Creature.DamageType.Bite, 0] = 2.5f;
                template.damageRestistances[(int)Creature.DamageType.Bite, 1] = 3f;
                return template;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        }
    }
}