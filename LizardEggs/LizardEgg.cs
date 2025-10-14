using RWCustom;
using System.Linq;
using UnityEngine;

namespace LizardEggs
{
    /* TODO
     * Графика для разбитого яйца - TriangleMesh, Color, желток, физика?
     * Механика открытия яйца (хук в Player.GrabUpdate(), посмотреть задержку поедания, IL-hook)
     * Механика протухания яйца
     * Мб поворот подправить
     * Создать зловоние испорченного яйца
     * Сделать более удобный счётчик или что-то вроде того для lightIntensity
     * Почистить архитектуру после добавления новой механики
     * Почистить настройки Ремикса - убрать ненужное, если найдётся
    */
    public class LizardEgg : PlayerCarryableItem, IPlayerEdible, IDrawable
    {
        public Vector2 rotation, lastRotation;
        public Vector2[] frontCrack, backCrack;
        public float darkness, lastDarkness, lightIntensity = 0f;
        public int bites = 3, lastShaking = 200;
        public LightSource light;
        public bool opened;

        public AbstractLizardEgg AbstractLizardEgg => abstractPhysicalObject as AbstractLizardEgg;

        public LizardEgg(AbstractPhysicalObject abstr) : base(abstr)
        {
            float rad = Mathf.Lerp(0.65f, 1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) + 0.5f * AbsStage;
            float mass = Mathf.Lerp(0.01f, 0.1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) * Mathf.Pow(rad, 2);
            bodyChunks = new BodyChunk[]
            { new BodyChunk(this, 0, Vector2.zero, 10f * rad, mass) };
            bodyChunkConnections = new BodyChunkConnection[0];
            gravity = 0.9f;
            airFriction = 0.999f;
            waterFriction = 0.92f;
            surfaceFriction = 0.55f;
            collisionLayer = 1;
            bounce = 0.1f;
            buoyancy = 0.95f;
            Random.State rstate = Random.state;
            Random.InitState(abstr.ID.RandomSeed);
            frontCrack = new Vector2[Random.Range(5, 8)];
            backCrack = new Vector2[Random.Range(5, 8)];
            GenerateCracks();
            Random.state = rstate;
        }

        public void GenerateCracks()
        {
            frontCrack[0] = Vector2.zero;
            for (int i = 1; i < frontCrack.Length - 1; i++)
            {
                float x = (4f * i + Mathf.Lerp(-1f, 1f, Random.value)) / (4f * (frontCrack.Length - 1));
                float y = -0.15f * FCustom.RandomSinusoidDeviation(x, 0.5f);
                frontCrack[i] = new Vector2(x, y);
            }
            float rightEdge = Mathf.Lerp(-1f, 1f, Random.value) / (4f * (frontCrack.Length - 1));
            frontCrack[frontCrack.Length - 1] = new Vector2(1f, rightEdge);

            backCrack[0] = Vector2.zero;
            for (int i = 1; i < backCrack.Length - 1; i++)
            {
                float x = (4f * i + Mathf.Lerp(-1f, 1f, Random.value)) / (4f * (backCrack.Length - 1));
                float y = 0.2f * FCustom.RandomSinusoidDeviation(x, 0.65f);
                backCrack[i] = new Vector2(x, y);
            }
            backCrack[backCrack.Length - 1] = new Vector2(1f, rightEdge);
        }

