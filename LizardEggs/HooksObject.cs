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
            //Slugpups
            On.MoreSlugcats.SlugNPCAI.GetFoodType += (orig, self, food) => (food is LizardEgg && Options.edibleForPups.Value) ? Register.LizardEggNPCFood : orig(self, food);
            On.MoreSlugcats.SlugNPCAI.WantsToEatThis += (orig, self, obj) => (obj is LizardEgg) ? Options.edibleForPups.Value && orig(self, obj) : orig(self, obj);
            On.MoreSlugcats.SlugNPCAI.AteFood += SlugNPCAI_AteFood;
            //Moon conversation
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
            On.SLOracleBehaviorHasMark.TypeOfMiscItem += (orig, self, testItem) => (testItem is LizardEgg) ? Register.EggConv : orig(self, testItem);
            //Icon
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;
            On.ItemSymbol.SpriteNameForItem += (orig, itemType, intData) => itemType == Register.LizardEgg ? "HipsA" : orig(itemType, intData);
        }

        private static IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            if (item.type == Register.LizardEgg)
            {
                Color color = (item is AbstractLizardEgg egg) ? egg.color : Color.green;
                return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, Register.LizardEgg, (int)FCustom.ARGB2HEX(color));
            }
            return orig(item);
        }

        private static Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (itemType == Register.LizardEgg)
            {
                Color color = (intData == 0) ? Color.green : FCustom.HEX2ARGB((uint)intData);
                return Color.Lerp(color, Color.black, 0.4f);
            }
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
            if (food is LizardEgg && Options.edibleForPups.Value)
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
                    return Player.ObjectGrabability.OneHand;
                else if (egg.firstChunk.mass < 0.4f)
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
