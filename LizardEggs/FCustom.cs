using UnityEngine;

namespace LizardEggs
{
    public static class FCustom
    {
        public static uint ARGB2HEX(Color color)
        {
            uint hex = (uint)Mathf.Lerp(0, 255f, color.r);
            hex = (hex << 8) ^ (uint)Mathf.Lerp(0, 255f, color.g);
            hex = (hex << 8) ^ (uint)Mathf.Lerp(0, 255f, color.b);
            hex = (hex << 8) ^ (uint)Mathf.Lerp(0, 255f, color.a);
            return hex;
        }
        public static Color HEX2ARGB(uint hex)
        {
            float a = (hex & 0xFF) / 255f;
            float b = ((hex >> 8) & 0xFF) / 255f;
            float g = ((hex >> 16) & 0xFF) / 255f;
            float r = ((hex >> 24) & 0xFF) / 255f;
            return new Color(r, g, b, a);
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