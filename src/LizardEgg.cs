using RWCustom;
using System.Linq;
using UnityEngine;

namespace LizardEggs
{
    public class LizardEgg : PlayerCarryableItem, IPlayerEdible, IDrawable
    {
        public AbstractLizardEgg AbstractLizardEgg => abstractPhysicalObject as AbstractLizardEgg;
        public LizardEgg(AbstractPhysicalObject abstr) : base(abstr)
        {
            bodyChunks = new BodyChunk[]
            { new BodyChunk(this, 0, Vector2.zero, 8 * (1f + 0.5f * AbsStage), 0.2f * AbstractLizardEgg.size * (1f + 0.6f * AbsStage)) };
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
            if (firstChunk.ContactPoint.y < 0)
            {
                rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * firstChunk.vel.x).normalized;
                firstChunk.vel.x *= 0.8f;
            }

            if (lastShaking > 0)
                lastShaking--;
            if (lastShaking <= 0 && Random.value < 0.0025f && AbsStage > 0.33f)
            {
                firstChunk.vel += Custom.RNV() * Random.Range(1f, 3f) * (0.66f + AbsStage);
                lastShaking = Random.Range(120 / (int)(3 * AbsStage), 400 / (int)(3 * AbsStage));
            }

            if (room.abstractRoom.creatures.Any(abstr => abstr.ID == AbstractLizardEgg.parentID) && AbstractLizardEgg.stage == 0)
            {
                lightIntensity += 0.025f;
                if (lightIntensity > 1f)
                    lightIntensity = -1;
            }
            else if (AbstractLizardEgg.stage == Options.eggGrowthTime.Value - 1 && AbstractLizardEgg.stage != 0)
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
                light.setAlpha = Luminance * Options.glowBrightness.Value;
                light.setRad = AbstractLizardEgg.size * 5 * (0.7f + Mathf.Pow(Luminance * 0.7f, 1.6f));
                if (light.slatedForDeletetion || light.room != room)
                    light = null;
            }
            else
            {
                light = new LightSource(firstChunk.pos, false, Color.Lerp(AbstractLizardEgg.color, Color.white, 0.15f), this);
                room.AddObject(light);
            }
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
            sLeaser.sprites = new FSprite[]
            {
                new FSprite("SnailShellA") { scale = 0.6f * AbstractLizardEgg.size * (1f + 0.6f * AbsStage) },
                new FSprite("SnailShellB") { scale = 0.6f * AbstractLizardEgg.size * (1f + 0.6f * AbsStage) },
                new FSprite("Futile_White")
                {
                    shader = rCam.game.rainWorld.Shaders["LightSource"],
                    scale = AbstractLizardEgg.size * 3.5f * (1f + 0.6f * AbsStage)
                }
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 rt = Vector3.Slerp(lastRotation, rotation, timeStacker);
            lastDarkness = darkness;
            darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
            if (darkness != lastDarkness)
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            for (int i = 0; i < 2; i++)
            {
                sLeaser.sprites[i].x = pos.x - camPos.x;
                sLeaser.sprites[i].y = pos.y - camPos.y;
                sLeaser.sprites[i].rotation = Custom.VecToDeg(rt);
            }
            if (lightIntensity != 0)
            {
                sLeaser.sprites[2].x = pos.x - camPos.x - rt.x * 3f;
                sLeaser.sprites[2].y = pos.y - camPos.y - rt.y * 3f;
                sLeaser.sprites[2].alpha = Luminance * Options.glowBrightness.Value;
            }
            if (blink > 0 && Random.value < 0.5f)
                sLeaser.sprites[1].color = blinkColor;
            else sLeaser.sprites[1].color = Color.Lerp(color, rCam.currentPalette.blackColor, Luminance - 0.2f - 0.6f * AbsStage);
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.blackColor;
            if (rCam.room.PlayersInRoom?.Count > 0)
            {
                try { color = Color.Lerp(AbstractLizardEgg.color, PlayerGraphics.SlugcatColor(rCam.room.PlayersInRoom[0].slugcatStats?.name), 0.4f * AbsStage); }
                catch { color = Color.Lerp(AbstractLizardEgg.color, Color.white, 0.3f * AbsStage); }
            }
            color = Color.Lerp(color, palette.blackColor, darkness);
            sLeaser.sprites[2].color = color;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner = newContatiner ?? rCam.ReturnFContainer("Items");
            foreach (FSprite sprite in sLeaser.sprites)
                sprite.RemoveFromContainer();
            newContatiner.AddChild(sLeaser.sprites[0]);
            newContatiner.AddChild(sLeaser.sprites[1]);
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[2]);
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

        public override void HitByWeapon(Weapon weapon)
        {
            if (abstractPhysicalObject.world.game.IsArenaSession && weapon is Spear)
            {
                FCustom.InitLizTypes();
                EntityID entityID = room.world.game.GetNewID(abstractPhysicalObject.ID.spawner);
                CreatureTemplate.Type type = FCustom.lizTypes[Random.Range(0, FCustom.lizTypes.Count)].type;
                Plugin.lizards.Add(new Plugin.SavedLizard(entityID, type, Options.lizGrowthTime.Value - 1) { slatedForDeletion = true });
                AbstractCreature abstrLiz = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(Register.BabyLizard), null, abstractPhysicalObject.pos, entityID);
                room.abstractRoom.AddEntity(abstrLiz);
                abstrLiz.RealizeInRoom();
                Destroy();
            }
        }

        public void ThrowByPlayer() { }

        public int BitesLeft => bites;
        public int FoodPoints => 1 + (int)(AbstractLizardEgg.size / 1.24f);
        public bool Edible => true;
        public bool AutomaticPickUp => true;
        public float Luminance => Mathf.Pow(Mathf.Sin(lightIntensity * Mathf.PI), 2);
        public float AbsStage => Mathf.Clamp01((float)AbstractLizardEgg.stage / Options.eggGrowthTime.Value);
        public Vector2 rotation;
        public Vector2 lastRotation;
        public float darkness;
        public float lastDarkness;
        public float lightIntensity = 0f;
        public int bites = 3;
        public int lastShaking = 200;
        public LightSource light;
    }
}