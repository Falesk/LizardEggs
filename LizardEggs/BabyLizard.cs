namespace LizardEggs
{
    public class AbstractBabyLizard : AbstractCreature
    {
        public AbstractBabyLizard(World world, CreatureTemplate creatureTemplate, WorldCoordinate pos, EntityID ID, CreatureTemplate parent, int stage = 0) : base(world, creatureTemplate, null, pos, ID)
        {
            this.stage = stage;
            this.parent = parent;
            state = new LizardState(this);
        }

        public override string ToString()
        {
            string text = $"{creatureTemplate.type}<cA>{pos.SaveToString()}<cA>{ID}<cA>{parent.type}<cA>{stage}";
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<cA>", unrecognizedAttributes);
        }

        public CreatureTemplate parent;
        public int stage;
    }
}