        private static string PrintVectorArr(Vector2[] vectors)
        {
            string s = "";
            for (int i = 0; i < vectors.Length - 1; i++)
                s += $"({vectors[i].x}, {vectors[i].y}),";
            return s + $"({vectors[vectors.Length - 1].x}, {vectors[vectors.Length - 1].y})";
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (room.game.devToolsActive && Input.GetKey("b"))
                firstChunk.vel += Custom.DirVec(firstChunk.pos, Futile.mousePosition) * 3f;
            if (Input.GetKeyDown("0"))
                opened = !opened;
            lastRotation = rotation;
            if (grabbedBy.Count > 0)
            {
                rotation = Custom.PerpendicularVector(Custom.DirVec(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                rotation.y = Mathf.Abs(rotation.y);
            }
            if (firstChunk.ContactPoint.y < 0)
            {
                rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * firstChunk.vel.x).normalized;
                firstChunk.vel.x *= 0.85f;
            }

            if (lastShaking > 0)
                lastShaking--;
            if (lastShaking <= 0 && Random.value < 0.0025f && AbsStage > 0.33f)
            {
                firstChunk.vel += Custom.RNV() * Random.Range(2f, 4f) * (0.66f + AbsStage);
                lastShaking = Random.Range(120 / (int)(3 * AbsStage), 400 / (int)(3 * AbsStage));
            }

            if (room.abstractRoom.creatures.Any(abstr => abstr.ID == AbstractLizardEgg.parentID) && Stage == 0)
            {
                lightIntensity += 0.03f;
                if (lightIntensity > 1f)
                    lightIntensity = -1;
            }
            else if (Stage == Options.eggGrowthTime.Value - 1 && Stage != 0)
            {
                lightIntensity += 0.0125f;
                if (lightIntensity > 1f)
                    lightIntensity = -1;
            }
            else
            {
                if (Mathf.Abs(lightIntensity) < 0.01f)
                    lightIntensity = 0;
                else if (lightIntensity > 0)
                    lightIntensity -= 0.03f;
                else lightIntensity += 0.03f;
            }

            if (light != null)
            {
                light.setPos = firstChunk.pos;
                light.setAlpha = Luminance * Options.glowBrightness.Value;
                light.setRad = firstChunk.rad * 0.55f * (1f + Luminance);
                if (light.slatedForDeletetion || light.room != room)
                    light = null;
            }
            else
            {
                light = new LightSource(firstChunk.pos, false, Color.Lerp(AbstractLizardEgg.color, Color.white, 0.15f), this);
                room.AddObject(light);
            }

            if (opened)
            {
                firstChunk.rad = 3.5f * (Mathf.Lerp(0.65f, 1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) + 0.5f * AbsStage);
                firstChunk.mass = 0.5f * Mathf.Lerp(0.01f, 0.1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) * Mathf.Pow(firstChunk.rad / 3.5f, 2);
            }
            else
            {
                firstChunk.rad = 10f * (Mathf.Lerp(0.65f, 1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) + 0.5f * AbsStage);
                firstChunk.mass = Mathf.Lerp(0.01f, 0.1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) * Mathf.Pow(firstChunk.rad / 10f, 2);
            }
        }

        public override void Destroy()
        {
            light?.Destroy();
            base.Destroy();
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
                new FSprite($"LizardEggA{(opened ? 1 : 0)}") { scale = firstChunk.rad / 10f },
                new FSprite($"LizardEggB{(opened ? 1 : 0)}") { scale = firstChunk.rad / 10f },
                TriangleMesh.MakeLongMesh(backCrack.Length - 1, false, true),
                TriangleMesh.MakeLongMesh(frontCrack.Length - 1, false, true)
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
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].SetPosition(pos - camPos);
                sLeaser.sprites[i].rotation = Custom.VecToDeg(rt);
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("LizardEgg" + (i % 2 == 0 ? "A" : "B") + (opened ? "1" : "0"));
            }
            if (blink > 0 && Random.value < 0.5f)
                sLeaser.sprites[1].color = blinkColor;
            else sLeaser.sprites[1].color = Color.Lerp(color, rCam.currentPalette.blackColor, 0.3f * (1f - Luminance));
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[0].color = palette.blackColor;
            if (rCam.room.PlayersInRoom?.Count > 0)
                color = Color.Lerp(AbstractLizardEgg.color, Color.white, 0.3f * AbsStage);
            color = Color.Lerp(color, palette.blackColor, darkness);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner = newContatiner ?? rCam.ReturnFContainer("Items");
            foreach (FSprite sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContatiner.AddChild(sprite);
            }
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
                EntityID entityID = room.world.game.GetNewID(abstractPhysicalObject.ID.spawner);
                AbstractCreature abstrLiz = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(Register.BabyLizard), null, abstractPhysicalObject.pos, entityID);
                (abstrLiz.state as BabyLizardState).parent = StaticWorld.GetCreatureTemplate(FDataManager.RandomLizard()).type;
                (abstrLiz.state as BabyLizardState).LimbFix();
                room.abstractRoom.AddEntity(abstrLiz);
                abstrLiz.RealizeInRoom();
                Destroy();
            }
        }

        public void ThrowByPlayer() { }

        public int BitesLeft => bites;
        public int FoodPoints => 1 + (int)(AbstractLizardEgg.size / 5f);
        public bool Edible => true;
        public bool AutomaticPickUp => true;
        public float Luminance => 0.5f * (1f - Mathf.Cos(Mathf.PI * lightIntensity));
        public int Stage
        {
            get
            {
                if (abstractPhysicalObject.world.game.session is StoryGameSession session)
                    return session.saveState.cycleNumber - AbstractLizardEgg.birthday;
                else return 0;
            }
        }
        public float AbsStage => Mathf.Clamp01((float)Stage / Options.eggGrowthTime.Value);
    }
}