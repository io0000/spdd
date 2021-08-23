using System.Collections.Generic;
using System.IO;
using watabou.glwrap;
using watabou.noosa;
using watabou.noosa.audio;
using watabou.noosa.particles;
using watabou.utils;
using spdd.actors;
using spdd.actors.blobs;
using spdd.actors.hero;
using spdd.actors.mobs;
using spdd.effects;
using spdd.items;
using spdd.items.artifacts;
using spdd.items.bags;
using spdd.items.potions;
using spdd.items.scrolls;
using spdd.journal;
using spdd.levels;
using spdd.levels.traps;
using spdd.messages;
using spdd.plants;
using spdd.sprites;
using spdd.tiles;
using spdd.ui;
using spdd.utils;
using spdd.windows;

namespace spdd.scenes
{
    public class GameScene : PixelScene
    {
        public static GameScene scene;

        private SkinnedBlock water;
        private DungeonTerrainTilemap tiles;
        private GridTileMap visualGrid;
        private TerrainFeaturesTilemap terrainFeatures;
        private RaisedTerrainTilemap raisedTerrain;
        private DungeonWallsTilemap walls;
        private WallBlockingTilemap wallBlocking;
        private FogOfWar fog;
        private HeroSprite hero;

        private StatusPane pane;

        private GameLog log;

        private BusyIndicator busy;
        private CircleArc counter;

        private static CellSelector cellSelector;

        private Group terrain;
        private Group customTiles;
        private Group levelVisuals;
        private Group customWalls;
        private Group ripples;
        //private Group plants;
        //private Group traps;
        private Group heaps;
        private Group mobs;
        private Group floorEmitters;
        private Group emitters;
        private Group effects;
        private Group gases;
        private Group spells;
        private Group statuses;
        private Group emoicons;
        private Group overFogEffects;
        private Group healthIndicators;

        private Toolbar toolbar;
        private Toast prompt;

        private AttackIndicator attack;
        private LootIndicator loot;
        private ActionIndicator action;
        private ResumeIndicator resume;

