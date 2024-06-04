using Menu.Remix.MixedUI;
using Menu.Remix;
using UnityEngine;

namespace LizardEggs
{
    class Options : OptionInterface
    {
        public override void Initialize()
        {
            base.Initialize();
            if (intConfig == null)
                ArrayConfig();
            Tabs = new OpTab[] { new OpTab(this) };
            PlaceConfigBlock(new Vector2(70f, 550f), intConfig);
            PlaceConfigBlock(new Vector2(70f, 350f), floatConfig);
            PlaceConfigBlock(new Vector2(350f, 550f), boolConfig);
        }

        public override string ValidationString()
        {
            if (intConfig == null)
                ArrayConfig();
            string text = $"[{Plugin.GUID} v{Plugin.Version}] ";
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
            Tabs[0]._AddItem(new OpLabel(pos, Vector2.zero, Translate(header), bigText: true));
        }

        public void ArrayConfig()
        {
            intConfig = new Configurable<int>[]
            {
                Register.eggGrowthTime,
                Register.lizGrowthTime
            };
            boolConfig = new Configurable<bool>[]
            {
                Register.trLizOpport,
                Register.colorInheritance,
                Register.tamedAggressiveness,
                Register.youngLiz
            };
            floatConfig = new Configurable<float>[]
            {
                Register.baseChance,
                Register.occurrenceFrequency,
                Register.glowBrightness,
                Register.indBrightness,
                Register.lizAggressiveness
            };
        }

        private Configurable<int>[] intConfig;
        private Configurable<bool>[] boolConfig;
        private Configurable<float>[] floatConfig;
    }
}
