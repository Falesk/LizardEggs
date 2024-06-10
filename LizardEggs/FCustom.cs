using MoreSlugcats;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LizardEggs
{
    public static class FCustom
    {
        // CWT
        public class CritData
        {
            public CritData(AbstractCreature self)
            {
            }
            public bool isChild;
            public AbstractLizardEgg egg;
            public bool sawPlayerWithEgg;
        }
        private static readonly ConditionalWeakTable<AbstractCreature, CritData> lizardData = new ConditionalWeakTable<AbstractCreature, CritData>();
        public static CritData GetData(this AbstractCreature self) => lizardData.GetValue(self, x => new CritData(x));

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
                if (template.IsLizard && template.name != "YoungLizard")
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
            if (name == SlugcatStats.Name.Red)
                return 0.285f;
            if (name == SlugcatStats.Name.Yellow)
                return 0.4f;
            if (ModManager.MSC)
            {
                if (name == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                    return 0.35f;
                if (name == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
                    return 0.2f;
                if (name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                    return 0.3f;
                if (name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
                    return 0.385f;
                if (name == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                    return 0.1f;
                if (name == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
                    return 1f;
            }
            return Register.baseChance.Value;
        }
    }
}
