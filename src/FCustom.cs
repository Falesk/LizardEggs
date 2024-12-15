using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LizardEggs
{
    public static class FCustom
    {
        // CWT
        public class LizardData
        {
            public AbstractLizardEgg egg;
            public bool sawPlayerWithEgg;
        }
        private static readonly ConditionalWeakTable<AbstractCreature, LizardData> lizardData = new ConditionalWeakTable<AbstractCreature, LizardData>();
        public static LizardData GetData(this AbstractCreature self) => lizardData.GetValue(self, x => new LizardData());

        // Custom methods
        public static int ColorToInt(Color color) => (int)(color.r * 100) * 1000000 + (int)(color.g * 100) * 1000 + (int)(color.b * 100);
        public static Color IntToColor(int val)
        {
            int r = val / 1000000;
            int g = val / 1000 - (r * 1000);
            int b = val % 1000;
            return new Color((float)r / 100, (float)g / 100, (float)b / 100);
        }

        public static List<CreatureTemplate> lizTypes;
        public static void InitLizTypes()
        {
            lizTypes = new List<CreatureTemplate>();
            foreach (CreatureTemplate template in StaticWorld.creatureTemplates)
                if (template.IsLizard && template.name != "YoungLizard" && template.name != "BabyLizard")
                    lizTypes.Add(template);
        }

        public static void ChangeDictTuple<T, P>(Dictionary<T, (P, int)> dict, T key, int value)
        {
            var a = dict[key];
            a.Item2 += value;
            dict[key] = a;
        }

        public static int GetAbstractNode(WorldCoordinate wc, Room room)
        {
            for (int i = 0; i < room.abstractRoom.TotalNodes; i++)
                if (wc.CompareDisregardingNode(room.LocalCoordinateOfNode(i)))
                    return i;
            return -1;
        }

        public static float EggSpawnChance(SlugcatStats.Name name)
        {
            switch (name.value)
            {
                case "Red":
                    return 0.285f;
                case "Yellow":
                    return 0.4f;
                case "Spear":
                    return 0.35f;
                case "Artificer":
                    return 0.2f;
                case "Gourmand":
                    return 0.3f;
                case "Rivulet":
                    return 0.35f;
                case "Saint":
                    return 0.1f;
                case "Inv":
                    return 1f;
                default:
                    return Options.baseChance.Value;
            }
        }
    }
}