using MoreSlugcats;

namespace LizardEggs
{
    public static class Register
    {
        public static void RegisterValues()
        {
            LizardEgg = new AbstractPhysicalObject.AbstractObjectType("LizardEgg", true);
            LizardEggNPCFood = new SlugNPCAI.Food("LizardEggNPCFood", true);
            BabyLizard = new CreatureTemplate.Type("BabyLizard", true);
        }

        public static void UnregisterValues()
        {
            AbstractPhysicalObject.AbstractObjectType lizardEgg = LizardEgg;
            lizardEgg?.Unregister();
            LizardEgg = null;

            SlugNPCAI.Food lizardEggFood = LizardEggNPCFood;
            lizardEggFood?.Unregister();
            LizardEggNPCFood = null;

            CreatureTemplate.Type babyLizard = BabyLizard;
            babyLizard?.Unregister();
            BabyLizard = null;
        }

        public static CreatureTemplate.Type BabyLizard;
        public static AbstractPhysicalObject.AbstractObjectType LizardEgg;
        public static SlugNPCAI.Food LizardEggNPCFood;
    }
}