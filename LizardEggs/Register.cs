using MoreSlugcats;

namespace LizardEggs
{
    public static class Register
    {
        public static AbstractPhysicalObject.AbstractObjectType LizardEgg;
        public static SlugNPCAI.Food LizardEggNPCFood;
        public static SLOracleBehaviorHasMark.MiscItemType EggConv;
        public static MultiplayerUnlocks.SandboxUnlockID LizardEggUnlock;
        public static CreatureTemplate.Type BabyLizard;

        public static void RegisterValues()
        {
            LizardEgg = new AbstractPhysicalObject.AbstractObjectType("LizardEgg", true);
            LizardEggNPCFood = new SlugNPCAI.Food("LizardEggNPCFood", true);
            EggConv = new SLOracleBehaviorHasMark.MiscItemType("EggConv", true);
            LizardEggUnlock = new MultiplayerUnlocks.SandboxUnlockID("LizardEggUnlock", true);
            BabyLizard = new CreatureTemplate.Type("BabyLizard", true);
        }

        public static void UnregisterValues()
        {
            LizardEgg?.Unregister();
            LizardEgg = null;

            LizardEggNPCFood?.Unregister();
            LizardEggNPCFood = null;

            EggConv?.Unregister();
            EggConv = null;

            LizardEggUnlock?.Unregister();
            LizardEggUnlock = null;

            BabyLizard?.Unregister();
            BabyLizard = null;
        }
    }
}