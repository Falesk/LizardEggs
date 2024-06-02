using UnityEngine;

namespace LizardEggs
{
    public class Indicator : CosmeticSprite
    {
        public Indicator(WorldCoordinate den, Room room)
        {
            this.den = den;
            this.room = room;
            pos = room.MiddleOfTile(room.LocalCoordinateOfNode(den.abstractNode).Tile);
            lizard = (Plugin.EggsInDen[den].Item1?.realizedCreature as Lizard) ?? new Lizard(Plugin.EggsInDen[den].Item1, room.world);
            color = lizard.effectColor;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];
            sLeaser.sprites[0] = new FSprite("Futile_White")
            {
                scale = 1.75f,
                shader = rCam.room.game.rainWorld.Shaders["FlatLight"],
                color = Color.Lerp(color, Color.white, 0.4f),
                alpha = Register.indBrightness.Value
            };
            sLeaser.sprites[1] = new FSprite("Futile_White")
            {
                scale = 3.5f,
                shader = rCam.room.game.rainWorld.Shaders["LightSource"],
                color = color,
                alpha = Register.indBrightness.Value
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].x = pos.x - camPos.x;
                sLeaser.sprites[i].y = pos.y - camPos.y;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("Bloom");
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContatiner.AddChild(sprite);
            }
        }

        public WorldCoordinate den;
        public Color color;
        public Lizard lizard;
    }
}
