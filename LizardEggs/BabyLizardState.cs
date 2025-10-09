using System.Text.RegularExpressions;

namespace LizardEggs
{
    public class BabyLizardState : LizardState
    {
        public CreatureTemplate.Type parent;
        public int age;
        public uint hexColor;

        public BabyLizardState(AbstractCreature creature) : base(creature)
        {
            parent = CreatureTemplate.Type.GreenLizard;
            age = 0;
            hexColor = 0;
        }

        public override string ToString()
        {
            string text = HealthBaseSaveString();
            if (throatHealth < 1f)
                text += $"<cB>ThroatHealth<cC>{throatHealth}";
            for (int i = 0; i < limbHealth.Length; i++)
            {
                if (limbHealth[i] < 1f)
                {
                    text += $"<cB>LimbHealth<cC>{string.Join("<cC>", limbHealth)}";
                    break;
                }
            }
            if (rotType != RotType.None)
                text += $"<cB>RotType<cC>{rotType.Index}";
            text += $"<cB>Age<cC>{age}<cB>HexColor<cC>{hexColor}";
            foreach (var keyValuePair in unrecognizedSaveStrings)
                text = $"{text}<cB>{keyValuePair.Key}<cC>{keyValuePair.Value}";
            return text;
        }

        public override void LoadFromString(string[] s)
        {
            base.LoadFromString(s);
            for (int i = 0; i < s.Length; i++)
            {
                string[] arr = Regex.Split(s[i], "<cC>");
                string text = arr[0];
                if (!string.IsNullOrEmpty(text))
                {
                    switch (text)
                    {
                        case "Age":
                            age = int.Parse(arr[1]);
                            break;
                        case "HexColor":
                            hexColor = uint.Parse(arr[1]);
                            break;
                    }
                }
            }
            unrecognizedSaveStrings.Remove("Age");
            unrecognizedSaveStrings.Remove("HexColor");
        }

        public override void CycleTick()
        {
            age += 1;
            base.CycleTick();
        }
    }
}
