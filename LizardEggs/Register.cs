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
            AbstractPhysicalObject.AbstractObjectType lizardEgg = LizardEgg;
            lizardEgg?.Unregister();
            LizardEgg = null;

            SlugNPCAI.Food lizardEggFood = LizardEggNPCFood;
            lizardEggFood?.Unregister();
            LizardEggNPCFood = null;

            SLOracleBehaviorHasMark.MiscItemType eggConv = EggConv;
            eggConv?.Unregister();
            EggConv = null;

            MultiplayerUnlocks.SandboxUnlockID lizardEggUnlock = LizardEggUnlock;
            lizardEggUnlock?.Unregister();
            LizardEggUnlock = null;

            CreatureTemplate.Type babyLizard = BabyLizard;
            babyLizard?.Unregister();
            BabyLizard = null;
        }
    }
}