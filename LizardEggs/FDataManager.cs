using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LizardEggs
{
    public static class FDataManager
    {
        public class LizardData
        {
            public AbstractLizardEgg egg;
            public bool sawPlayerWithEgg;
            public bool playerIsParent;
        }
        private static readonly ConditionalWeakTable<AbstractCreature, LizardData> lizardData = new ConditionalWeakTable<AbstractCreature, LizardData>();
        public static LizardData GetData(this AbstractCreature self) => lizardData.GetValue(self, x => new LizardData());

        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> Dens { get; private set; }
        public static List<CreatureTemplate> lizTypes;

        public static void InitLizTypes()
        {
            lizTypes = new List<CreatureTemplate>();
            foreach (CreatureTemplate template in StaticWorld.creatureTemplates)
                if (template.IsLizard && template.name != "YoungLizard" && template.name != "BabyLizard")
                    lizTypes.Add(template);
        }

        public static string RandomLizard()
        {
            if (lizTypes == null || lizTypes.Count == 0)
                InitLizTypes();
            return lizTypes[UnityEngine.Random.Range(0, lizTypes.Count)].name;
        }

        public static void InitDens()
        {
            Dens = new Dictionary<WorldCoordinate, (AbstractCreature, int)>();
        }

        public static void ChangeDensValue(WorldCoordinate key, int value)
        {
            var a = Dens[key];
            a.Item2 += value;
            Dens[key] = a;
        }

        public static void AddToDens(AbstractCreature abstr, SlugcatStats.Name name)
        {
            if (Dens.ContainsKey(abstr.spawnDen) || !LizardCanHaveAnEgg(abstr))
                return;
            Dens.Add(abstr.spawnDen, (abstr, 0));
            float chance = FCustom.EggSpawnChance(name);
            if (Options.occurrenceFrequency.Value > 1f)
                chance += (1f - chance) * (1f - 1f / Options.occurrenceFrequency.Value);
            else chance *= Options.occurrenceFrequency.Value;
            if (UnityEngine.Random.value < chance)
                ChangeDensValue(abstr.spawnDen, 1);
        }

        private static bool LizardCanHaveAnEgg(AbstractCreature abstr) => abstr.spawnDen.NodeDefined && abstr.creatureTemplate.name != "YoungLizard" && abstr.creatureTemplate.name != "BabyLizard";
    }
}
