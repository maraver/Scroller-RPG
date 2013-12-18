using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RPG.Sprites;
using RPG.GameObjects;
using RPG.Helpers;
using RPG.Entities;
using RPG.Items;
using RPG.Tiles;

using Microsoft.VisualBasic;

namespace RPG.Screen
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GameScreen : Screen
    {
        public static int TRANSITION_MS = 500;
        public static Random RANDOM = new Random();

        // -------------------
        // Game Variables
        // -------------------
        public Player Player;
        public TileMap TileMap { get; private set; }
        Point offset;

        int transitionMs;

        KeyboardState oldKb;
        // -------------------
        // Game Textures
        // -------------------
        public static Dictionary<GUISpriteId, Texture2D> sprGUI;
        public static Dictionary<EntitySpriteId, Sprite> sprEntities;
        public static Dictionary<AttackSpriteId, Texture2D> sprAttacks, sprAttacks_Icons;    
        public static Dictionary<TerrainSpriteId, Texture2D> sprTerrains;
        public static Dictionary<Animation, Sprite> Animations;

        public static Dictionary<ItemId, Armour> Armours;
        public static Dictionary<ItemId, Item> Items;
        
        Texture2D sprBackground;

        public GameScreen(ScreenManager screenManager) : base(screenManager) {
            offset = new Point(0, 0);
            oldKb = Keyboard.GetState();
        }

        public override void LoadContent() {
            loadEntities();
            loadAttacks();

            // Load Terrain
            Texture2D TerrainSpriteSheet = Content.Load<Texture2D>("Terrain/Terrain");
            Texture2D[] TerrainTexs = new Texture2D[TerrainSpriteSheet.Width / TileMap.SPRITE_SIZE];
            for (int i=0; i<TerrainTexs.Length; i++) TerrainTexs[i] = SpriteParser.ParseSpriteSheet(TerrainSpriteSheet, i, TileMap.SPRITE_SIZE);
            sprTerrains = new Dictionary<TerrainSpriteId, Texture2D>();
            sprTerrains.Add(TerrainSpriteId.None, null);
            sprTerrains.Add(TerrainSpriteId.Stone_Wall, TerrainTexs[0]);
            sprTerrains.Add(TerrainSpriteId.Stone2_Wall, TerrainTexs[1]);
            sprTerrains.Add(TerrainSpriteId.Door, TerrainTexs[2]);
            sprTerrains.Add(TerrainSpriteId.EmptyMagicWall, TerrainTexs[7]);
            sprTerrains.Add(TerrainSpriteId.ClosedChest, TerrainTexs[8]);
            sprTerrains.Add(TerrainSpriteId.OpenChest, TerrainTexs[9]);
            sprTerrains.Add(TerrainSpriteId.IronDoor, TerrainTexs[10]);
            sprTerrains.Add(TerrainSpriteId.Stairs, TerrainTexs[11]);
            sprTerrains.Add(TerrainSpriteId.Stairs_Flip, SpriteParser.Flip(TerrainTexs[11]));

            Animations = new Dictionary<Animation,Sprite>();
            Animations.Add(Animation.RedSpiral, new Sprite(new Texture2D[] {TerrainTexs[3],TerrainTexs[4],TerrainTexs[5],TerrainTexs[6]}, 4, 100));         

            // Load GUI
            sprGUI = new Dictionary<GUISpriteId, Texture2D>();
            sprGUI.Add(GUISpriteId.Blocking, Content.Load<Texture2D>("GUI/Blocking"));
            sprGUI.Add(GUISpriteId.Ducking, Content.Load<Texture2D>("GUI/Crouching"));
            sprGUI.Add(GUISpriteId.Standing, Content.Load<Texture2D>("GUI/Standing"));

            sprBackground = Content.Load<Texture2D>("Terrain/cave1_background");

            // Load Items
            Texture2D ItemSpriteSheet = Content.Load<Texture2D>("Items/Items");
            Texture2D[] ItemTexs = new Texture2D[ItemSpriteSheet.Width / Item.SIZE];
            for (int i=0; i<ItemTexs.Length; i++) ItemTexs[i] = SpriteParser.ParseSpriteSheet(ItemSpriteSheet, i, Item.SIZE);
            Items = new Dictionary<ItemId, Item>();
            Items.Add(ItemId.None, null);
            Items.Add(ItemId.Key, new Item(ItemId.Key, "Door Key", ItemTexs[0], ItemEvents.NewRoom));
            Items.Add(ItemId.SmallPotion, new Item(ItemId.SmallPotion, "Small Potion", ItemTexs[1], ItemEvents.UseSmallPotion));
            Items.Add(ItemId.Gold, new Item(ItemId.Gold, "Gold", ItemTexs[2], ItemEvents.NoAction, true));

            // Load Armour
            Texture2D ArmourSpriteSheet = Content.Load<Texture2D>("Armour/Armour");
            Texture2D[,] ArmourTexs = new Texture2D[ArmourSpriteSheet.Height / TileMap.SPRITE_SIZE, ArmourSpriteSheet.Width / TileMap.SPRITE_SIZE];
            for (int h = 0; h < ArmourTexs.GetLength(0); h++)
                for (int w = 0; w < ArmourTexs.GetLength(1); w++)
                    ArmourTexs[h, w] = SpriteParser.ParseSpriteSheet(ArmourSpriteSheet, w, h, TileMap.SPRITE_SIZE);
            Armours = new Dictionary<ItemId, Armour>();
            Armours.Add(ItemId.BronzeHead, new Armour(ArmourTexs[0, 0], ArmourTexs[0, 3], "Bronze Helm", ItemId.BronzeHead, ArmourParts.Head, 0.075f, true));
            Armours.Add(ItemId.BronzeBody, new Armour(ArmourTexs[0, 1], ArmourTexs[0, 4], "Bronze Body", ItemId.BronzeBody, ArmourParts.Body, 0.075f, true));
            Armours.Add(ItemId.BronzeLegs, new Armour(ArmourTexs[0, 2], ArmourTexs[0, 5], "Bronze Legs", ItemId.BronzeLegs, ArmourParts.Legs, 0.075f, true));
            Armours.Add(ItemId.IronHead, new Armour(ArmourTexs[1, 0], ArmourTexs[1, 3], "Iron Helm", ItemId.IronHead, ArmourParts.Head, 0.150f, true));
            Armours.Add(ItemId.IronBody, new Armour(ArmourTexs[1, 1], ArmourTexs[1, 4], "Iron Body", ItemId.IronBody, ArmourParts.Body, 0.150f, true));
            Armours.Add(ItemId.IronLegs, new Armour(ArmourTexs[1, 2], ArmourTexs[1, 5], "Iron Legs", ItemId.IronLegs, ArmourParts.Legs, 0.150f, true));
        }

        private void loadEntities() {
            sprEntities = new Dictionary<EntitySpriteId, Sprite>();
            sprEntities.Add(EntitySpriteId.Warrior, new Sprite(Content, "Warrior").loadSpriteParts(SpriteParts.Entity));
            sprEntities.Add(EntitySpriteId.Warlock, new Sprite(Content, "Warlock").loadSpriteParts(SpriteParts.Entity));
            sprEntities.Add(EntitySpriteId.Wraith, new Sprite(Content, "Wraith").loadSpriteParts(SpriteParts.Entity));
            sprEntities.Add(EntitySpriteId.SkeletonKing, new Sprite(Content, "Skeleton_King").loadSpriteParts(SpriteParts.Entity));
        }

        private void loadAttacks() {
            sprAttacks = new Dictionary<AttackSpriteId, Texture2D>();
            sprAttacks_Icons = new Dictionary<AttackSpriteId,Texture2D>();
            loadSpell(AttackSpriteId.Fireball, "Fireball/Fireball");
            loadSpell(AttackSpriteId.Iceball, "Iceball/Iceball");
            loadSpell(AttackSpriteId.Scurge_Shot, "Scurge_Shot/Scurge_Shot");
            loadSpell(AttackSpriteId.Raise_Death, "Raise_Death/Raise_Death");
        }

        private void loadSpell(AttackSpriteId id, String name) {
            sprAttacks.Add(id, Content.Load<Texture2D>(name));
            sprAttacks_Icons.Add(id, Content.Load<Texture2D>(name + "_Icon"));
        }

        public override void UnloadContent() { }

        // Initialize the various game screens and create the player with the inputed name
        public string init(string name) {
            Player = new Player(this, 0, 3 * TileMap.SPRITE_SIZE, name, sprEntities[EntitySpriteId.Warrior]);
            Player.newMainRoom();

            return name;
        }

        public void setRoom(TileMap map) {
            if (TileMap != null) TileMap.LeavePoint = Player.Location;

            TileMap = map;

            // Run a few ticks
            for(int i=0; i<20; i++) {
                TileMap.update(ScreenManager.TargElapsedTime);
            }

            Player.moveToNewMap(map);
            TileMap.addEntity(Player);
            transitionMs = TRANSITION_MS;
        }

        public void startOver() {
            Player = new Player(this, 0, 0, Player.Name, SprEntity[EntitySpriteId.Warrior]);
            Player.newMainRoom();
        }

        public override void Update(GameTime time) {
            // Don't run until player is created
            if (Player == null) {
                return;
            }

            // Don't do anything while transitioning
            if (transitionMs > 0) {
                transitionMs -= time.ElapsedGameTime.Milliseconds;
                return;
            }

            // ### Movement input
            if (ScreenManager.kbState.IsKeyDown(Keys.Right) && !ScreenManager.kbState.IsKeyDown(Keys.Left))
                Player.doMove(Direction.Right);
            else if (!ScreenManager.kbState.IsKeyDown(Keys.Right) && ScreenManager.kbState.IsKeyDown(Keys.Left))
                Player.doMove(Direction.Left);
            else
                Player.doMove(Direction.Stopped);

            // ### Jump/Duck input
            if (ScreenManager.kbState.IsKeyDown(Keys.Space))
                Player.doJump();
            else if (!ScreenManager.kbState.IsKeyDown(Keys.Space) && ScreenManager.kbState.IsKeyDown(Keys.Down) && !ScreenManager.kbState.IsKeyDown(Keys.Up))
                Player.doDuck();
            else if (!ScreenManager.kbState.IsKeyDown(Keys.Space) && !ScreenManager.kbState.IsKeyDown(Keys.Down) && ScreenManager.kbState.IsKeyDown(Keys.Up))
                Player.doBlock();
            else if (Player.State == EntityState.Crouching || Player.State == EntityState.Blocking)
                Player.stand();

            // ### Attack input
            if (ScreenManager.kbState.IsKeyDown(Keys.Q))
                Player.doAttack(TileMap, EntityPart.Head);
            else if (ScreenManager.kbState.IsKeyDown(Keys.W))
                Player.doAttack(TileMap, EntityPart.Body);
            else if (ScreenManager.kbState.IsKeyDown(Keys.E))
                Player.doAttack(TileMap, EntityPart.Legs);

            // ### Hotbat input
            if (ScreenManager.kbState.IsKeyDown(Keys.D1)) HotBar.select(0);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D2)) HotBar.select(1);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D3)) HotBar.select(2);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D4)) HotBar.select(3);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D5)) HotBar.select(4);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D6)) HotBar.select(5);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D7)) HotBar.select(6);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D8)) HotBar.select(7);
            else if (ScreenManager.kbState.IsKeyDown(Keys.D9)) HotBar.select(8);
            HotBar.update(time.ElapsedGameTime);

            // Interact with tile block
            if (ScreenManager.kbState.IsKeyDown(Keys.Enter) && ScreenManager.oldKBState.IsKeyUp(Keys.Enter)) {
                Rectangle rect = Player.EBounds.Rect;
                TileMap.getPixel(rect.Center.X, rect.Center.Y).interact(this);
            }

            // Update tilemap
            TileMap.update(time.ElapsedGameTime);           

            // ### Offset
            offset.X = (int) (Player.Location.X - getScreenManager().Width / 2);
            if (offset.X < 0) 
                offset.X = 0;
            else if (offset.X + getScreenManager().Width > TileMap.getPixelWidth()) 
                offset.X = (int) (TileMap.getPixelWidth() - getScreenManager().Width);
            offset.X *= -1; // Invert

            offset.Y = (int) (Player.Location.Y - getScreenManager().Height / 2);
            if (offset.Y < 0)
                offset.Y = 0;
            else if (offset.Y + getScreenManager().Height > TileMap.getPixelHeight())
                offset.Y = (int) (TileMap.getPixelHeight() - getScreenManager().Height);
            offset.Y *= -1; // Invert

            oldKb = ScreenManager.kbState;
        }
        
        public override void Draw(GameTime time) {
            // While transitioning all black
            if (transitionMs > 0) {
                SpriteBatch.GraphicsDevice.Clear(Color.Black);
                return;
            }

            int bgW = sprBackground.Width;
            int bgH = TileMap.getPixelHeight();

            int bgX = offset.X % (bgW * 2);
            int bgY = offset.Y % (bgH * 2);

            SpriteBatch.Draw(sprBackground, new Rectangle(bgX, bgY, bgW, bgH), Color.White);

            // # Draw a second one in front of first to wrap around
            bgX += bgW;
            SpriteBatch.Draw(sprBackground, new Rectangle(bgX, bgY, bgW, bgH), Color.White);    

            // Draw map
            TileMap.draw(SpriteBatch, offset, time.ElapsedGameTime);

            // Draw hotbar
            HotBar.draw(SpriteBatch);
        }

        private HotBar HotBar { get { return Player.HotBar; } }

        public Dictionary<EntitySpriteId, Sprite> SprEntity { get { return sprEntities; } }
        public Dictionary<AttackSpriteId, Texture2D> SprAttack { get { return sprAttacks; } }
        public Dictionary<TerrainSpriteId, Texture2D> SprTerrains { get { return sprTerrains; } }
    }
}
