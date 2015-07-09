using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    // including Enemy and Player, used for spawning Laser/EnemyLaser for Player/Enemies
    public interface IFlier
    {
        Vector2 GetPosition();
        Vector2 GetGunOffset();
    }

    // used for lists in IOHighscore()
    public class MyEntry
    {
        public int Score { get; set; }
        public string Information { get; set; }
    }

    public class ShooterGame : Game
    {
        const int NUMBER_OF_ENEMY_TYPES = 4;  // How many enemies are there?

        GraphicsDeviceManager graphics;       
        SpriteBatch spriteBatch;

        Player player;                        // The Player

        Texture2D t2d_mainbg;                 // Background texture

        KeyboardState currentKeyboardState;   // Keyboard controlling objects
        KeyboardState previousKeyboardState;

        Texture2D t2d_laserTexture;           // Shot texture
        List<Laser> list_lasers;              // List of all player's lasers

        TimeSpan TS_laserRate;                // Rate of player's laser spawn
        TimeSpan TS_laserPrevious;            // When was the previous shot fired

        Texture2D t2d_enemyLaserTexture;      // Enemy shot texture
        List<Laser> list_enemiesLasers;       // List of all enemies' lasers

        TimeSpan TS_enemyLaserRate;           // Rate of enemies' laser spawn
        TimeSpan TS_bosslaserRate;            // Rate of bosses' laser spawn, different because boss gets tougher

        Texture2D[] t2dar_enemies;            // Enemy textures
        List<Enemy> list_enemies;             // List of all the enemies

        TimeSpan TS_enemyRate;                // Rate of enemies spawning
        TimeSpan TS_enemyPrevious;            // When did the last enemy spawn

        List<Explosion> list_explos;          // Explosion art
        Texture2D t2d_exploTexture;           // ALL the explosions!

        Random random;
        SpriteFont font_smallCalibri;         // Font sprites
        SpriteFont font_bigCalibri;           

        TimeSpan TS_buildingRate;             // Rate of buildings spawning
        TimeSpan TS_buildingPrevious;         // When did the last building spawn

        bool boss;                            // Is a boss out right now?
        TimeSpan TS_bossTimeout;              // The lengthof no-enemies spawned period after a boss kill
        TimeSpan TS_bossDeath;                // When did the boss die?

        TimeSpan TS_levelStart;               // When did the level start?
        TimeSpan TS_levelLength;              // How long should the level be?
        int i_levelCount;                     // Which level are we in now?
        TimeSpan TS_levelSpawnPenalty;        // Penalty that is making enemies spawn faster
        float BOSS_RATE_OF_FIRE;              // How fast the boss fires (getting higher with progressing levels)

        TimeSpan TS_lastRestart = TimeSpan.FromSeconds(0);

        // default MonoGame "Game" class setup
        public ShooterGame()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        // called by MonoGame before any Updates(), and when 'R' is pressed for restarting the game
        // used for initializing all the variables and objects to the beginning of the game
        protected override void Initialize()
        {
            player = new Player();

            graphics.PreferredBackBufferWidth = 1280;   //   width of the window
            graphics.PreferredBackBufferHeight = 720;   //  height of the window 
            graphics.ApplyChanges();
            
            list_enemies = new List<Enemy>();
            list_enemiesLasers = new List<Laser>();

            TS_enemyPrevious = TimeSpan.Zero;
            TS_enemyRate = TimeSpan.FromSeconds(1f); // enemies start to spawn one per second
            
            random = new Random();

            // create laser list, adjust spawn rate of lasers
            list_lasers = new List<Laser>();
            const float RATE_OF_FIRE = 200f;
            const float ENEMY_RATE_OF_FIRE = 10f;
            BOSS_RATE_OF_FIRE = 25f;

            // spawn span is (#SECONDS_IN_MINUTE / #RATE_OF_FIRE)
            TS_laserRate = TimeSpan.FromSeconds(60f / RATE_OF_FIRE); 
            TS_laserPrevious = TimeSpan.Zero;

            TS_enemyLaserRate = TimeSpan.FromSeconds(60f / ENEMY_RATE_OF_FIRE);
            TS_bosslaserRate = TimeSpan.FromSeconds(60f / BOSS_RATE_OF_FIRE);

            TS_buildingPrevious = TimeSpan.Zero;
            TS_buildingRate = TimeSpan.FromSeconds(3f);

            TS_bossTimeout = TimeSpan.FromSeconds(2f);

            list_explos = new List<Explosion>();

            // Level 1 starts now, length of level is LEVEL_LENGTH seconds
            float LEVEL_LENGTH = 25f;
            TS_levelLength = TimeSpan.FromSeconds(LEVEL_LENGTH);     // length
            TS_levelStart = TimeSpan.FromSeconds(0f);                // start of level 1
            i_levelCount = 1;                                        // it is level 1
            TS_levelSpawnPenalty = TimeSpan.FromSeconds(0.3f);

            boss = false;

            base.Initialize();
        }

        // called by MonoGame each gameTime clock tick to update the game objects,
        // consisting of smaller methods, see below
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            UpdatePlayer(gameTime);
            UpdateEnemies(gameTime);   
            UpdateLasers(gameTime);
            UpdateCollision();
            UpdateExplosions(gameTime);

            base.Update(gameTime);
        }

        // called by MonoGame for Drawing objects
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);  // Clear graphics (with blue)
            
            spriteBatch.Begin();
            
            spriteBatch.Draw(t2d_mainbg, new Vector2(0, 0), Color.White); // draw the BG
            
            // UI
            string str_topLeftText = "";
            str_topLeftText += "Level " + i_levelCount  + "\nHealth: " + player.i_playerHealth.ToString() + "\nScore: " + player.i_playerScore.ToString();
            spriteBatch.DrawString(font_smallCalibri, str_topLeftText, new Vector2(10, 10), Color.White);

            // Player, we draw him if he lives, draw the "HighScore & GameOver screen" if he is dead
            if (player.i_playerHealth > 0)
            {
                player.Draw(spriteBatch);
            }
            else
            {
                HighScoreDraw(); // HighScore & GameOver screen
            }

            // Enemies
            for (int i = 0; i < list_enemies.Count; i++)
            {
                list_enemies[i].Draw(spriteBatch);
            }

            // Player's Lasers
            foreach (var l in list_lasers)
            {
                l.Draw(spriteBatch);
            }
            // Enemies' Lasers
            foreach (var l in list_enemiesLasers)
            {
                l.Draw(spriteBatch);
            }

            // draw explosions
            foreach (var e in list_explos)
            {
                e.Draw(spriteBatch);
            }       

            spriteBatch.End(); 

            base.Draw(gameTime);
        }
        // helper method to make Draw() more readable
        private void HighScoreDraw()
        {
            string str_gameover = "Game Over\nYour final score is: " + player.i_playerScore;
            spriteBatch.DrawString(font_bigCalibri, str_gameover, new Vector2(240, 90), Color.White);

            string str_col1 = String.Format("{0,11}", "Scores:") + "\n";
            string str_col2 = String.Format("{0,7}", "Level:") + "\n";
            string str_col3 = String.Format("{0,10}", "Date:") + "\n";
            string line;
            if (File.Exists("highscore.atf"))
            {
                using (StreamReader strR = new StreamReader("highscore.atf"))
                {
                    while ((line = strR.ReadLine()) != null)
                    {
                        string[] lines = line.Split(' ');
                        str_col1 += String.Format("{0,10}", lines[0]) + "\n";
                        str_col2 += String.Format("{0,6}", lines[1]) + "\n";
                        str_col3 += String.Format("{0,10}", lines[2]) + "\n";
                    }
                }
            }
            spriteBatch.DrawString(font_smallCalibri, str_col1, new Vector2(240, 340), Color.White);
            spriteBatch.DrawString(font_smallCalibri, str_col2, new Vector2(340, 340), Color.White);
            spriteBatch.DrawString(font_smallCalibri, str_col3, new Vector2(410, 340), Color.White);

            spriteBatch.DrawString(font_smallCalibri, "Press \'R\' to restart the game.\nPress \'Esc\' to quit the game.", new Vector2(610, 340), Color.White);
        }

        // (Un)Loads content into the Game
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X,
                GraphicsDevice.Viewport.TitleSafeArea.Y + GraphicsDevice.Viewport.TitleSafeArea.Height / 2);

            Animation playerAnimation = new Animation();
            t2d_mainbg = Content.Load<Texture2D>("bg2");
            Texture2D t2d_playerTexture = Content.Load<Texture2D>("shipAnimation");

            t2dar_enemies = new Texture2D[NUMBER_OF_ENEMY_TYPES];
            t2dar_enemies[0] = Content.Load<Texture2D>("EnemyAnimation");
            t2dar_enemies[1] = Content.Load<Texture2D>("EnemyAnimation2");
            t2dar_enemies[2] = Content.Load<Texture2D>("Budova2");
            t2dar_enemies[3] = Content.Load<Texture2D>("boss");

            t2d_laserTexture = Content.Load<Texture2D>("laser");
            t2d_enemyLaserTexture = Content.Load<Texture2D>("Enemylaser");

            t2d_exploTexture = Content.Load<Texture2D>("explosion");

            font_smallCalibri = Content.Load<SpriteFont>("smallfont");
            font_bigCalibri = Content.Load<SpriteFont>("bigfont");

            playerAnimation.Initialize(t2d_playerTexture, Vector2.Zero, 115, 69, 8, 50, Color.White, 1f, true);
            player.Initialize(playerAnimation, playerPosition, 7.5f);
        }
        protected override void UnloadContent()
        {
        }

        /* ^------------------------------  MonoGame's Methods  --------------------------------^ */

        // Handling KeyBoard INPUT, updates the player object
        private void UpdatePlayer(GameTime gameTime)
        {
            player.Update(gameTime);
            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentKeyboardState.IsKeyDown(Keys.A))
            {
                player.v2_playerPosition.X -= player.f_playerSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentKeyboardState.IsKeyDown(Keys.D))
            {
                player.v2_playerPosition.X += player.f_playerSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentKeyboardState.IsKeyDown(Keys.W))
            {
                player.v2_playerPosition.Y -= player.f_playerSpeed;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentKeyboardState.IsKeyDown(Keys.S))
            {
                player.v2_playerPosition.Y += player.f_playerSpeed;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Space))
            {
                FireLaser(gameTime, ref TS_laserPrevious, TS_laserRate, player);
            }
            if (currentKeyboardState.IsKeyDown(Keys.R)) // restart the game
            {
                // You can only restart after a second has passed,
                // if this wasn't here, each press of 'R' button restarts about 3x,
                // because the game tick is faster than the press of a button.
                TimeSpan fromStart = TimeSpan.FromSeconds(1);
                if (fromStart < gameTime.TotalGameTime - TS_lastRestart)
                {
                    TS_lastRestart = gameTime.TotalGameTime;
                    Initialize();
                }            
            }

            //  Clamp player's position if player is active
            if (player.b_playerActive)
            {
                player.v2_playerPosition.X = MathHelper.Clamp(player.v2_playerPosition.X, player.Width / 2, GraphicsDevice.Viewport.Width - player.Width / 2);
                player.v2_playerPosition.Y = MathHelper.Clamp(player.v2_playerPosition.Y, player.Height / 2, GraphicsDevice.Viewport.Height - player.Height / 2);
            }
        }

        /* Collision
        * check enemies against lasers,
        *  player against enemies' bodies,
        *  player against enemies' lasers   */
        private void UpdateCollision()
        {
            Rectangle rect_playerBox;
            rect_playerBox = new Rectangle((int)player.v2_playerPosition.X-6, (int)player.v2_playerPosition.Y-6, player.Width-16, player.Height-6);
        
            Rectangle[] rect_enemyBoxList = new Rectangle[list_enemies.Count];
            Rectangle[] rect_laserBoxList = new Rectangle[list_lasers.Count];

            // Make collision boxes for enemies
            for (int i = 0; i < list_enemies.Count; i++)
            {
                rect_enemyBoxList[i] = new Rectangle((int)list_enemies[i].v2_enemyPosition.X + 10, (int)list_enemies[i].v2_enemyPosition.Y-13,
                    list_enemies[i].Width-5, list_enemies[i].Height-10);
            }
            // Make collision boxes for lasers
            for (int i = 0; i < list_lasers.Count; i++)
            {
                rect_laserBoxList[i] = new Rectangle((int)list_lasers[i].v2_laserPosition.X, (int)list_lasers[i].v2_laserPosition.Y,
                    list_lasers[i].Width, list_lasers[i].Height);
            }

            // Collision of Player's lasers with enemies
            for (int i = 0; i < list_enemies.Count; i++)
            {
                for (int l = 0; l < list_lasers.Count; l++)
                {
                    if (rect_enemyBoxList[i].Intersects(rect_laserBoxList[l]))
                    {
                        if (!(list_enemies[i] is BuildingEnemy)) // buildings do not take damage
                        {
                            list_enemies[i].i_enemyHealth -= list_lasers[l].i_laserDamage;
                        }
                        list_lasers[l].b_laserActive = false;    // laser gets deactivated                    
                    }
                    if (list_enemies[i].i_enemyHealth <= 0) // enemy is dead
                    {
                        AddExplosion(list_enemies[i].v2_enemyPosition);
                        list_enemies[i].b_enemyActive = false;
                        player.i_playerScore += list_enemies[i].GetEnemyValue();
                    }
                }
                if (player.b_playerActive) // check collision with player only if he lives
                {
                    // if a player crashed into an ACTIVE enemy (which could have been killed before)
                    if (rect_playerBox.Intersects(rect_enemyBoxList[i]) && list_enemies[i].b_enemyActive)
                    {
                        if (!(list_enemies[i] is BuildingEnemy) && !(list_enemies[i] is BossEnemy))  // buildings and bosses don't die on impact
                        {
                            player.i_playerHealth -= list_enemies[i].GetEnemyDamage(); // player takes damage
                            player.i_playerScore += list_enemies[i].GetEnemyValue();   // and gets a lot of score

                            list_enemies[i].b_enemyActive = false; // enemy is deactivated
                            
                            AddExplosion(list_enemies[i].v2_enemyPosition);
                        }
                        else // buildings and bosses kill player instantly
                        {
                            player.i_playerHealth = 0;
                        }
                    }
                    if ((player.i_playerHealth <= 0)) // Player is dead, set his position to away and speed to zero
                    {
                        PlayerDead();
                    }
                }
            }

            // Collision of enemies' lasers with player
            if (player.b_playerActive)
            {
                rect_laserBoxList = new Rectangle[list_enemiesLasers.Count];
                // Make collision boxes for lasers
                for (int i = 0; i < list_enemiesLasers.Count; i++)
                {
                    rect_laserBoxList[i] = new Rectangle((int)list_enemiesLasers[i].v2_laserPosition.X, (int)list_enemiesLasers[i].v2_laserPosition.Y,
                        list_enemiesLasers[i].Width, list_enemiesLasers[i].Height);
                }

                // Collision
                for (int i = 0; i < list_enemiesLasers.Count; i++)
                {
                    if (rect_laserBoxList[i].Intersects(rect_playerBox))
                    {
                        player.i_playerHealth -= list_enemiesLasers[i].i_laserDamage;
                        list_enemiesLasers[i].b_laserActive = false;
                    }

                    // Player is dead, set his position to away and speed to zero
                    if ((player.i_playerHealth <= 0)) 
                    {
                        PlayerDead();
                    }
                }
            }
        }
        // Helper method to make UpdateCollision() more readable
        private void PlayerDead()
        {
            // explode!
            AddExplosion(player.v2_playerPosition);

            // we move the player out of the way
            player.b_playerActive = false;
            player.ani_playerAnimation.Active = false;
            player.v2_playerPosition = new Vector2(-500, -500);
            player.f_playerSpeed = 0f;

            IOHighscore(i_levelCount, player.i_playerScore);    // save the score
            boss = true;                                        // kind of a hack, this stops the enemies spawning over the score screen

            player.i_playerHealth = 0; // we do not want to see '-25 health'
        }

        // Enemies' methods
        // Spawns a enemy of a type given by the number 'whichEnemy'
        private void AddEnemy(int whichEnemy)
        {
            // Instance
            Enemy enemy;
            int loc_width, loc_height, loc_frameCount, loc_frameTime, loc_X, loc_Y; // local variables, for easier animation initialize

            // Where do enemies spawn by default 
            loc_X = GraphicsDevice.Viewport.Width + t2dar_enemies[whichEnemy].Width / 2;
            loc_Y = random.Next(100, GraphicsDevice.Viewport.Height - 140);

            // Setting the enemies' specific information
            switch (whichEnemy)
            {
                case 0:
                    enemy = new Enemy();
                    loc_width = 47; loc_height = 61;
                    loc_frameCount = 8; loc_frameTime = 30;
                    break;
                case 1:
                    enemy = new TopDownEnemy();
                    loc_width = 47; loc_height = 61;
                    loc_frameCount = 8; loc_frameTime = 30;
                    break;
                case 2:
                    enemy = new BuildingEnemy();
                    loc_width = 175; loc_height = 140;
                    loc_frameCount = 1; loc_frameTime = 500;
                    loc_Y = GraphicsDevice.Viewport.Height - t2dar_enemies[whichEnemy].Height/2;
                    break;
                case 3:
                    enemy = new BossEnemy();
                    loc_width = 239; loc_height = 161;
                    loc_frameCount = 1; loc_frameTime = 500;
                    loc_Y = (GraphicsDevice.Viewport.Height + t2dar_enemies[whichEnemy].Height) / 2;
                    break;
                default: // redundant, it's here just because C# wants it
                    enemy = new Enemy();
                    loc_width = 0; loc_height = 0;
                    loc_frameCount = 0; loc_frameTime = 0;
                    Console.WriteLine("ERROR AT SWITCH IN AddEnemy(whichenemy)");
                    break;
            }

            // Create enemy animation
            Animation enemyAnimation = new Animation();
            enemyAnimation.Initialize(t2dar_enemies[whichEnemy], Vector2.Zero, loc_width, loc_height, loc_frameCount, loc_frameTime, Color.White, 1f, true);

            Vector2 position = new Vector2(loc_X, loc_Y);

            // Initiate the enemy
            enemy.Initialize(enemyAnimation, position, GraphicsDevice.Viewport.Height);

            // Remember the enemy
            list_enemies.Add(enemy);
        }
        // Returns 0/1 if enemy can spawn (his previous spawn is further than enemy spawnrate)
        private bool SpawnDeterminate(ref TimeSpan prev, GameTime gameTime, TimeSpan spawnrate)
        {
            if (gameTime.TotalGameTime - prev > spawnrate)
            {
                prev = gameTime.TotalGameTime;
                return true;
            }
            return false;
        }
        // Handles spawning of enemies, bosses, removing the deactivated enemies
        private void UpdateEnemies(GameTime gameTime)
        {
            // If we don't have a boss
            if (!boss)
            {
                // AND we don't have timeout after boss kill, we spawn enemies
                if (gameTime.TotalGameTime - TS_bossDeath > TS_bossTimeout)
                {
                    // if it is time, spawn an enemy
                    if (SpawnDeterminate(ref TS_enemyPrevious, gameTime, TS_enemyRate)) { AddEnemy(random.Next(0, 2)); }

                    // spawn building?
                    if (SpawnDeterminate(ref TS_buildingPrevious, gameTime, TS_buildingRate))
                    {
                        if (random.Next(0, 99) < 70) { AddEnemy(2); } // add a bit of random factor
                    }
                }

                // if a boss should appear, we spawn him and stop enemies by 'true'ing the boolean 'boss' variable
                if ((gameTime.TotalGameTime - TS_levelStart > TS_levelLength))
                {
                    boss = true;
                    AddEnemy(3);
                }
            }

            // Update the Enemies
            for (int i = list_enemies.Count - 1; i >= 0; i--)
            {
                list_enemies[i].Update(gameTime); // All enemies update (moving)
              
                // if enemy isnt a building and is VISIBLE to the player, he tries to shoot
                if (!(list_enemies[i] is BuildingEnemy) && (list_enemies[i].v2_enemyPosition.X + list_enemies[i].Width <= 1280))
                {
                    // rate of fire depends on the type - bosses are faster
                    TimeSpan laserRate = TS_enemyLaserRate;
                    if (list_enemies[i] is BossEnemy) { laserRate = TS_bosslaserRate; }
                    
                    // FireLaser() fires the laser(s), handles the boss shooting 3 instead of 1
                    FireLaser(gameTime, ref list_enemies[i].TS_prevShot, laserRate, list_enemies[i]); 
                }    
       
                // if enemy isn't active, he gets removed
                if (list_enemies[i].b_enemyActive == false)
                {
                    if (list_enemies[i] is BossEnemy) // boss is dead, the Timeout starts to run
                    {
                        boss = false;
                        TS_bossDeath = gameTime.TotalGameTime;

                        // Another level starts, bosses get harder, enemies are more present
                        TS_levelStart = gameTime.TotalGameTime + TS_bossTimeout;
                        i_levelCount++;

                        // enemies spawn quicker
                        TS_enemyRate -= TS_levelSpawnPenalty;
                        TS_levelSpawnPenalty = TimeSpan.FromSeconds(TS_levelSpawnPenalty.TotalSeconds / 1.75f);

                        // boss shoots faster
                        BOSS_RATE_OF_FIRE += 25f;
                        TS_bosslaserRate = TimeSpan.FromSeconds(60f / BOSS_RATE_OF_FIRE);
                    }
                    list_enemies.RemoveAt(i);
                }
            }
        }

        // Laser's methods
        // Fires a laser if the laser rate of 'flier' allows it, if 'flier' is a Boss, he fires twice more
        protected void FireLaser(GameTime gameTime, ref TimeSpan prev, TimeSpan rate, IFlier flier)
        {
            // if we CAN fire a laser, we fire it
            if (gameTime.TotalGameTime - prev > rate)
            {
                prev = gameTime.TotalGameTime;
                AddLaser(flier,0f); // see below the process of spawning a laser

                // Bosses are badasses and shoot three shots, at an angle
                if (flier is BossEnemy)
                {
                    AddLaser(flier, 3f);
                    AddLaser(flier, -3f);
                }
            }
        }
        // Handles the spawn of the laser and adding it to the active lasers lists
        protected void AddLaser(IFlier flier, float shotDirection)
        {
            // declaration of locals variables
            Texture2D loc_texture;
            Vector2 loc_flierPosition;
            Vector2 loc_laserPostion;
            Laser laser;

            // we set the laser position to the position of the flier
            loc_flierPosition = flier.GetPosition();
            loc_laserPostion = loc_flierPosition;

            // each flier has it's offset to make the laser come out of the gun
            Vector2 loc_laserFlier = flier.GetGunOffset();
            loc_laserPostion += loc_laserFlier; // we adjust the laser position with the offset

            // we spawn the laser, standard laser for the player
            // boss-specific is handled in the FireLaser(), a bit of player tracking for the enemies
            if ((flier is Player))
            {
                loc_texture = t2d_laserTexture;
                laser = new Laser();
            }
            else if ((flier is BossEnemy))
            {
                loc_texture = t2d_enemyLaserTexture;
                laser = new EnemyLaser(shotDirection);
            }
            else if ((flier is Enemy))
            {
                loc_texture = t2d_enemyLaserTexture;

                // Enemies track the player's position and adjust aim very slightly
                float a = 0f;
                if (player.v2_playerPosition.Y < loc_flierPosition.Y) { a = -2f; }
                else if (player.v2_playerPosition.Y > loc_flierPosition.Y) { a = 2f; }

                laser = new EnemyLaser(a);
            }
            else { // DEFAULT because C# knows what is good better than us  :-)
                loc_texture = t2d_laserTexture;
                laser = new Laser();
            }

            Animation laserAnimation = new Animation();

            // initlize the laser animation
            laserAnimation.Initialize(loc_texture, loc_flierPosition, 46, 16, 1, 30,
                Color.White, 1f, true);

            // init the laser
            laser.Initialize(laserAnimation, loc_laserPostion);

            // add the laser to the proper list
            if(laser is EnemyLaser) {
                list_enemiesLasers.Add(laser);
            }
            else {
                list_lasers.Add(laser);
            }            
        }
        // Updates all the lasers active right now and deletes the nonactive lasers
        protected void UpdateLasers(GameTime gameTime)
        {
            // update Player's laserbeams
            for (int i = 0; i < list_lasers.Count; i++)
            {
                list_lasers[i].Update(gameTime);

                // Remove the beam when its deactivated or is at the end of the screen.
                if (!list_lasers[i].b_laserActive || list_lasers[i].v2_laserPosition.X >= GraphicsDevice.Viewport.Width)
                {
                    list_lasers.RemoveAt(i);
                }
            }

            // update enemies' lasers
            for (int i = 0; i < list_enemiesLasers.Count; i++)
            {
                list_enemiesLasers[i].Update(gameTime);

                // Remove the beam when its deactivated or is at the end of the screen.
                if (!list_enemiesLasers[i].b_laserActive || list_enemiesLasers[i].v2_laserPosition.X < -list_enemiesLasers[i].Width)
                {
                    list_enemiesLasers.RemoveAt(i); 
                }
            }
        }

        // Explosions' methods
        // Spawns the explosion and initializes
        protected void AddExplosion(Vector2 enemyPosition)
        {
            Animation explosionAnimation = new Animation();

            explosionAnimation.Initialize(t2d_exploTexture, enemyPosition, 134, 134, 12, 30,
                Color.White, 1.0f, true);

            Explosion explosion = new Explosion();
            explosion.Initialize(explosionAnimation, enemyPosition);

            list_explos.Add(explosion);
        }
        // Update explosions, remove the not active ones
        private void UpdateExplosions(GameTime gameTime)
        {
            for (int e = 0; e < list_explos.Count; e++)
            {
                list_explos[e].Update(gameTime);

                if (!list_explos[e].b_exploActive)
                    list_explos.Remove(list_explos[e]);
            }
        }

        // Saves & updates the TOP10 highscores in a file 'highscores.atf'
        protected void IOHighscore(int par_reachedLevel, int par_score)
        {
            List<MyEntry> list_scores = new List<MyEntry>();
            string[] output = new string[10];

            // Get our line ready
            DateTime now = DateTime.Now;
            string str_now = now.ToString();
            string[] stra_now = str_now.Split(' ');

            string out_string = par_reachedLevel + " " + stra_now[0] + stra_now[1] + stra_now[2];

            int i_count;  // used as a iterator
            string str_line;

            // If the file exists, we read it
            if (File.Exists("highscore.atf"))
            {
                // Read the lines, save them
                using (StreamReader str_Read = new StreamReader("highscore.atf"))
                {
                    while ((str_line = str_Read.ReadLine()) != null)
                    {
                        string[] items = str_line.Split(' ');

                        int key_score = Convert.ToInt32(items[0]);
                        string value_levelDate = items[1] + " " + items[2];
                        list_scores.Add(new MyEntry() { Score = key_score, Information = value_levelDate });
                    }
                }
                // Add our new score
                list_scores.Add(new MyEntry() { Score = par_score, Information = out_string });

                // Sort all the scores
                list_scores.Sort((PosA, PosB) => PosB.Score.CompareTo(PosA.Score));
                i_count = 0;

                // Get the "top 10" scores
                foreach (var item in list_scores)
                {
                    output[i_count] = item.Score.ToString() + " " + item.Information;
                    if (i_count == 9)
                    {
                        break;
                    }
                    i_count++;
                }
            }
            else // if we have only our score (the file doesn't exist), we simply add it as the only one
            {
                i_count = 1;
                output[0] = par_score.ToString() + " " + out_string;
            }

            // We write the result 
            StreamWriter str_Write = new StreamWriter("highscore.atf");          
            for (int i = 0; i < i_count+1; i++)
            {                
                str_Write.Write(output[i]);
                if(i != i_count) // we do not want to add a newline at the last line
                {
                    str_Write.WriteLine();
                }
            }
            str_Write.Close();
        }
    }
}
