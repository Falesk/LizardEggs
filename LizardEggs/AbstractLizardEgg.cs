using UnityEngine;

namespace LizardEggs
{
    public class AbstractLizardEgg : AbstractPhysicalObject
    {
        public AbstractLizardEgg(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID, EntityID parentID, float size, Color color, string parentType, int stage = 0) : base(world, Register.LizardEgg, obj, pos, ID)
        {
            this.color = color;
            this.parentID = parentID;
            this.stage = stage;
            this.parentType = parentType;
            if (stage > 0) this.size = size;
            else this.size = Mathf.Sqrt(0.7f * Mathf.Pow(size, 0.7f));
        }

        public override void Realize()
        {
            if (realizedObject == null)
                realizedObject = new LizardEgg(this);
        }

        public override string ToString()
        {
            string text = $"{ID}<oA>{type}<oA>{pos.SaveToString()}<oA>{FCustom.ColorToInt(color)}<oA>{size}<oA>{parentID}<oA>{stage}<oA>{parentType}";
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
        }
        public string parentType;
        public int stage;
        public Color color;
        public float size;
        public EntityID parentID;
    }
}
