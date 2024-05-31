using UnityEngine;

namespace LizardEggs
{
    public class AbstractLizardEgg : AbstractPhysicalObject
    {
        public AbstractLizardEgg(AbstractPhysicalObject abstrObj) : base(abstrObj.world, Register.LizardEgg, null, abstrObj.pos, abstrObj.ID)
        {
            color = new Color(Random.value, Random.value, Random.value);
            parentID = world.game.GetNewID();
            stage = 0;
            parentType = "";
            size = Mathf.Sqrt(0.7f * Mathf.Pow(Random.Range(1.5f, 4f), 0.7f));
        }

        public AbstractLizardEgg(World world, WorldCoordinate pos, EntityID ID, EntityID parentID, float size, Color color, string parentType, int stage = 0, bool fromStr = false) : base(world, Register.LizardEgg, null, pos, ID)
        {
            this.color = color;
            this.parentID = parentID;
            this.stage = stage;
            this.parentType = parentType;
            if (fromStr) this.size = size;
            else this.size = Mathf.Sqrt(0.7f * Mathf.Pow(size, 0.7f));
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
