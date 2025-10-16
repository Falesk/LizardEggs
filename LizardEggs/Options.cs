using Menu.Remix.MixedUI;
using Menu.Remix;
using UnityEngine;

namespace LizardEggs
{
    public class Options : OptionInterface
    {
        public static Configurable<int> eggGrowthTime;
        public static Configurable<int> lizGrowthTime;
        public static Configurable<bool> trLizOpport;
        public static Configurable<bool> stillborn;
        public static Configurable<bool> tamedAggressiveness;
        public static Configurable<bool> edibleForPups;
        public static Configurable<float> baseChance;
        public static Configurable<float> occurrenceFrequency;
        public static Configurable<float> glowBrightness;
        public static Configurable<float> indBrightness;
        public static Configurable<float> lizAggressiveness;
        public static Configurable<KeyCode> takeButton;
        private static Configurable<int>[] intConfig;
        private static Configurable<bool>[] boolConfig;
        private static Configurable<float>[] floatConfig;

        public Options()
        {
            intConfig = new Configurable<int>[]
            {
                eggGrowthTime = config.Bind("eggGrowthTime", 3, new ConfigurableInfo("Changes the amount of cycles required to hatch an egg", new ConfigAcceptableRange<int>(1, 99), tags: "Cycles to hatch")),
                lizGrowthTime = config.Bind("lizGrowthTime", 3, new ConfigurableInfo("Changes the amount of cycles required for the cub to grow up", new ConfigAcceptableRange<int>(1, 99), tags: "Cycles to grow up"))
            };
            boolConfig = new Configurable<bool>[]
            {
                trLizOpport = config.Bind("trLizOpport", false, new ConfigurableInfo("If enabled, then with some chance a Train lizard can hatch from a Red lizard egg", tags: "Opportunity for a Train lizard to appear")),
                tamedAggressiveness = config.Bind("tamedAggressiveness", false, new ConfigurableInfo("If enabled, tamed lizards will attack you if you steal their egg", tags: "Aggressiveness of the tamed")),
                stillborn = config.Bind("stillborn", true, new ConfigurableInfo("If enabled, lizards won't be able to be born if the player is not around (in one region)", tags: "Possibility of stillbirth")),
                edibleForPups = config.Bind("edibleForPups", true, new ConfigurableInfo("If enabled, slugpups can eat lizard eggs", tags: "Lizard eggs are edible for slugpups"))
            };
            floatConfig = new Configurable<float>[]
            {
                baseChance = config.Bind("baseChance", 0.33f, new ConfigurableInfo("Base spawn chance, affects only the Survivor and custom slugcats", new ConfigAcceptableRange<float>(0f, 1f), tags: "Base spawn chance")),
                occurrenceFrequency = config.Bind("occurrenceFrequency", 1f, new ConfigurableInfo("Changes the egg appearance multiplier for all slugcats", new ConfigAcceptableRange<float>(0f, 10f), tags: "Occurrence frequency multiplier")),
                glowBrightness = config.Bind("glowBrightness", 1f, new ConfigurableInfo("Changes the brightness of the egg glow", new ConfigAcceptableRange<float>(0f, 1f), tags: "Glow brightness")),
                indBrightness = config.Bind("indBrightness", 1f, new ConfigurableInfo("Changes the brightness of the indicator glow", new ConfigAcceptableRange<float>(0f, 1f), tags: "Indicator brightness")),
                lizAggressiveness = config.Bind("lizAggressiveness", 1f, new ConfigurableInfo("It affects how much the reputation of the lizards decreases if lizard sees its egg in the hands of the player", new ConfigAcceptableRange<float>(0f, 1f), tags: "Lizard aggressiveness"))
            };
            takeButton = config.Bind("takeButton", KeyCode.None, new ConfigurableInfo("You can reconfigure the button that you use to steal the egg. The default button is a \"Grab\" button", tags: "Take button"));
        }
        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[] { new OpTab(this) };
            PlaceConfigBlock(new Vector2(70f, 550f), intConfig);
            PlaceConfigBlock(new Vector2(70f, 350f), floatConfig);
            PlaceConfigBlock(new Vector2(350f, 550f), boolConfig);

            Tabs[0].AddItems(
                new OpLabel(new Vector2(380f, 350f), Vector2.zero, Translate("Take button"), bigText: true),
                new OpKeyBinder(takeButton, new Vector2(330f, 290f), new Vector2(70f, 40f)) { description = Translate(takeButton.info.description) });
        }

        public override string ValidationString()
        {
            string text = $"[{Plugin.ID} v{Plugin.Version}] ";
            foreach (Configurable<int> intConf in intConfig)
                text += $"{intConf.Value} ";
            foreach (Configurable<bool> boolConf in boolConfig)
                text += boolConf.Value ? "T" : "F";
            text += " ";
            foreach (Configurable<float> floatConf in floatConfig)
                text += $"{floatConf.Value} ";
            return text.TrimEnd();
        }

        public void PlaceConfigBlock(Vector2 pos, ConfigurableBase[] config)
        {
            string header = string.Empty;
            for (int i = 0; i < config.Length; i++)
            {
                Vector2 v = new Vector2(pos.x, pos.y - (40f * (i + 1)));
                float x = 0f;
                UIconfig UIconf = null;
                switch (ValueConverter.GetTypeCategory(config[i].settingType))
                {
                    case ValueConverter.TypeCategory.Integrals:
                        if (i == 0)
                            header = "Growth";
                        UIconf = new OpUpdown(config[i] as Configurable<int>, new Vector2(v.x - 25f, v.y - 5f), 70f)
                        { description = Translate((config[i] as Configurable<int>).info.description) };
                        x = 60f;
                        break;
                    case ValueConverter.TypeCategory.Boolean:
                        if (i == 0)
                            header = "Check boxes";
                        UIconf = new OpCheckBox(config[i] as Configurable<bool>, new Vector2(v.x - 45f, v.y))
                        { description = Translate((config[i] as Configurable<bool>).info.description) };
                        x = -5f;
                        break;
                    case ValueConverter.TypeCategory.Floats:
                        if (i == 0)
                            header = "Custom settings";
                        UIconf = new OpUpdown(config[i] as Configurable<float>, new Vector2(v.x - 25f, v.y - 5f), 70f, 2)
                        { description = Translate((config[i] as Configurable<float>).info.description) };
                        x = 60f;
                        break;
                }
                Tabs[0].AddItems(new UIelement[]
                {
                    UIconf,
                    new OpLabel(new Vector2(v.x + x, v.y), Vector2.zero, Translate(config[i].info.Tags[0] as string), FLabelAlignment.Left)
                });
            }
            Tabs[0].AddItems(new OpLabel(pos, Vector2.zero, Translate(header), bigText: true));
        }
    }
}