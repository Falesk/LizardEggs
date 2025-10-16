using UnityEngine;

namespace LizardEggs
{
    public class AbstractLizardEgg : AbstractPhysicalObject
    {
        public AbstractLizardEgg(AbstractPhysicalObject abstrObj) : base(abstrObj.world, Register.LizardEgg, null, abstrObj.pos, abstrObj.ID)
        {
            parentID = abstrObj.world.game.GetNewID();
            if (world.game.session is StoryGameSession session)
                birthday = session.saveState.cycleNumber;
            else birthday = 0;
            parentType = FDataManager.RandomLizard();
            color = (StaticWorld.GetCreatureTemplate(parentType).breedParameters as LizardBreedParams).standardColor + FCustom.RandomGray(0.15f);
            if (color == Color.black) color += 0.01f * Color.white;
            color.a = 1f;
            color = FCustom.Clamp01Color(color);
            size = Random.Range(1f, 10f);
            bites = 5;
            openTime = -1;
        }
        public AbstractLizardEgg(World world, WorldCoordinate pos, EntityID ID, EntityID parentID, float size, Color color, string parentType, int birthday, int bites = 5, int openTime = -1) : base(world, Register.LizardEgg, null, pos, ID)
        {
            this.color = color;
            if (color == Color.black) this.color += 0.01f * Color.white;
            this.parentID = parentID;
            this.birthday = birthday;
            this.parentType = parentType;
            this.size = size;
            this.bites = bites;
            this.openTime = openTime;
        }

        public override string ToString()
        {
            if (bites == 0) return "";
            string text = $"{ID}<oA>{type}<oA>{pos.SaveToString()}<oA>{FCustom.ARGB2HEX(color)}<oA>{size}<oA>{parentID}<oA>{birthday}<oA>{parentType}<oA>{bites}<oA>{openTime}";
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
        }

        public string parentType;
        public int birthday;
        public int openTime;
        public int bites;
        public Color color;
        public float size;
        public EntityID parentID;
    }
}