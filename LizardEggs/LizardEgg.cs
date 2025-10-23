using RWCustom;
using System.Linq;
using UnityEngine;

namespace LizardEggs
{
    public class LizardEgg : PlayerCarryableItem, IPlayerEdible, IDrawable
    {
        public Vector2 rotation, lastRotation;
        public Vector2[] frontCrack, backCrack;
        public float darkness, lastDarkness, lightIntensity = 0f;
        public int lastShaking = 200;
        public bool justOpened;
        public LightSource light;
        public Color yolkColor;

        public AbstractLizardEgg AbstractLizardEgg => abstractPhysicalObject as AbstractLizardEgg;

        public LizardEgg(AbstractPhysicalObject abstr) : base(abstr)
        {
            float rad = Mathf.Lerp(0.65f, 1.1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) + 0.5f * AbsStage;
            float mass = 2f * Mathf.Lerp(0.01f, 0.1f, Mathf.InverseLerp(1, 10, AbstractLizardEgg.size)) * Mathf.Pow(rad, 2);
            bodyChunks = new BodyChunk[]
            { new BodyChunk(this, 0, Vector2.zero, (Opened ? 3.5f : 10f) * rad, (Opened ? 0.5f : 1f) * mass) };
            bodyChunkConnections = new BodyChunkConnection[0];
            gravity = 0.9f;
            airFriction = 0.999f;
            waterFriction = 0.92f;
            surfaceFriction = 0.55f;
            collisionLayer = 1;
            bounce = 0.1f;
            buoyancy = 0.95f;
            Random.State rstate = Random.state;
            Random.InitState(abstractPhysicalObject.ID.RandomSeed);
            frontCrack = new Vector2[Random.Range(5, 8)];
            backCrack = new Vector2[Random.Range(5, 8)];
            GenerateCracks();
            yolkColor = new Color(Rotten ? Mathf.Lerp(0.4f, 0.6f, Random.value) : 1f, Mathf.Lerp(0.4f, 1f, Random.value), Mathf.Lerp(0f, 0.35f, Random.value));
            Random.state = rstate;
        }

        public void GenerateCracks()
        {
            frontCrack[0] = Vector2.zero;
            for (int i = 1; i < frontCrack.Length - 1; i++)
            {
                float x = (4f * i + Mathf.Lerp(-1f, 1f, Random.value)) / (4f * (frontCrack.Length - 1));
                float y = Mathf.Lerp(-0.1f, 0.2f, Random.value);
                frontCrack[i] = new Vector2(x, y);
            }
            float rightEdge = Mathf.Lerp(-1f, 1f, Random.value) / (4f * (frontCrack.Length - 1));
            frontCrack[frontCrack.Length - 1] = new Vector2(1f, rightEdge);

            backCrack[0] = Vector2.zero;
            for (int i = 1; i < backCrack.Length - 1; i++)
            {
                float x = (4f * i + Mathf.Lerp(-1f, 1f, Random.value)) / (4f * (backCrack.Length - 1));
                float y = 0.35f * FCustom.RandomSinusoidDeviation(x, 1f);
                backCrack[i] = new Vector2(x, y);
            }
            backCrack[backCrack.Length - 1] = new Vector2(1f, rightEdge);
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
                firstChunk.vel.x *= 0.85f;
            }

            if (!Opened && !Rotten)
            {
                Shaking();
                LightIntensityChange();
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
        }

        public void Shaking()
        {
            if (lastShaking > 0)
                lastShaking--;
            if (lastShaking <= 0 && Random.value < 0.0025f && AbsStage > 0.33f)
            {
                firstChunk.vel += Custom.RNV() * Random.Range(2f, 4f) * (0.66f + AbsStage);
                lastShaking = Random.Range(120 / (int)(3 * AbsStage), 400 / (int)(3 * AbsStage));
            }
        }

        public void LightIntensityChange()
        {
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
                TriangleMesh.MakeLongMesh(backCrack.Length - 1, false, true),
                new FSprite("BodyA") { isVisible = Opened },
                TriangleMesh.MakeLongMesh(frontCrack.Length - 1, false, true),
                new FSprite($"LizardEggA{(Opened ? 1 : 0)}"),
                new FSprite($"LizardEggB{(Opened ? 1 : 0)}"),
            };
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            Vector2 rt = Vector3.Slerp(lastRotation, rotation, timeStacker);
            float sizeFac = firstChunk.rad / 3.5f;
            float sizeFacMain = Opened ? sizeFac : firstChunk.rad / 10f;

