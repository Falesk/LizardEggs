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

        /// <summary>
        /// A method that returns a random deviation of the value of the function sin(x*pi)
        /// </summary>
        /// <param name="k">Value between 0 and 1</param>
        /// <param name="d">Max deviation, between 0 and 1</param>
        public static float RandomSinusoidDeviation(float k, float d)
        {
            float baseValue = Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI);
            return baseValue * Mathf.Lerp(1f - Mathf.Clamp01(d) / 2f, 1f + Mathf.Clamp01(d) / 2f, Random.value);
        }

        public static Color RandomGray(float d)
        {
            float rand = Mathf.Lerp(-0.5f * d, 0.5f * d, Random.value);
            return new Color(rand, rand, rand);
        }

        public static Color Clamp01Color(Color color)
        {
            return new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), Mathf.Clamp01(color.a));
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