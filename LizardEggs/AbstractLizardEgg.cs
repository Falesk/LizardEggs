using UnityEngine;

namespace LizardEggs
{
    class AbstractLizardEgg : AbstractPhysicalObject
    {
        public AbstractLizardEgg(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID, EntityID parentID, float size, Color color, bool fromStr = false) : base(world, Register.LizardEgg, obj, pos, ID)
        {
            this.color = color;
            this.parentID = parentID;
            if (fromStr) this.size = size;
            else this.size = Mathf.Sqrt(0.7f * Mathf.Pow(size, 0.7f));
        }
        public override void Realize()
        {
            if (realizedObject == null)
                realizedObject = new LizardEgg(this);
        }
        public override string ToString()
        {
            string text = $"{ID}<oA>{type}<oA>{pos.SaveToString()}<oA>{color.r}<oA>{color.g}<oA>{color.b}<oA>{size}<oA>{parentID}";
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
        }
        public Color color;
        public float size;
        public EntityID parentID;
    }
}
