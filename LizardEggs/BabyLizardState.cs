namespace LizardEggs
{
    public class BabyLizardState : LizardState
    {
        public CreatureTemplate.Type parent;
        public int age;
        public uint hexColor;

        public BabyLizardState(AbstractCreature creature) : base(creature)
        {
            parent = StaticWorld.GetCreatureTemplate(FDataMananger.RandomLizard()).type;
            age = 0;
            hexColor = 0;
            LimbFix();
        }

        public void LimbFix()
        {
            LizardState prt = new LizardState(new AbstractCreature(creature.world, StaticWorld.GetCreatureTemplate(parent), null, creature.pos, creature.ID));
            limbHealth = new float[prt.limbHealth.Length];
            for (int i = 0; i < limbHealth.Length; i++)
                limbHealth[i] = 1f;
        }

        public override string ToString()
        {
            return base.ToString() + $"<cB>Parent<cC>{StaticWorld.GetCreatureTemplate(parent).name}<cB>Age<cC>{age}<cB>HexColor<cC>{hexColor}";
        }

        public override void LoadFromString(string[] s)
        {
            base.LoadFromString(s);
            for (int i = 0; i < s.Length; i++)
            {
                string[] arr = s[i].Split(new[] { "<cC>" }, System.StringSplitOptions.None);
                string text = arr[0];
                if (!string.IsNullOrEmpty(text))
                {
                    switch (text)
                    {
                        case "Parent":
                            parent = StaticWorld.GetCreatureTemplate(arr[1]).type;
                            break;
                        case "Age":
                            age = int.Parse(arr[1]);
                            break;
                        case "HexColor":
                            hexColor = uint.Parse(arr[1]);
                            break;
                    }
                }
            }
            unrecognizedSaveStrings.Remove("Parent");
            unrecognizedSaveStrings.Remove("Age");
            unrecognizedSaveStrings.Remove("HexColor");
        }

        public override void CycleTick()
        {
            age++;
            base.CycleTick();
        }
    }
}