        public override void Create()
        {
            if (Dungeon.hero == null)
            {
                ShatteredPixelDungeonDash.SwitchNoFade(typeof(TitleScene));
                return;
            }

            Music.Instance.Play(Assets.Music.GAME, true);

            SPDSettings.LastClass(Dungeon.hero.heroClass.Ordinal());

            base.Create();
            Camera.main.Zoom(GameMath.Gate(minZoom, defaultZoom + SPDSettings.Zoom(), maxZoom));

            scene = this;

            terrain = new Group();
            Add(terrain);

            water = new GameSceneSkinnedBlock(
                Dungeon.level.Width() * DungeonTilemap.SIZE,
                Dungeon.level.Height() * DungeonTilemap.SIZE,
                Dungeon.level.WaterTex());

            terrain.Add(water);

            ripples = new Group();
            terrain.Add(ripples);

            DungeonTileSheet.SetupVariance(Dungeon.level.map.Length, Dungeon.SeedCurDepth());

            tiles = new DungeonTerrainTilemap();
            terrain.Add(tiles);

            customTiles = new Group();
            terrain.Add(customTiles);

            foreach (CustomTilemap visual in Dungeon.level.customTiles)
            {
                AddCustomTile(visual);
            }

            visualGrid = new GridTileMap();
            terrain.Add(visualGrid);

            terrainFeatures = new TerrainFeaturesTilemap(Dungeon.level.plants, Dungeon.level.traps);
            terrain.Add(terrainFeatures);

            levelVisuals = Dungeon.level.AddVisuals();
            Add(levelVisuals);

            floorEmitters = new Group();
            Add(floorEmitters);

            heaps = new Group();
            Add(heaps);

            foreach (Heap heap in Dungeon.level.heaps.Values)
            {
                AddHeapSprite(heap);
            }

            emitters = new Group();
            effects = new Group();
            healthIndicators = new Group();
            emoicons = new Group();
            overFogEffects = new Group();

            mobs = new Group();
            Add(mobs);

            hero = new HeroSprite();
            hero.Place(Dungeon.hero.pos);
            hero.UpdateArmor();
            mobs.Add(hero);

            foreach (Mob mob in Dungeon.level.mobs)
            {
                AddMobSprite(mob);
                if (Statistics.amuletObtained)
                {
                    mob.Beckon(Dungeon.hero.pos);
                }
            }

            raisedTerrain = new RaisedTerrainTilemap();
            Add(raisedTerrain);

            walls = new DungeonWallsTilemap();
            Add(walls);

            customWalls = new Group();
            Add(customWalls);

            foreach (CustomTilemap visual in Dungeon.level.customWalls)
            {
                AddCustomWall(visual);
            }

            wallBlocking = new WallBlockingTilemap();
            Add(wallBlocking);

            Add(emitters);
            Add(effects);

            gases = new Group();
            Add(gases);

            foreach (Blob blob in Dungeon.level.blobs.Values)
            {
                blob.emitter = null;
                AddBlobSprite(blob);
            }

            fog = new FogOfWar(Dungeon.level.Width(), Dungeon.level.Height());
            Add(fog);

            spells = new Group();
            Add(spells);

            Add(overFogEffects);

            statuses = new Group();
            Add(statuses);

            Add(healthIndicators);
            //always appears ontop of other health indicators
            Add(new TargetHealthIndicator());

            Add(emoicons);

            Add(cellSelector = new CellSelector(tiles));

            pane = new StatusPane();
            pane.camera = uiCamera;
            pane.SetSize(uiCamera.width, 0);
            Add(pane);

            toolbar = new Toolbar();
            toolbar.camera = uiCamera;
            toolbar.SetRect(0, uiCamera.height - toolbar.Height(), uiCamera.width, toolbar.Height());
            Add(toolbar);

            attack = new AttackIndicator();
            attack.camera = uiCamera;
            Add(attack);

            loot = new LootIndicator();
            loot.camera = uiCamera;
            Add(loot);

            action = new ActionIndicator();
            action.camera = uiCamera;
            Add(action);

            resume = new ResumeIndicator();
            resume.camera = uiCamera;
            Add(resume);

            log = new GameLog();
            log.camera = uiCamera;
            log.NewLine();
            Add(log);

            LayoutTags();

            busy = new BusyIndicator();
            busy.camera = uiCamera;
            busy.x = 1;
            busy.y = pane.Bottom() + 1;
            Add(busy);

            counter = new CircleArc(18, 4.25f);
            counter.Color(new Color(0x80, 0x80, 0x80, 0xFF), true);
            counter.camera = uiCamera;
            counter.Show(this, busy.Center(), 0f);

            switch (InterlevelScene.mode)
            {
                case InterlevelScene.Mode.RESURRECT:
                    ScrollOfTeleportation.Appear(Dungeon.hero, Dungeon.level.entrance);
                    new Flare(8, 32).Color(new Color(0xFF, 0xFF, 0x66, 0xFF), true).Show(hero, 2f);
                    break;
                case InterlevelScene.Mode.RETURN:
                    ScrollOfTeleportation.Appear(Dungeon.hero, Dungeon.hero.pos);
                    break;
                case InterlevelScene.Mode.DESCEND:
                    switch (Dungeon.depth)
                    {
                        case 1:
                            WndStory.ShowChapter(WndStory.ID_SEWERS);
                            break;
                        case 6:
                            WndStory.ShowChapter(WndStory.ID_PRISON);
                            break;
                        case 11:
                            WndStory.ShowChapter(WndStory.ID_CAVES);
                            break;
                        case 16:
                            WndStory.ShowChapter(WndStory.ID_CITY);
                            break;
                        case 21:
                            WndStory.ShowChapter(WndStory.ID_HALLS);
                            break;
                    }
                    if (Dungeon.hero.IsAlive())
                    {
                        BadgesExtensions.ValidateNoKilling();
                    }
                    break;
                    //default:
            }

            List<Item> dropped = Dungeon.droppedItems[Dungeon.depth];
            if (dropped != null)
            {
                foreach (Item item in dropped)
                {
                    int pos = Dungeon.level.RandomRespawnCell(null);
                    if (item is Potion)
                    {
                        ((Potion)item).Shatter(pos);
                    }
                    else if (item is Plant.Seed)
                    {
                        Dungeon.level.Plant((Plant.Seed)item, pos);
                    }
                    else if (item is Honeypot)
                    {
                        Dungeon.level.Drop(((Honeypot)item).Shatter(null, pos), pos);
                    }
                    else
                    {
                        Dungeon.level.Drop(item, pos);
                    }
                }
                Dungeon.droppedItems.Remove(Dungeon.depth);
            }

            List<Item> ported = Dungeon.portedItems[Dungeon.depth];
            if (ported != null)
            {
                //TODO currently items are only ported to boss rooms, so this works well
                //might want to have a 'near entrance' function if items can be ported elsewhere
                int pos;
                //try to find a tile with no heap, otherwise just stick items onto a heap.
                int tries = 100;
                do
                {
                    pos = Dungeon.level.RandomRespawnCell(null);
                    --tries;
                }
                while (tries > 0 && Dungeon.level.heaps[pos] != null);

                foreach (Item item in ported)
                {
                    Dungeon.level.Drop(item, pos).type = Heap.Type.CHEST;
                }
                Dungeon.level.heaps[pos].type = Heap.Type.CHEST;
                Dungeon.level.heaps[pos].sprite.Link(); //sprite reset to show chest
                Dungeon.portedItems.Remove(Dungeon.depth);
            }

            Dungeon.hero.Next();

            switch (InterlevelScene.mode)
            {
                case InterlevelScene.Mode.FALL:
                case InterlevelScene.Mode.DESCEND:
                case InterlevelScene.Mode.CONTINUE:
                    Camera.main.SnapTo(hero.Center().x, hero.Center().y - DungeonTilemap.SIZE * (defaultZoom / Camera.main.zoom));
                    break;
                case InterlevelScene.Mode.ASCEND:
                    Camera.main.SnapTo(hero.Center().x, hero.Center().y + DungeonTilemap.SIZE * (defaultZoom / Camera.main.zoom));
                    break;
                default:
                    Camera.main.SnapTo(hero.Center().x, hero.Center().y);
                    break;
            }
            Camera.main.PanTo(hero.Center(), 2.5f);

            if (InterlevelScene.mode != InterlevelScene.Mode.NONE)
            {
                if (Dungeon.depth == Statistics.deepestFloor &&
                    (InterlevelScene.mode == InterlevelScene.Mode.DESCEND || InterlevelScene.mode == InterlevelScene.Mode.FALL))
                {
                    GLog.Highlight(Messages.Get(this, "descend"), Dungeon.depth);
                    Sample.Instance.Play(Assets.Sounds.DESCEND);

                    foreach (var ch in Actor.Chars())
                    {
                        if (ch is DriedRose.GhostHero)
                        {
                            ((DriedRose.GhostHero)ch).SayAppeared();
                        }
                    }

                    int spawnersAbove = Statistics.spawnersAlive;
                    if (spawnersAbove > 0 && Dungeon.depth <= 25)
                    {
                        foreach (Mob m in Dungeon.level.mobs)
                        {
                            if (m is DemonSpawner && ((DemonSpawner)m).spawnRecorded)
                            {
                                --spawnersAbove;
                            }
                        }

                        if (spawnersAbove > 0)
                        {
                            if (Dungeon.BossLevel())
                            {
                                GLog.Negative(Messages.Get(this, "spawner_warn_final"));
                            }
                            else
                            {
                                GLog.Negative(Messages.Get(this, "spawner_warn"));
                            }
                        }
                    }
                }
                else if (InterlevelScene.mode == InterlevelScene.Mode.RESET)
                {
                    GLog.Highlight(Messages.Get(this, "warp"));
                }
                else
                {
                    GLog.Highlight(Messages.Get(this, "return"), Dungeon.depth);
                }

                switch (Dungeon.level.feeling)
                {
                    case Level.Feeling.CHASM:
                        GLog.Warning(Messages.Get(this, "chasm"));
                        break;
                    case Level.Feeling.WATER:
                        GLog.Warning(Messages.Get(this, "water"));
                        break;
                    case Level.Feeling.GRASS:
                        GLog.Warning(Messages.Get(this, "grass"));
                        break;
                    case Level.Feeling.DARK:
                        GLog.Warning(Messages.Get(this, "dark"));
                        break;
                        //default:
                }
                if (Dungeon.level is RegularLevel &&
                        ((RegularLevel)Dungeon.level).secretDoors > Rnd.IntRange(3, 4))
                {
                    GLog.Warning(Messages.Get(this, "secrets"));
                }

                InterlevelScene.mode = InterlevelScene.Mode.NONE;
            }

            FadeIn();
        }

