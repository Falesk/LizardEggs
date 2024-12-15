using MoreSlugcats;
using UnityEngine;

namespace LizardEggs
{
    public static class HooksObject
    {
        public static void Init()
        {
            //Main object
            On.AbstractPhysicalObject.Realize += AbstractPhysicalObject_Realize;
            On.Player.Grabability += Player_Grabability;
            On.SlugcatStats.NourishmentOfObjectEaten += SlugcatStats_NourishmentOfObjectEaten;
            On.PlayerSessionRecord.AddEat += PlayerSessionRecord_AddEat;
            On.ScavengerAI.CollectScore_PhysicalObject_bool += ScavengerAI_CollectScore_PhysicalObject_bool;
            //Slugpups
            On.MoreSlugcats.SlugNPCAI.GetFoodType += (On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food) =>
                (food is LizardEgg) ? Register.LizardEggNPCFood : orig(self, food);
            On.MoreSlugcats.SlugNPCAI.AteFood += SlugNPCAI_AteFood;
            //Moon conversation
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
            On.SLOracleBehaviorHasMark.TypeOfMiscItem += (On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject testItem) =>
                (testItem is LizardEgg) ? Register.EggConv : orig(self, testItem);
            //Icon
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.SpriteNameForItem += (On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData) =>
                itemType == Register.LizardEgg ? "HipsA" : orig(itemType, intData);
        }

        private static int ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
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
        }

        private static IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            if (item.type == Register.LizardEgg)
            {
                int data = (item is AbstractLizardEgg egg) ? ((FCustom.ColorToInt(egg.color) == 0) ? 1 : FCustom.ColorToInt(egg.color)) : 0;
                return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, data);
            }
            return orig(item);
        }

        private static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (itemType == Register.LizardEgg)
                return Color.Lerp((intData == 0) ? Color.green : FCustom.IntToColor(intData), Color.black, 0.4f);
            return orig(itemType, intData);
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            orig(self);
            if (self.id == Conversation.ID.Moon_Misc_Item && self.describeItem == Register.EggConv)
                self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It looks like the egg of some kind of creature. How interesting, it has a very hard shell,<LINE>and there are bioluminescent inclusions all over its surface! The indigenous fauna has adapted well."), 0));
        }

        private static void PlayerSessionRecord_AddEat(On.PlayerSessionRecord.orig_AddEat orig, PlayerSessionRecord self, PhysicalObject eatenObject)
        {
            orig(self, eatenObject);
            if (eatenObject is LizardEgg)
            {
                self.vegetarian = false;
                self.carnivorous = true;
            }
        }

        private static void SlugNPCAI_AteFood(On.MoreSlugcats.SlugNPCAI.orig_AteFood orig, SlugNPCAI self, PhysicalObject food)
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
        }

        private static int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
        {
            if (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint && eatenobject is LizardEgg)
                return -1;
            if ((slugcatIndex == SlugcatStats.Name.Red || (ModManager.MSC && slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Artificer)) && eatenobject is LizardEgg)
                return 6 * eatenobject.FoodPoints;
            return orig(slugcatIndex, eatenobject);
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (obj is LizardEgg egg)
            {
                if (egg.firstChunk.mass < 0.25f)
                    return Player.ObjectGrabability.BigOneHand;
                return Player.ObjectGrabability.TwoHands;
            }
            return orig(self, obj);
        }

        private static void AbstractPhysicalObject_Realize(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {
            orig(self);
            if (self.type == Register.LizardEgg)
            {
                if (self is AbstractLizardEgg)
                    self.realizedObject = new LizardEgg(self);
                else self.realizedObject = new LizardEgg(new AbstractLizardEgg(self));
            }
        }
    }
}
