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
            On.StaticWorld.InitCustomTemplates += StaticWorld_InitCustomTemplates;
            On.StaticWorld.InitStaticWorld += StaticWorld_InitStaticWorld;
            On.WorldLoader.CreatureTypeFromString += WorldLoader_CreatureTypeFromString;
            On.CreatureTemplate.ctor_Type_CreatureTemplate_List1_List1_Relationship += CreatureTemplate_ctor_Type_CreatureTemplate_List1_List1_Relationship;
            On.CreatureSymbol.ColorOfCreature += CreatureSymbol_ColorOfCreature;
            On.CreatureSymbol.SpriteNameOfCreature += CreatureSymbol_SpriteNameOfCreature;
        }

        private static string CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (iconData.critType == Register.BabyLizard)
                return "Kill_Standard_Lizard";
            return orig(iconData);
        }

        private static Color CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
        {
            if (iconData.critType == Register.BabyLizard)
                return new Color(1f, 0.5f, 1f);
            return orig(iconData);
        }

        private static void CreatureTemplate_ctor_Type_CreatureTemplate_List1_List1_Relationship(On.CreatureTemplate.orig_ctor_Type_CreatureTemplate_List1_List1_Relationship orig, CreatureTemplate self, CreatureTemplate.Type type, CreatureTemplate ancestor, List<TileTypeResistance> tileResistances, List<TileConnectionResistance> connectionResistances, CreatureTemplate.Relationship defaultRelationship)
        {
            orig(self, type, ancestor, tileResistances, connectionResistances, defaultRelationship);
            if (type == Register.BabyLizard)
                self.name = "BabyLizard";
        }

        private static CreatureTemplate.Type WorldLoader_CreatureTypeFromString(On.WorldLoader.orig_CreatureTypeFromString orig, string s)
        {
            if (s.ToLower() == "babylizard")
                return Register.BabyLizard;
            return orig(s);
        }

        private static void StaticWorld_InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig)
        {
            orig();
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

            CreatureTemplate template = new CreatureTemplate(Register.BabyLizard, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), tileTypeResistance, tileConnectionResistance, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f))
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
            StaticWorld.creatureTemplates[template.type.Index] = template;
        }

        private static void StaticWorld_InitStaticWorld(On.StaticWorld.orig_InitStaticWorld orig)
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

            if (ModManager.MSC)
            {
                //BabyLizard Relationships to Creature
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.6f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.Inspector, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.3f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.25f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.BigJelly, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.FireBug, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.3f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.PlaysWith, 0.5f));
                StaticWorld.EstablishRelationship(Register.BabyLizard, MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.35f));
                
                //Creature Relationships to BabyLizard
                StaticWorld.EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
                StaticWorld.EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
                StaticWorld.EstablishRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, Register.BabyLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.1f));
            }
        }
    }
}