        public class GameSceneSkinnedBlock : SkinnedBlock
        {
            public GameSceneSkinnedBlock(float width, float height, object tx)
                : base(width, height, tx)
            { }

            protected override NoosaScript Script()
            {
                return NoosaScriptNoLighting.Get();
            }

            public override void Draw()
            {
                //water has no alpha component, this improves performance
                Blending.Disable();
                base.Draw();
                Blending.Enable();
            }
        }

        public override void Destroy()
        {
            //tell the actor thread to finish, then wait for it to complete any actions it may be doing.
            //if (actorThread != null && actorThread.isAlive()){
            //    synchronized (GameScene.class){
            //	    synchronized (actorThread) {
            //		    actorThread.interrupt();
            //	    }
            //	    try {
            //		    GameScene.class.wait(5000);
            //	    } catch (InterruptedException e) {
            //		    ShatteredPixelDungeon.reportException(e);
            //	    }
            //	    synchronized (actorThread) {
            //		    if (Actor.processing()) {
            //			    Throwable t = new Throwable();
            //			    t.setStackTrace(actorThread.getStackTrace());
            //			    throw new RuntimeException("timeout waiting for actor thread! ", t);
            //		    }
            //	    }
            //    }
            //}
            Emitter.freezeEmitters = false;

            scene = null;
            BadgesExtensions.SaveGlobal();
            Journal.SaveGlobal();

            base.Destroy();
        }

