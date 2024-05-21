using System.Runtime.CompilerServices;
using UnityEngine;

namespace LizardEggs
{
    public static class Features
    {
        public class Data
        {
            public Data(AbstractCreature self)
            {
            }
            public int stage = -1;
        }
        public static ConditionalWeakTable<AbstractCreature, Data> lizardCWT = new ConditionalWeakTable<AbstractCreature, Data>();
        public static Data GetData(this AbstractCreature self) => lizardCWT.GetValue(self, x => new Data(x));
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
    }
}
