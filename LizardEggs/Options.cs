using Menu.Remix.MixedUI;
using Menu.Remix;
using System;
using UnityEngine;

namespace LizardEggs
{
    class Options : OptionInterface
    {
        public override void Initialize()
        {
            base.Initialize();
            if (integerConfig == null)
                ArrayConfig();
            Tabs = new OpTab[] { new OpTab(this) };
            PlaceInterface(new Vector2(70f, 550f), integerConfig);
            PlaceInterface(new Vector2(70f, 360f), boolConfig);
            PlaceInterface(new Vector2(350f, 550f), floatConfig);
        }

        public override string ValidationString()
        {
            if (integerConfig == null)
                ArrayConfig();
            string text = $"[{Plugin.GUID} v{Plugin.Version}] ";
            foreach (Configurable<int> intConf in integerConfig)
                text += $"{intConf.Value} ";
            foreach (Configurable<bool> boolConf in boolConfig)
                text += boolConf.Value ? "T " : "F ";
            foreach (Configurable<float> floatConf in floatConfig)
                text += $"{Math.Round((double)floatConf.Value, 2)} ";
            return text;
        }

        public void PlaceInterface(Vector2 pos, ConfigurableBase[] config)
        {
            if (integerConfig == null)
                ArrayConfig();
            string header = "";
            for (int i = 0; i < config.Length; i++)
            {
                Vector2 v = new Vector2(pos.x, pos.y - 40f - 40f * i);
                float x = 0f;
                UIconfig UIconf = null;
                switch (ValueConverter.GetTypeCategory(config[i].settingType))
                {
                    case ValueConverter.TypeCategory.Integrals:
                        if (i == 0)
                            header = "Growth";
                        UIconf = new OpUpdown(config[i] as Configurable<int>, new Vector2(v.x - 25f, v.y - 5f), 60f)
                        { description = (config[i] as Configurable<int>).info.description };
                        x = 50f;
                        break;
                    case ValueConverter.TypeCategory.Boolean:
                        if (i == 0)
                            header = "Check boxes";
                        UIconf = new OpCheckBox(config[i] as Configurable<bool>, new Vector2(v.x - 45f, v.y))
                        { description = (config[i] as Configurable<bool>).info.description };
                        break;
                    case ValueConverter.TypeCategory.Floats:
                        if (i == 0)
                            header = "Custom";
                        UIconf = new OpUpdown(config[i] as Configurable<float>, new Vector2(v.x - 25f, v.y - 5f), 60f, 2)
                        { description = (config[i] as Configurable<float>).info.description };
                        x = 50f;
                        break;
                }
                Tabs[0].AddItems(new UIelement[]
                {
                    UIconf,
                    new OpLabel(new Vector2(v.x + x, v.y), Vector2.zero, config[i].info.Tags[0] as string, FLabelAlignment.Left)
                });
            }
            Tabs[0]._AddItem(new OpLabel(pos, Vector2.zero, header, bigText: true));
        }

        public void ArrayConfig()
        {
            integerConfig = new Configurable<int>[]
            {
                Register.eggGrowthTime,
                Register.lizGrowthTime
            };
            boolConfig = new Configurable<bool>[]
            {
                Register.trLizOpport,
                Register.colorInheritance,
                Register.youngLiz
            };
            floatConfig = new Configurable<float>[]
            {
                Register.baseChance,
                Register.occurrenceFrequency,
                Register.glowBrightness,
                Register.indBrightness
            };
        }

        private Configurable<int>[] integerConfig;
        private Configurable<bool>[] boolConfig;
        private Configurable<float>[] floatConfig;
    }
}