        public static void EndActorThread()
        {
            //if (actorThread != null && actorThread.isAlive())
            //{
            //    Actor.keepActorThreadAlive = false;
            //    actorThread.interrupt();
            //}
        }

        public override void OnPause()
        {
            try
            {
                Dungeon.SaveAll();
                BadgesExtensions.SaveGlobal();
                Journal.SaveGlobal();
            }
            catch (IOException e)
            {
                ShatteredPixelDungeonDash.ReportException(e);
            }
        }

        //private static Thread actorThread;

        //sometimes UI changes can be prompted by the actor thread.
        // We queue any removed element destruction, rather than destroying them in the actor thread.
        private List<Gizmo> toDestroy = new List<Gizmo>();

        public override void Update()
        {
            if (Dungeon.hero == null || scene == null)
                return;

            base.Update();

            if (!Emitter.freezeEmitters)
                water.Offset(0, -5 * Game.elapsed);

            if (!Actor.Processing() && Dungeon.hero.IsAlive())
            {
                if (Actor.ProcessStarted() == false)
                {
                    Actor.StartProcess();
                }
                else
                {
                    Actor.InputNext();
                }
            }

            Actor.Process();

            counter.SetSweep((1f - Actor.Now() % 1f) % 1f);

            if (Dungeon.hero.ready && Dungeon.hero.paralysed == 0)
            {
                log.NewLine();
            }

            if (tagAttack != attack.active ||
                tagLoot != loot.visible ||
                tagAction != action.visible ||
                tagResume != resume.visible)
            {
                //we only want to change the layout when new tags pop in, not when existing ones leave.
                bool tagAppearing = (attack.active && !tagAttack) ||
                                        (loot.visible && !tagLoot) ||
                                        (action.visible && !tagAction) ||
                                        (resume.visible && !tagResume);

                tagAttack = attack.active;
                tagLoot = loot.visible;
                tagAction = action.visible;
                tagResume = resume.visible;

                if (tagAppearing)
                    LayoutTags();
            }

            cellSelector.Enable(Dungeon.hero.ready);

            foreach (Gizmo g in toDestroy)
            {
                g.Destroy();
            }
            toDestroy.Clear();
        }

