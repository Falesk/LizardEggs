using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.IO;
using static LizardEggs.Plugin;

namespace LizardEggs
{
    public static class FDataMananger
    {
        // CWT
        public class LizardData
        {
            public AbstractLizardEgg egg;
            public bool sawPlayerWithEgg;
        }
        private static readonly ConditionalWeakTable<AbstractCreature, LizardData> lizardData = new ConditionalWeakTable<AbstractCreature, LizardData>();
        public static LizardData GetData(this AbstractCreature self) => lizardData.GetValue(self, x => new LizardData());

        public static string FilePath { get; private set; }
        public static Dictionary<WorldCoordinate, (AbstractCreature, int)> Dens { get; private set; }
        public static List<SavedLizard> SavedLizards { get; private set; }
        public static List<CreatureTemplate> lizTypes;
        public static void InitLizTypes()
        {
            lizTypes = new List<CreatureTemplate>();
            foreach (CreatureTemplate template in StaticWorld.creatureTemplates)
                if (template.IsLizard && template.name != "YoungLizard" && template.name != "BabyLizard")
                    lizTypes.Add(template);
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
            if (Dens.ContainsKey(abstr.spawnDen) || !abstr.spawnDen.NodeDefined || abstr.creatureTemplate.name == "YoungLizard" || abstr.creatureTemplate.name == "BabyLizard")
                return;
            Dens.Add(abstr.spawnDen, (abstr, 0));
            int amount = (abstr.world.GetSpawner(abstr.ID) as World.SimpleSpawner).amount;
            float chance = FCustom.EggSpawnChance(name);
            if (Options.occurrenceFrequency.Value > 1f)
                chance += (1f - chance) * (1f - 1f / Options.occurrenceFrequency.Value);
            else chance *= Options.occurrenceFrequency.Value;
            for (int i = 0; i < amount; i++)
                if (UnityEngine.Random.value < chance)
                    ChangeDensValue(abstr.spawnDen, 1);
        }

        //kostyl
        public static void InitLizards()
        {
            SavedLizards = new List<SavedLizard>();
            if (ModManager.ActiveMods.Find(x => x.id == ID) is ModManager.Mod mod)
                FilePath = AssetManager.ResolveFilePath("plugins/babyLizards.txt", true, true);
            if (File.Exists(FilePath))
                foreach (string line in File.ReadLines(FilePath))
                    SavedLizards.Add(SavedLizard.FromString(line));
            else File.Create(FilePath);
        }

        public static void RemoveLizAt(int i)
        {
            SavedLizards.RemoveAt(i);
        }

        //probably kostyl
        public class SavedLizard
        {
            public SavedLizard(EntityID ID, CreatureTemplate.Type parent, int stage)
            {
                this.ID = ID;
                this.parent = parent;
                this.stage = stage;
                slatedForDeletion = false;
            }

            public override string ToString() => $"{ID}~{parent}~{stage}";

            public static SavedLizard FromString(string s)
            {
                string[] array = s.Split('~');
                EntityID ID = EntityID.FromString(array[0]);
                CreatureTemplate.Type parent = new CreatureTemplate.Type(array[1]);
                int stage = int.Parse(array[2]);
                return new SavedLizard(ID, parent, stage);
            }

            public EntityID ID;
            public CreatureTemplate.Type parent;
            public int stage;
            public bool slatedForDeletion;
        }
    }
}
