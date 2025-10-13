using UnityEngine;

namespace LizardEggs
{
    public class Indicator : CosmeticSprite
    {
        public WorldCoordinate den;
        public Color color;
        public Lizard lizard;

        public Indicator(WorldCoordinate _den, Room _room)
        {
            den = _den;
            room = _room;
            pos = room.MiddleOfTile(room.LocalCoordinateOfNode(den.abstractNode).Tile);
            lizard = FDataManager.Dens[den].Item1?.realizedCreature as Lizard ?? new Lizard(FDataManager.Dens[den].Item1, room.world);
            color = lizard.effectColor;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (!room.BeingViewed)
                Destroy();
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[]
            {
                new FSprite("Futile_White")
                {
                    scale = 1.75f,
                    shader = rCam.room.game.rainWorld.Shaders["FlatLight"],
                    color = Color.Lerp(color, Color.white, 0.4f),
                    alpha = Options.indBrightness.Value
                },
                new FSprite("Futile_White")
                {
                    scale = 3.5f,
                    shader = rCam.room.game.rainWorld.Shaders["LightSource"],
                    color = color,
                    alpha = Options.indBrightness.Value
                }
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            foreach (FSprite sprite in sLeaser.sprites)
                sprite.SetPosition(pos - camPos);
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner = newContatiner ?? rCam.ReturnFContainer("Bloom");
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContatiner.AddChild(sprite);
            }
        }
    }
}