        private bool tagAttack;
        private bool tagLoot;
        private bool tagAction;
        private bool tagResume;

        public static void LayoutTags()
        {
            if (scene == null)
                return;

            float tagLeft = SPDSettings.FlipTags() ? 0 : uiCamera.width - scene.attack.Width();

            if (SPDSettings.FlipTags())
            {
                scene.log.SetRect(scene.attack.Width(), scene.toolbar.Top() - 2, uiCamera.width - scene.attack.Width(), 0);
            }
            else
            {
                scene.log.SetRect(0, scene.toolbar.Top() - 2, uiCamera.width - scene.attack.Width(), 0);
            }

            float pos = scene.toolbar.Top();

            if (scene.tagAttack)
            {
                scene.attack.SetPos(tagLeft, pos - scene.attack.Height());
                scene.attack.Flip(tagLeft == 0);
                pos = scene.attack.Top();
            }

            if (scene.tagLoot)
            {
                scene.loot.SetPos(tagLeft, pos - scene.loot.Height());
                scene.loot.Flip(tagLeft == 0);
                pos = scene.loot.Top();
            }

            if (scene.tagAction)
            {
                scene.action.SetPos(tagLeft, pos - scene.action.Height());
                scene.action.Flip(tagLeft == 0);
                pos = scene.action.Top();
            }

            if (scene.tagResume)
            {
                scene.resume.SetPos(tagLeft, pos - scene.resume.Height());
                scene.resume.Flip(tagLeft == 0);
            }
        }

        public override void OnBackPressed()
        {
            if (!Cancel())
                Add(new WndGame());
        }

        public void AddCustomTile(CustomTilemap visual)
        {
            customTiles.Add(visual.Create());
        }

        public void AddCustomWall(CustomTilemap visual)
        {
            customWalls.Add(visual.Create());
        }

        private void AddHeapSprite(Heap heap)
        {
            var sprite = heap.sprite = heaps.Recycle<ItemSprite>();
            sprite.Revive();
            sprite.Link(heap);
            heaps.Add(sprite);
        }

        private void AddDiscardedSprite(Heap heap)
        {
            heap.sprite = heaps.Recycle<DiscardedItemSprite>();
            heap.sprite.Revive();
            heap.sprite.Link(heap);
            heaps.Add(heap.sprite);
        }

        //private void AddPlantSprite(Plant plant)
        //{ }
        //
        //private void AddTrapSprite(Trap trap)
        //{ }

        private void AddBlobSprite(Blob gas)
        {
            if (gas.emitter == null)
                gases.Add(new BlobEmitter(gas));
        }

        private void AddMobSprite(Mob mob)
        {
            //CharSprite sprite = mob.sprite();
            var sprite = mob.GetSprite();
            sprite.visible = Dungeon.level.heroFOV[mob.pos];
            mobs.Add(sprite);
            sprite.Link(mob);
        }

        private void Prompt(string text)
        {
            if (prompt != null)
            {
                prompt.KillAndErase();
                toDestroy.Add(prompt);
                prompt = null;
            }

            if (text == null)
                return;

            prompt = new Toast(text);
            prompt.closeAction = () => Cancel();
            prompt.camera = uiCamera;
            prompt.SetPos((uiCamera.width - prompt.Width()) / 2, uiCamera.height - 60);
            Add(prompt);
        }

        private void ShowBanner(Banner banner)
        {
            banner.camera = uiCamera;
            banner.x = Align(uiCamera, (uiCamera.width - banner.width) / 2);
            banner.y = Align(uiCamera, (uiCamera.height - banner.height) / 3);
            AddToFront(banner);
        }

