using RWCustom;
using UnityEngine;

namespace LizardEggs
{
    public class LizardEgg : PlayerCarryableItem, IPlayerEdible, IDrawable
    {
        public AbstractLizardEgg AbstractLizardEgg => abstractPhysicalObject as AbstractLizardEgg;
        public LizardEgg(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, Vector2.zero, 8 * (1f + 0.15f * AbstractLizardEgg.stage), 0.2f * AbstractLizardEgg.size * (1f + 0.2f * AbstractLizardEgg.stage));
            bodyChunkConnections = new BodyChunkConnection[0];
            gravity = 0.9f;
            airFriction = 0.999f;
            waterFriction = 0.92f;
            surfaceFriction = 0.55f;
            collisionLayer = 1;
            bounce = 0.1f;
            buoyancy = 1.01f;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game.devToolsActive && Input.GetKey("b"))
                firstChunk.vel += Custom.DirVec(firstChunk.pos, Futile.mousePosition) * 3f;
            lastRotation = rotation;
            if (grabbedBy.Count > 0)
            {
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                rotation.y = Mathf.Abs(rotation.y);
            }
            if (setRotation != null)
            {
                rotation = setRotation.Value;
                setRotation = null;
            }
            if (firstChunk.ContactPoint.y < 0)
            {
                rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * firstChunk.vel.x).normalized;
                BodyChunk chunk = firstChunk;
                chunk.vel.x *= 0.8f;
            }
            lastShaking--;
            if (lastShaking < 0 && Random.value < 0.0025f && AbstractLizardEgg.stage > 0)
            {
                firstChunk.vel += Custom.RNV() * Random.Range(1f, 3f);
                lastShaking = Random.Range(120 / AbstractLizardEgg.stage, 400 / AbstractLizardEgg.stage);
            }

            bool flag = false;
            foreach (AbstractCreature abstr in room.abstractRoom.creatures)
                if (abstr.ID == AbstractLizardEgg.parentID)
                {
                    flag = true;
                    break;
                }
            if (flag && AbstractLizardEgg.stage == 0)
            {
                lightIntensity += 0.025f;
                if (lightIntensity > 1f)
                    lightIntensity = -1;
            }
            else if (AbstractLizardEgg.realizedObject != null && AbstractLizardEgg.stage == 2)
            {
                lightIntensity += 0.01f;
                if (lightIntensity > 1f)
                    lightIntensity = -1;
            }
            else
            {
                if (Mathf.Abs(lightIntensity) < 0.01f)
                    lightIntensity = 0;
                else if (lightIntensity > 0)
                    lightIntensity -= 0.025f;
                else lightIntensity += 0.025f;
            }
            if (light != null)
            {
                light.setPos = firstChunk.pos;
                light.setAlpha = Luminance;
                light.setRad = AbstractLizardEgg.size * 5 * (0.7f + Mathf.Pow(Luminance * 0.7f, 1.6f));
                if (light.slatedForDeletetion || light.room != room)
                    light = null;
            }
            else
            {
                light = new LightSource(firstChunk.pos, false, color, this);
                room.AddObject(light);
            }

            if (AbstractLizardEgg.stage == 3)
                SpawnLizard();
        }
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(AbstractLizardEgg.pos.Tile));
            rotation = Custom.RNV();
            lastRotation = rotation;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[4];
            sLeaser.sprites[0] = new FSprite("SnailShellA");
            sLeaser.sprites[0].scale = 0.6f * AbstractLizardEgg.size * (1f + 0.2f * AbstractLizardEgg.stage);

            sLeaser.sprites[1] = new FSprite("SnailShellB");
            sLeaser.sprites[1].scale = 0.6f * AbstractLizardEgg.size * (1f + 0.2f * AbstractLizardEgg.stage);

            sLeaser.sprites[2] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"],
                scale = AbstractLizardEgg.size * (1f + 0.1f * AbstractLizardEgg.stage)
            };

            sLeaser.sprites[3] = new FSprite("Futile_White")
            {
                shader = rCam.game.rainWorld.Shaders["LightSource"],
                scale = AbstractLizardEgg.size * 3.5f * (1f + 0.2f * AbstractLizardEgg.stage)
            };
            AddToContainer(sLeaser, rCam, null);
        }
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
            lastDarkness = darkness;
            darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
            if (darkness != lastDarkness)
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].x = vector.x - camPos.x;
                sLeaser.sprites[i].y = vector.y - camPos.y;
                sLeaser.sprites[i].rotation = Custom.VecToDeg(v);
            }
            if (lightIntensity != 0)
            {
                sLeaser.sprites[2].x = vector.x - camPos.x - v.x * 3f;
                sLeaser.sprites[2].y = vector.y - camPos.y - v.y * 3f;
                sLeaser.sprites[2].alpha = Luminance;
                sLeaser.sprites[3].x = vector.x - camPos.x - v.x * 3f;
                sLeaser.sprites[3].y = vector.y - camPos.y - v.y * 3f;
                sLeaser.sprites[3].alpha = Luminance;
            }
            if (blink > 0 && Random.value < 0.5f)
                sLeaser.sprites[1].color = blinkColor;
            else sLeaser.sprites[1].color = Color.Lerp(color, rCam.currentPalette.blackColor, Luminance - 0.2f);
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }
        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.blackColor;
            color = Color.Lerp(AbstractLizardEgg.color, palette.blackColor, darkness);
            if (rCam.room.PlayersInRoom != null && rCam.room.PlayersInRoom.Count > 0)
            {
                try { color = Color.Lerp(color, PlayerGraphics.SlugcatColor(rCam.room.PlayersInRoom[0].slugcatStats?.name), 0.3f * AbstractLizardEgg.stage); }
                catch { color = Color.Lerp(color, Color.white, 0.3f * AbstractLizardEgg.stage); }
            }
            sLeaser.sprites[2].color = new Color(color.r, Mathf.Clamp01(color.g * 1.1f), Mathf.Clamp01(color.b * 1.2f));
            sLeaser.sprites[3].color = Color.Lerp(color, Color.white, 0.3f);
        }
        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
                newContatiner = rCam.ReturnFContainer("Items");
            foreach (FSprite sprite in sLeaser.sprites)
                sprite.RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
            rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[3]);
        }

        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            bites--;
            room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos);
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                Destroy();
            }
        }
        public void ThrowByPlayer()
        {
        }
        public void SpawnLizard()
        {
            AbstractCreature abstr = new AbstractCreature(room.world, FCustom.CreatureTemplateFromType(AbstractLizardEgg.parentType), null, AbstractLizardEgg.pos, room.game.GetNewID(AbstractLizardEgg.parentID.spawner));
            room.abstractRoom.AddEntity(abstr);
            if (abstr.GetData() is FCustom.Data data)
                data.isChild = true;
            abstr.RealizeInRoom();
            Lizard liz = abstr.realizedCreature as Lizard;
            liz.mainBodyChunk.HardSetPosition(room.MiddleOfTile(AbstractLizardEgg.pos.Tile));
            Player player = room.PlayersInRoom[0];
            liz.AI.friendTracker.friend = player;
            liz.AI.LizardPlayerRelationChange(1f, player.abstractCreature);
            Destroy();
        }

        public int BitesLeft => bites;
        public int FoodPoints => (AbstractLizardEgg.size > 1.24f) ? 2 : 1;
        public bool Edible => true;
        public bool AutomaticPickUp => true;
        public float Luminance => Mathf.Sin(Mathf.Abs(lightIntensity) * Mathf.PI);
        public Vector2 rotation;
        public Vector2 lastRotation;
        public Vector2? setRotation;
        public float lightIntensity = 0f;
        public float darkness;
        public float lastDarkness;
        public int lastShaking = 200;
        public int bites = 3;
        public LightSource light;
    }
}