            lastDarkness = darkness;
            darkness = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos));
            if (darkness != lastDarkness)
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            for (int i = 3; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].SetPosition(pos - camPos);
                sLeaser.sprites[i].rotation = Custom.VecToDeg(rt);
                sLeaser.sprites[i].scale = sizeFacMain;
            }

            if (Opened)
            {
                TriangleMesh backCr = sLeaser.sprites[0] as TriangleMesh;
                backCr.SetPosition(pos - camPos);
                Vector2 spriteSize = new Vector2(20f, 12f) * sizeFac;
                Vector2 root = Vector2.down * spriteSize.y * 0.5f;

                Vector2 fixPoint = new Vector2(-0.5f, 1) * spriteSize;
                Vector2 tipPoint = Vector2.zero;
                Vector2 refSysVec = fixPoint;
                for (int i = 0; i < backCrack.Length - 1; i++)
                {
                    float x = spriteSize.x * Mathf.Lerp(-0.5f, 0.5f, (i + 1) / (float)(backCrack.Length - 1));
                    float y = spriteSize.y - 3f * sizeFac * Mathf.Cos(x * Mathf.PI / spriteSize.x) - 1.5f * sizeFac;
                    Vector2 fixPointNext = new Vector2(x, y);
                    Vector2 tipPointNext = backCrack[i + 1] * spriteSize;

                    backCr.MoveVertice(4 * i + 0, root + fixPoint);
                    backCr.MoveVertice(4 * i + 1, root + fixPointNext);
                    backCr.MoveVertice(4 * i + 2, root + refSysVec + tipPoint);
                    backCr.MoveVertice(4 * i + 3, root + refSysVec + tipPointNext);
                    fixPoint = fixPointNext;
                    tipPoint = tipPointNext;
                }
                backCr.rotation = Custom.VecToDeg(rt);

                FSprite yolk = sLeaser.sprites[1];
                yolk.SetPosition(pos - camPos);
                yolk.rotation = Custom.VecToDeg(rt);
                float scale = Mathf.InverseLerp(0, 4, AbstractLizardEgg.bites);
                yolk.scaleX = Mathf.Lerp(0f, 1f, scale) * sizeFacMain * 1.1f;
                yolk.scaleY = -0.5f * sizeFac * Mathf.Lerp(0.5f, 1f, scale);
                yolk.anchorY = Mathf.Lerp(1.1f, 1.3f, scale);

                TriangleMesh frontCr = sLeaser.sprites[2] as TriangleMesh;
                frontCr.SetPosition(pos - camPos);

                fixPoint = new Vector2(-0.5f, 0) * spriteSize.y;
                tipPoint = Vector2.zero;
                for (int i = 0; i < frontCrack.Length - 1; i++)
                {
                    float x = spriteSize.x * Mathf.Lerp(-0.5f, 0.5f, (i + 1) / (float)(frontCrack.Length - 1));
                    float y = spriteSize.y - 3f * sizeFac * Mathf.Cos(x * Mathf.PI / spriteSize.x) - 3f * sizeFac;
                    Vector2 fixPointNext = new Vector2(x, y);
                    Vector2 tipPointNext = frontCrack[i + 1] * spriteSize;

                    frontCr.MoveVertice(4 * i + 0, root + fixPoint);
                    frontCr.MoveVertice(4 * i + 1, root + fixPointNext);
                    frontCr.MoveVertice(4 * i + 2, root + refSysVec + tipPoint);
                    frontCr.MoveVertice(4 * i + 3, root + refSysVec + tipPointNext);
                    fixPoint = fixPointNext;
                    tipPoint = tipPointNext;
                }
                frontCr.rotation = Custom.VecToDeg(rt);

                if (justOpened)
                {
                    sLeaser.sprites[3].element = Futile.atlasManager.GetElementWithName("LizardEggA1");
                    sLeaser.sprites[4].element = Futile.atlasManager.GetElementWithName("LizardEggB1");
                    yolk.isVisible = true;
                    ColorizeMesh(sLeaser);
                    justOpened = false;
                }
            }

            BlinkColor(sLeaser, rCam);
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            sLeaser.sprites[3].color = Opened ? Color.Lerp(color, rCam.currentPalette.blackColor, 0.85f) : palette.blackColor;
            if (rCam.room.PlayersInRoom?.Count > 0)
                color = Color.Lerp(AbstractLizardEgg.color, Color.white, 0.3f * AbsStage);
            color = Color.Lerp(color, palette.blackColor, darkness);
            ColorizeMesh(sLeaser);
        }

        public void ColorizeMesh(RoomCamera.SpriteLeaser sLeaser)
        {
            TriangleMesh backCr = sLeaser.sprites[0] as TriangleMesh;
            Vector3 hslColor = Custom.RGB2HSL(color);
            Color inner = Color.Lerp(Custom.HSL2RGB(hslColor.x, 1f - hslColor.y, 1f - hslColor.z), color, 0.4f);
            for (int i = 0; i < backCr.verticeColors.Length; i++)
                backCr.verticeColors[i] = inner;

            sLeaser.sprites[1].color = Color.Lerp(yolkColor, sLeaser.sprites[3].color, Mathf.Pow(darkness, 2));

            TriangleMesh frontCr = sLeaser.sprites[2] as TriangleMesh;
            Color outer = Color.Lerp(sLeaser.sprites[3].color, color, 0.3f);
            for (int i = 0; i < frontCr.verticeColors.Length; i++)
                frontCr.verticeColors[i] = outer;
        }

        public void BlinkColor(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (blink > 0 && Random.value < 0.5f)
            {
                sLeaser.sprites[4].color = blinkColor;
                TriangleMesh backCr = sLeaser.sprites[0] as TriangleMesh;
                for (int i = 0; i < backCr.verticeColors.Length; i++)
                    backCr.verticeColors[i] = blinkColor;
                TriangleMesh frontCr = sLeaser.sprites[2] as TriangleMesh;
                for (int i = 0; i < frontCr.verticeColors.Length; i++)
                    frontCr.verticeColors[i] = blinkColor;
            }
            else
            {
                sLeaser.sprites[4].color = Color.Lerp(color, rCam.currentPalette.blackColor, 0.2f * (1f - Luminance));
                ColorizeMesh(sLeaser);
            }
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

        public void Open()
        {
            if (Opened)
                return;
            if (abstractPhysicalObject.world.game.session is StoryGameSession session)
                AbstractLizardEgg.openTime = session.saveState.cycleNumber;
            else AbstractLizardEgg.openTime = 0;
            justOpened = true;
            firstChunk.rad *= 0.35f;
            firstChunk.mass *= 0.5f;
            room.PlaySound(DLCSharedEnums.SharedSoundID.Duck_Pop, firstChunk, false, 1f, 0.5f + Random.value * 0.5f);
            for (int i = 0; i < 5; i++)
                room.AddObject(new WaterDrip(firstChunk.pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), false));
        }

        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            if (AbstractLizardEgg.bites == 5 && !Opened)
                Open();
            AbstractLizardEgg.bites--;
            room.PlaySound((AbstractLizardEgg.bites == 0) ? SoundID.Slugcat_Eat_Slime_Mold : SoundID.Slugcat_Bite_Slime_Mold, firstChunk.pos);
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (AbstractLizardEgg.bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
            }
            if (Rotten) (grasp.grabber as Player).Stun(100 + (int)(Random.value * 100));
        }

        public override void HitByWeapon(Weapon weapon)
        {
            if (abstractPhysicalObject.world.game.IsArenaSession && weapon is Spear && !Opened)
            {
                EntityID entityID = room.world.game.GetNewID(abstractPhysicalObject.ID.spawner);
                AbstractCreature abstrLiz = new AbstractCreature(room.world, StaticWorld.GetCreatureTemplate(Register.BabyLizard), null, abstractPhysicalObject.pos, entityID);
                (abstrLiz.state as BabyLizardState).parent = StaticWorld.GetCreatureTemplate(FDataManager.RandomLizard()).type;
                (abstrLiz.state as BabyLizardState).LimbFix();
                room.abstractRoom.AddEntity(abstrLiz);
                abstrLiz.RealizeInRoom();
                Destroy();
            }
            else if (weapon is Rock || weapon is Spear)
            {
                firstChunk.vel = new Vector2(Mathf.Lerp(-0.5f, 0.5f, Random.value), Mathf.Lerp(0f, 1f, Random.value)).normalized * Mathf.Lerp(5, 8, Random.value);
                if (AbstractLizardEgg.bites == 5) AbstractLizardEgg.bites--;
                Open();
            }
            else if (weapon is ScavengerBomb)
            {
                for (int i = 0; i < 5; i++)
                    room.AddObject(new WaterDrip(firstChunk.pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), false));
                Destroy();
            }
        }

        public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
        {
            base.HitByExplosion(hitFac, explosion, hitChunk);
            if (Random.value < hitFac)
            {
                for (int i = 0; i < 5; i++)
                    room.AddObject(new WaterDrip(firstChunk.pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), false));
                Destroy();
            }
        }

        public void ThrowByPlayer() { }

        public int BitesLeft => AbstractLizardEgg.bites;
        public int FoodPoints => Rotten ? 0 : 1 + (int)(AbstractLizardEgg.size / 5f);
        public bool Edible => AbstractLizardEgg.bites != 0;
        public bool AutomaticPickUp => true;
        public float Luminance => 0.5f * (1f - Mathf.Cos(Mathf.PI * lightIntensity));
        public int Stage
        {
            get
            {
                if (abstractPhysicalObject.world.game.session is StoryGameSession session)
                    return Mathf.Abs((Opened ? AbstractLizardEgg.openTime : session.saveState.cycleNumber) - AbstractLizardEgg.birthday);
                else return 0;
            }
        }
        public bool Rotten
        {
            get
            {
                if (Opened && abstractPhysicalObject.world.game.session is StoryGameSession session)
                    return Mathf.Abs(session.saveState.cycleNumber - AbstractLizardEgg.openTime) > 0;
                return Stillborn;
            }
        }
        public float AbsStage => Mathf.Clamp01((float)Stage / Options.eggGrowthTime.Value);
        public bool Opened => AbstractLizardEgg.openTime != -1;
        public bool Stillborn => Options.stillborn.Value && !Opened && Stage > Options.eggGrowthTime.Value;
    }
}