        // -------------------------------------------------------

        //public static void Add(Plant plant)
        //{
        //    if (scene != null)
        //        scene.AddPlantSprite(plant);
        //}
        //
        //public static void Add(Trap trap)
        //{
        //    if (scene != null)
        //        scene.AddTrapSprite(trap);
        //}

        public static void Add(Blob gas)
        {
            Actor.Add(gas);
            if (scene != null)
                scene.AddBlobSprite(gas);
        }

        public static void Add(Heap heap)
        {
            if (scene != null)
                scene.AddHeapSprite(heap);
        }

        public static void Discard(Heap heap)
        {
            if (scene != null)
                scene.AddDiscardedSprite(heap);
        }

        public static void Add(Mob mob)
        {
            Dungeon.level.mobs.Add(mob);
            scene.AddMobSprite(mob);
            Actor.Add(mob);
        }

        public static void AddSprite(Mob mob)
        {
            scene.AddMobSprite(mob);
        }

        public static void Add(Mob mob, float delay)
        {
            Dungeon.level.mobs.Add(mob);
            scene.AddMobSprite(mob);
            Actor.AddDelayed(mob, delay);
        }

        public static void Add(EmoIcon icon)
        {
            scene.emoicons.Add(icon);
        }

        public static void Add(CharHealthIndicator indicator)
        {
            if (scene != null)
                scene.healthIndicators.Add(indicator);
        }

        public static void Add(CustomTilemap t, bool wall)
        {
            if (scene == null)
                return;

            if (wall)
            {
                scene.AddCustomWall(t);
            }
            else
            {
                scene.AddCustomTile(t);
            }
        }

        public static void Effect(Visual effect)
        {
            scene.effects.Add(effect);
        }

        public static void EffectOverFog(Visual effect)
        {
            scene.overFogEffects.Add(effect);
        }

        public static Ripple Ripple(int pos)
        {
            if (scene != null)
            {
                var ripple = scene.ripples.Recycle<Ripple>();
                ripple.Reset(pos);
                return ripple;
            }
            else
            {
                return null;
            }
        }

        public static SpellSprite SpellSprite()
        {
            return scene.spells.Recycle<SpellSprite>();
        }

        //public static Emitter Emitter()
        public static Emitter GetEmitter()
        {
            if (scene == null)
                return null;

            var emitter = scene.emitters.Recycle<Emitter>();
            emitter.Revive();
            return emitter;
        }

        public static Emitter FloorEmitter()
        {
            if (scene == null)
                return null;

            var emitter = scene.floorEmitters.Recycle<Emitter>();
            emitter.Revive();
            return emitter;
        }

        public static FloatingText Status()
        {
            return scene != null ? scene.statuses.Recycle<FloatingText>() : null;
        }

        public static void PickUp(Item item, int pos)
        {
            scene.toolbar.Pickup(item, pos);
        }

        public static void PickUpJournal(Item item, int pos)
        {
            if (scene != null)
                scene.pane.Pickup(item, pos);
        }

        public static void FlashJournal()
        {
            if (scene != null)
                scene.pane.Flash();
        }

        public static void UpdateKeyDisplay()
        {
            if (scene != null)
                scene.pane.UpdateKeys();
        }

        public static void ResetMap()
        {
            if (scene != null)
            {
                scene.tiles.Map(Dungeon.level.map, Dungeon.level.Width());
                scene.visualGrid.Map(Dungeon.level.map, Dungeon.level.Width());
                scene.terrainFeatures.Map(Dungeon.level.map, Dungeon.level.Width());
                scene.raisedTerrain.Map(Dungeon.level.map, Dungeon.level.Width());
                scene.walls.Map(Dungeon.level.map, Dungeon.level.Width());
            }
            UpdateFog();
        }

        //updates the whole map
        public static void UpdateMap()
        {
            if (scene != null)
            {
                scene.tiles.UpdateMap();
                scene.visualGrid.UpdateMap();
                scene.terrainFeatures.UpdateMap();
                scene.raisedTerrain.UpdateMap();
                scene.walls.UpdateMap();
                UpdateFog();
            }
        }

