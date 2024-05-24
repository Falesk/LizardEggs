using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LizardEggs
{
    public static class FCustom
    {
        // CWT
        public class Data
        {
            public Data(AbstractCreature self)
            {
            }
            public bool isChild = false;
            public AbstractLizardEgg egg = null;
        }
        static ConditionalWeakTable<AbstractCreature, Data> lizardData = new ConditionalWeakTable<AbstractCreature, Data>();
        public static Data GetData(this AbstractCreature self) => lizardData.GetValue(self, x => new Data(x));

        // Custom methods
        public static int ColorToInt(Color color)
        {
            int val = 0;
            val += (int)(color.r * 100) * 1000000;
            val += (int)(color.g * 100) * 1000;
            val += (int)(color.b * 100);
            return val;
        }
        public static Color IntToColor(int val)
        {
            int r = val / 1000000;
            int g = val / 1000 - (r * 1000);
            int b = val % 1000;
            return new Color((float)r / 100, (float)g / 100, (float)b / 100);
        }
        public static CreatureTemplate CreatureTemplateFromType(string str)
        {
            string s = str[0].ToString();
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]))
                    s += " ";
                s += str[i].ToString();
            }
            return StaticWorld.GetCreatureTemplate(s);
        }
        public static CreatureTemplate CreatureTemplateFromType(CreatureTemplate.Type type) => CreatureTemplateFromType(type.value);
        public static void ChangeDictTouple<T, P>(Dictionary<T, (P, int)> dict, T key, int value)
        {
            var a = dict[key];
            a.Item2 += value;
            dict[key] = a;
        }
    }
}
