using UnityEngine;

namespace LizardEggs
{
    public class AbstractLizardEgg : AbstractPhysicalObject
    {
        public AbstractLizardEgg(AbstractPhysicalObject abstrObj) : base(abstrObj.world, Register.LizardEgg, null, abstrObj.pos, abstrObj.ID)
        {
            parentID = abstrObj.world.game.GetNewID();
            birthday = abstrObj.world.regionState.saveState.cycleNumber;
            parentType = FDataMananger.lizTypes[Random.Range(0, FDataMananger.lizTypes.Count)].type.value;
            color = (StaticWorld.GetCreatureTemplate(parentType).breedParameters as LizardBreedParams).standardColor;
            size = Random.Range(0.7f, 2f);
        }
        public AbstractLizardEgg(World world, WorldCoordinate pos, EntityID ID, EntityID parentID, float size, Color color, string parentType, int birthday) : base(world, Register.LizardEgg, null, pos, ID)
        {
            this.color = color;
            this.parentID = parentID;
            this.birthday = birthday;
            this.parentType = parentType;
            this.size = size;
        }

        public override string ToString()
        {
            string text = $"{ID}<oA>{type}<oA>{pos.SaveToString()}<oA>{FCustom.ARGB2HEX(color)}<oA>{size}<oA>{parentID}<oA>{birthday}<oA>{parentType}";
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
        }

        public string parentType;
        public int birthday;
        public Color color;
        public float size;
        public EntityID parentID;
    }
}