        public static void UpdateMap(int cell)
        {
            if (scene != null)
            {
                scene.tiles.UpdateMapCell(cell);
                scene.visualGrid.UpdateMapCell(cell);
                scene.terrainFeatures.UpdateMapCell(cell);
                scene.raisedTerrain.UpdateMapCell(cell);
                scene.walls.UpdateMapCell(cell);
                //update adjacent cells too
                UpdateFog(cell, 1);
            }
        }

        public static void PlantSeed(int cell)
        {
            if (scene != null)
            {
                scene.terrainFeatures.GrowPlant(cell);
            }
        }

        //todo this doesn't account for walls right now
        public static void DiscoverTile(int pos, int oldValue)
        {
            if (scene != null)
                scene.tiles.Discover(pos, oldValue);
        }

        public static void Show(Window wnd)
        {
            if (scene != null)
            {
                CancelCellSelector();
                scene.AddToFront(wnd);
            }
        }

        public static void UpdateFog()
        {
            if (scene != null)
            {
                scene.fog.UpdateFog();
                scene.wallBlocking.UpdateMap();
            }
        }

        public static void UpdateFog(int x, int y, int w, int h)
        {
            if (scene != null)
            {
                scene.fog.UpdateFogArea(x, y, w, h);
                scene.wallBlocking.UpdateArea(x, y, w, h);
            }
        }

        public static void UpdateFog(int cell, int radius)
        {
            if (scene != null)
            {
                scene.fog.UpdateFog(cell, radius);
                scene.wallBlocking.UpdateArea(cell, radius);
            }
        }

        public static void AfterObserve()
        {
            if (scene == null)
                return;

            foreach (var mob in Dungeon.level.mobs)
            {
                if (mob.sprite != null)
                    mob.sprite.visible = Dungeon.level.heroFOV[mob.pos];
            }
        }

        public static void Flash(Color color)
        {
            Flash(color, true);
        }

        public static void Flash(Color color, bool lightmode)
        {
            color.A = 0xFF;
            scene.FadeIn(color, lightmode);
        }

        public static void GameOver()
        {
            Banner gameOver = new Banner(BannerSprites.Get(BannerSprites.Type.GAME_OVER));
            gameOver.Show(new Color(0x00, 0x00, 0x00, 0xFF), 1f);
            scene.ShowBanner(gameOver);

            Sample.Instance.Play(Assets.Sounds.DEATH);
        }

        public static void BossSlain()
        {
            if (!Dungeon.hero.IsAlive())
                return;

            Banner bossSlain = new Banner(BannerSprites.Get(BannerSprites.Type.BOSS_SLAIN));
            bossSlain.Show(new Color(0xFF, 0xFF, 0xFF, 0xFF), 0.3f, 5f);
            scene.ShowBanner(bossSlain);

            Sample.Instance.Play(Assets.Sounds.BOSS);
        }

        public static void HandleCell(int cell)
        {
            cellSelector.Select(cell);
        }

        public static void SelectCell(CellSelector.IListener listener)
        {
            if (cellSelector.listener != null && cellSelector.listener != defaultCellListener)
            {
                cellSelector.listener.OnSelect(null);
            }

            cellSelector.listener = listener;
            if (scene != null)
                scene.Prompt(listener.Prompt());
        }

        private static bool CancelCellSelector()
        {
            cellSelector.ResetKeyHold();
            if (cellSelector.listener != null && cellSelector.listener != defaultCellListener)
            {
                cellSelector.Cancel();
                return true;
            }
            else
            {
                return false;
            }
        }

        public static WndBag SelectItem(WndBag.IListener listener, WndBag.Mode mode, string title)
        {
            CancelCellSelector();

            WndBag wnd =
                mode == WndBag.Mode.SEED ?
                    WndBag.GetBag<VelvetPouch>(listener, mode, title) :
                mode == WndBag.Mode.SCROLL ?
                    WndBag.GetBag<ScrollHolder>(listener, mode, title) :
                mode == WndBag.Mode.POTION ?
                    WndBag.GetBag<PotionBandolier>(listener, mode, title) :
                mode == WndBag.Mode.WAND ?
                    WndBag.GetBag<MagicalHolster>(listener, mode, title) :
                WndBag.LastBag(listener, mode, title);

            if (scene != null)
                scene.AddToFront(wnd);

            return wnd;
        }

