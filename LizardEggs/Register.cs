using MoreSlugcats;

namespace LizardEggs
{
    public static class Register
    {
        public static void RegisterValues()
        {
            LizardEgg = new AbstractPhysicalObject.AbstractObjectType("LizardEgg", true);
            LizardEggNPCFood = new SlugNPCAI.Food("LizardEggNPCFood", true);
        }

        public static void UnregisterValues()
        {
            AbstractPhysicalObject.AbstractObjectType lizardEgg = LizardEgg;
            lizardEgg?.Unregister();
            LizardEgg = null;

            SlugNPCAI.Food lizardEggFood = LizardEggNPCFood;
            lizardEggFood?.Unregister();
            LizardEggNPCFood = null;
        }

        public static AbstractPhysicalObject.AbstractObjectType LizardEgg;
        public static SlugNPCAI.Food LizardEggNPCFood;
    }
}