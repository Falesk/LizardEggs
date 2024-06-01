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

        public static void InitConfig()
        {
            OptionInterface options = new Options();
            MachineConnector.SetRegisteredOI(Plugin.GUID, options);
            options.config.configurables.Clear();
            eggGrowthTime = options.config.Bind("eggGrowthTime", 3, new ConfigurableInfo("Changes the amount of cycles required to hatch an egg", new ConfigAcceptableRange<int>(1, 99), tags: "Cycles to hatch"));
            lizGrowthTime = options.config.Bind("lizGrowthTime", 1, new ConfigurableInfo("Changes the amount of cycles required for the cub to grow up", new ConfigAcceptableRange<int>(1, 99), tags: "Cycles to grow up"));
            trLizOpport = options.config.Bind("trLizOpport", false, new ConfigurableInfo("If enabled, then with some chance a Train lizard can hatch from a Red lizard egg", tags: "Opportunity for a Train lizard to appear"));
            colorInheritance = options.config.Bind("colorInheritance", false, new ConfigurableInfo("Lizards inherit the color of their parent", tags: "Color inheritance"));
            youngLiz = options.config.Bind("youngLiz", true, new ConfigurableInfo("If enabled, young lizards spawn from the egg, otherwise adults", tags: "Young lizards spawn"));
            baseChance = options.config.Bind("baseChance", 0.333f, new ConfigurableInfo("Base spawn chance, affects only the Survivor and custom slugcats", new ConfigAcceptableRange<float>(0f, 1f), tags: "Base spawn chance"));
            occurrenceFrequency = options.config.Bind("occurrenceFrequency", 1f, new ConfigurableInfo("Changes the egg appearance multiplier for all slugcats", new ConfigAcceptableRange<float>(0f, 5f), tags: "Occurrence frequency multiplier"));
            glowBrightness = options.config.Bind("glowBrightness", 1f, new ConfigurableInfo("Changes the brightness of the egg glow", new ConfigAcceptableRange<float>(0f, 1f), tags: "Glow brightness"));
            indBrightness = options.config.Bind("indBrightness", 1f, new ConfigurableInfo("Changes the brightness of the indicator glow", new ConfigAcceptableRange<float>(0f, 1f), tags: "Indicator brightness"));
        }

        public static AbstractPhysicalObject.AbstractObjectType LizardEgg;
        public static SlugNPCAI.Food LizardEggNPCFood;

        public static Configurable<int> eggGrowthTime;
        public static Configurable<int> lizGrowthTime;
        public static Configurable<bool> trLizOpport;
        public static Configurable<bool> colorInheritance;
        public static Configurable<bool> youngLiz;
        public static Configurable<float> baseChance;
        public static Configurable<float> occurrenceFrequency;
        public static Configurable<float> glowBrightness;
        public static Configurable<float> indBrightness;
    }
}