        public static bool Cancel()
        {
            if (Dungeon.hero != null && (Dungeon.hero.curAction != null || Dungeon.hero.resting))
            {
                Dungeon.hero.curAction = null;
                Dungeon.hero.resting = false;
                return true;
            }
            else
            {
                return CancelCellSelector();
            }
        }

        public static void Ready()
        {
            SelectCell(defaultCellListener);
            QuickSlotButton.Cancel();
            if (scene != null && scene.toolbar != null)
                scene.toolbar.examining = false;
        }

        public static void CheckKeyHold()
        {
            cellSelector.ProcessKeyHold();
        }

        public static void ResetKeyHold()
        {
            cellSelector.ResetKeyHold();
        }

        public static void ExamineCell(int? c)
        {
            if (c == null)
                return;
            int cell = c.Value;

            if (cell < 0 ||
                cell > Dungeon.level.Length() ||
                (!Dungeon.level.visited[cell] && !Dungeon.level.mapped[cell]))
            {
                return;
            }

            List<string> names = new List<string>();
            List<object> objects = new List<object>();

            if (cell == Dungeon.hero.pos)
            {
                objects.Add(Dungeon.hero);
                names.Add(Dungeon.hero.ClassName().ToUpperInvariant());
            }
            else
            {
                if (Dungeon.level.heroFOV[cell])
                {
                    Mob mob = (Mob)Actor.FindChar(cell);
                    if (mob != null)
                    {
                        objects.Add(mob);
                        names.Add(Messages.TitleCase(mob.Name()));
                    }
                }
            }

            Heap heap = Dungeon.level.heaps[cell];
            if (heap != null && heap.seen)
            {
                objects.Add(heap);
                names.Add(Messages.TitleCase(heap.ToString()));
            }

            Plant plant = Dungeon.level.plants[cell];
            if (plant != null)
            {
                objects.Add(plant);
                names.Add(Messages.TitleCase(plant.plantName));
            }

            Trap trap = Dungeon.level.traps[cell];
            if (trap != null && trap.visible)
            {
                objects.Add(trap);
                names.Add(Messages.TitleCase(trap.name));
            }

            if (objects.Count == 0)
            {
                GameScene.Show(new WndInfoCell(cell));
            }
            else if (objects.Count == 1)
            {
                ExamineObject(objects[0]);
            }
            else
            {
                var wnd = new WndOptions(
                    Messages.Get(typeof(GameScene), "choose_examine"),
                    Messages.Get(typeof(GameScene), "multiple_examine"),
                    names.ToArray());

                wnd.selectAction = (index) =>
                {
                    ExamineObject(objects[index]);
                };

                GameScene.Show(wnd);
            }
        }

        public static void ExamineObject(object o)
        {
            if (o == Dungeon.hero)
                GameScene.Show(new WndHero());
            else if (o is Mob)
                GameScene.Show(new WndInfoMob((Mob)o));
            else if (o is Heap)
                GameScene.Show(new WndInfoItem((Heap)o));
            else if (o is Plant)
                GameScene.Show(new WndInfoPlant((Plant)o));
            else if (o is Trap)
                GameScene.Show(new WndInfoTrap((Trap)o));
            else
                GameScene.Show(new WndMessage(Messages.Get(typeof(GameScene), "dont_know")));
        }

        private static DefaultCellListener defaultCellListener = new DefaultCellListener();

        private class DefaultCellListener : CellSelector.IListener
        {
            public void OnSelect(int? cell)
            {
                if (Dungeon.hero.Handle(cell))
                {
                    Dungeon.hero.Next();
                }
            }

            public string Prompt()
            {
                return null;
            }
        }
    }
}