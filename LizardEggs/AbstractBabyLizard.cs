using UnityEngine;

namespace LizardEggs
{
    public class AbstractBabyLizard : AbstractCreature
    {
        public AbstractBabyLizard(World world, CreatureTemplate creatureTemplate, WorldCoordinate pos, EntityID ID, Color color, int stage = 0) : base(world, creatureTemplate, null, pos, ID)
        {
            this.stage = stage;
            this.color = color;
            state = new LizardState(this);
        }

        public override string ToString()
        {
            string text = $"{creatureTemplate.type}<cA>{ID}<cA>{pos.SaveToString()}<cA>{FCustom.ColorToInt(color)}<cA>{stage}";
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<cA>", unrecognizedAttributes);
        }

        public Color color;
        public int stage;
    }
}
