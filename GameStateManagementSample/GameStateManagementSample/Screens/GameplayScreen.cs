#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using GameStateManagement;
using ChaseCameraSample;
using BloomPostprocess;
using Particles.Particles;
using System.Collections.Generic;
using ShapeRenderingSample;
#endregion

namespace GameStateManagementSample
{
    // NOTE TO DEVELOPERS
    // BOTTOM VIEWPORT USES CAMERA and ship
    // TOP VIEWPORT USES CAMERA2 and ship2

    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        BloomComponent bloom;
        
        int bloomSettingsIndex = 0;

        KeyboardState lastKeyboardState = new KeyboardState();
        GamePadState lastGamePadState1 = new GamePadState();
        GamePadState lastGamePadState2 = new GamePadState();
        MouseState lastMousState = new MouseState();
        KeyboardState currentKeyboardState = new KeyboardState();
        GamePadState currentGamePadState = new GamePadState();
        MouseState currentMouseState = new MouseState();


        Random random = new Random();

        float pauseAlpha;

        InputAction pauseAction;

        Ship ship,ship2;
        ChaseCamera camera,camera2;
        Viewport topViewport, bottomViewport;

        EnvironmentMapEffect envEffect1;
        EnvironmentMapEffect envEffect2;
        TextureCube textureCube1;
        TextureCube textureCube2;

        Vector3 ship1Pos, ship2Pos;
        Vector3[] rockPos = new Vector3[800];
        int[] rockScale = new int[800];
        int[] rockRotation = new int[800];

        //AUDIO STUFF
        AudioEmitter shipEmit1, shipEmit2;
        AudioListener shipListen1, shipListen2;
        Cue FxCue;
        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;
        AudioCategory acSFX;
        AudioCategory acMusic;
        bool musicPlaying = false;

        Model rockModel;
        Model skyBoxModel;
        Model shipModel,shipModel2;
        Model groundModel;
        Model cubeModel;
        Model bulletModel;
        Model indicatorModel;
        
        bool cameraSpringEnabled = true;
        bool camera2SpringEnabled = true;

        Texture2D TLifeBarFrame, TLifeBarBar;

        //ParticleEffect particleEffect;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            pauseAction = new InputAction(
                new Buttons[] { Buttons.Start, Buttons.Back },
                new Keys[] { Keys.Escape },
                true);

            shipEmit1 = new AudioEmitter();
            shipEmit2 = new AudioEmitter();

            // Create the chase camera
            camera = new ChaseCamera();
            camera2 = new ChaseCamera();

            shipListen1 = new AudioListener();
            shipListen1.Position = camera.Position;
            // Set the camera offsets
            camera.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
            camera.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

            // Set camera perspective
            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 10000000.0f;

            // Set the camera offsets
            camera2.DesiredPositionOffset = new Vector3(0.0f, 2000.0f, 3500.0f);
            camera2.LookAtOffset = new Vector3(0.0f, 150.0f, 0.0f);

            // Set camera perspective
            camera2.NearPlaneDistance = 10.0f;
            camera2.FarPlaneDistance = 10000000.0f;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void Activate(bool instancePreserved)
        {
            if (!instancePreserved)
            {
                if (content == null)
                    content = new ContentManager(ScreenManager.Game.Services, "Content");

                DebugShapeRenderer.Initialize(ScreenManager.GraphicsDevice);

                topViewport = ScreenManager.GraphicsDevice.Viewport;
                bottomViewport = ScreenManager.GraphicsDevice.Viewport;

                if (ScreenManager.ScreenHorizontal == true)
                {
                    topViewport.Height = topViewport.Height / 2;
                    bottomViewport.Height = bottomViewport.Height / 2;
                    bottomViewport.Y = topViewport.Height;

                    camera.AspectRatio = (float)ScreenManager.GraphicsDevice.Viewport.Width /
                        (ScreenManager.GraphicsDevice.Viewport.Height / 2);
                    camera2.AspectRatio = (float)ScreenManager.GraphicsDevice.Viewport.Width /
                        (ScreenManager.GraphicsDevice.Viewport.Height / 2);

                    camera.FieldOfView = MathHelper.ToRadians(45);
                    camera2.FieldOfView = MathHelper.ToRadians(45);
                }
                else
                {
                    topViewport.Width = topViewport.Width / 2;
                    bottomViewport.Width = bottomViewport.Width / 2;
                    bottomViewport.X = topViewport.Width;

                    camera.AspectRatio = (float)(ScreenManager.GraphicsDevice.Viewport.Width / 2) /
                        ScreenManager.GraphicsDevice.Viewport.Height;
                    camera2.AspectRatio = (float)(ScreenManager.GraphicsDevice.Viewport.Width / 2) /
                        ScreenManager.GraphicsDevice.Viewport.Height;

                    camera.FieldOfView = MathHelper.ToRadians(60);
                    camera2.FieldOfView = MathHelper.ToRadians(60);
                }

                gameFont = content.Load<SpriteFont>("gamefont");
                rockModel = content.Load<Model>("Rock");
                shipModel = content.Load<Model>("SpaceShip1");
                shipModel2 = content.Load<Model>("SpaceShip2");
                cubeModel = content.Load<Model>("cube");
                bulletModel = content.Load<Model>("Cone");
                skyBoxModel = content.Load<Model>("Space_SkyBox");
                indicatorModel = content.Load<Model>("Cone");
                TLifeBarBar = content.Load<Texture2D>("LifeBarBar");
                TLifeBarFrame = content.Load<Texture2D>("LifeBarFrame");

                audioEngine = ScreenManager.AudioEngine;
                soundBank = ScreenManager.SoundBank;
                waveBank = ScreenManager.WaveBank;
                acSFX = audioEngine.GetCategory("SFX");
                acMusic = audioEngine.GetCategory("Music");

                // Environmental Map Effect for Player 1
                envEffect1 = new EnvironmentMapEffect(ScreenManager.GraphicsDevice);
                envEffect1.Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, ScreenManager.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100.0f);
                envEffect1.View = Matrix.CreateLookAt(
                    new Vector3(2, 3, 32), Vector3.Zero, Vector3.Up);
                textureCube1 = new TextureCube(ScreenManager.GraphicsDevice, 256, false, SurfaceFormat.Color);
                Color[] facedata1 = new Color[256 * 256];
                for (int i = 0; i < 6; i++)
                {
                    envEffect1.Texture = content.Load<Texture2D>("skybox" + i.ToString());
                    envEffect1.Texture.GetData<Color>(facedata1);
                    textureCube1.SetData<Color>((CubeMapFace)i, facedata1);
                }
                envEffect1.Texture = (shipModel.Meshes[0].Effects[0] as EnvironmentMapEffect).Texture;
                envEffect1.EnvironmentMap = textureCube1;
                envEffect1.EnableDefaultLighting();
                envEffect1.EnvironmentMapAmount = 1.0f;
                envEffect1.FresnelFactor = 1.0f;
                envEffect1.EnvironmentMapSpecular = Vector3.Zero;

                Texture2D smokeTex = content.Load<Texture2D>("Smoke");

                //PARTICLE EFFECT
                //particleEffect = new ParticleEffect(100, 1000, 1, 50.0f);
                //List<Texture2D> textures = new List<Texture2D> { content.Load<Texture2D>("Smoke") };
                //particleEffect.LoadContent(textures, ScreenManager.GraphicsDevice);

                // Environmental Map Effect for Player 2
                envEffect2 = new EnvironmentMapEffect(ScreenManager.GraphicsDevice);
                envEffect2.Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4, ScreenManager.GraphicsDevice.Viewport.AspectRatio, 1.0f, 100.0f);
                envEffect2.View = Matrix.CreateLookAt(
                    new Vector3(2, 3, 32), Vector3.Zero, Vector3.Up);
                textureCube2 = new TextureCube(ScreenManager.GraphicsDevice, 256, false, SurfaceFormat.Color);
                Color[] facedata2 = new Color[256 * 256];
                for (int i = 0; i < 6; i++)
                {
                    envEffect2.Texture = content.Load<Texture2D>("skybox" + i.ToString());
                    envEffect2.Texture.GetData<Color>(facedata2);
                    textureCube2.SetData<Color>((CubeMapFace)i, facedata2);
                }
                envEffect2.Texture = (shipModel2.Meshes[0].Effects[0] as EnvironmentMapEffect).Texture;
                envEffect2.EnvironmentMap = textureCube2;
                envEffect2.EnableDefaultLighting();
                envEffect2.EnvironmentMapAmount = 1.0f;
                envEffect2.FresnelFactor = 1.0f;
                envEffect2.EnvironmentMapSpecular = Vector3.Zero;

                //audioEngine = ScreenManager.AudioEngine;
                //soundBank = ScreenManager.SoundBank;
                //waveBank = ScreenManager.WaveBank;

                if (ScreenManager.AudioEnabled == true)
                {
                    acSFX.SetVolume(ScreenManager.SFXVolume);
                    acMusic.SetVolume(ScreenManager.AudioVolume);
                }
                else
                {
                    acSFX.SetVolume(0);
                    acMusic.SetVolume(0);
                }

                FxCue = soundBank.GetCue("ShotFx");
                ScreenManager.MusicCue = soundBank.GetCue("GameMusic");

                ship1Pos = new Vector3(10000, 350, 10000);
                ship2Pos = new Vector3(-10000, 350, 10000);

                // Create ship
                ship = new Ship(ScreenManager.GraphicsDevice,ship1Pos,soundBank,shipEmit1,shipEmit2,shipListen1);
                ship2 = new Ship(ScreenManager.GraphicsDevice,ship2Pos,soundBank,shipEmit1,shipEmit2,shipListen1);
                //ship2.Position = new Vector3(100, 100, 100);

                UpdateCameraChaseTarget(ship,camera);
                UpdateCameraChaseTarget(ship2,camera2);

                camera.Reset();
                camera2.Reset();

                // Spawn rocks around the map
                // Will not spawn within 500 pixels around players' ships
                RandomRockSpawner();

                // Enable Bloom
                // Orlando Bloom
                bloom = new BloomComponent(ScreenManager.Game);
                ScreenManager.Game.Components.Add(bloom);

                // Modify bloom
                bloom.Settings = BloomSettings.PresetSettings[1];
                bloom.Visible = true;

                // A real game would probably have more content than this sample, so
                // it would take longer to load. We simulate that by delaying for a
                // while, giving you a chance to admire the beautiful loading screen.
                Thread.Sleep(1000);

                // once the load has finished, we use ResetElapsedTime to tell the game's
                // timing mechanism that we have just finished a very long frame, and that
                // it should not try to catch up.
                ScreenManager.Game.ResetElapsedTime();
            }

#if WINDOWS_PHONE
            if (Microsoft.Phone.Shell.PhoneApplicationService.Current.State.ContainsKey("PlayerPosition"))
            {
                playerPosition = (Vector2)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"];
                enemyPosition = (Vector2)Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"];
            }
#endif
        }

        public override void Deactivate()
        {
#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["PlayerPosition"] = playerPosition;
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State["EnemyPosition"] = enemyPosition;
#endif

            base.Deactivate();
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void Unload()
        {
            content.Unload();

            if (ScreenManager.ScreenHorizontal == true)
            {
                topViewport.Height *= 2;
                topViewport.Y = 0;
                ScreenManager.GraphicsDevice.Viewport = topViewport;
            }
            else
            {
                topViewport.Width *= 2;
                topViewport.X = 0;
                ScreenManager.GraphicsDevice.Viewport = topViewport;
            }

#if WINDOWS_PHONE
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("PlayerPosition");
            Microsoft.Phone.Shell.PhoneApplicationService.Current.State.Remove("EnemyPosition");
#endif
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                    lastKeyboardState = currentKeyboardState;
                    //lastGamePadState = currentGamePadState;
                    lastMousState = currentMouseState;

#if WINDOWS_PHONE
            currentKeyboardState = new KeyboardState();
#else
                    currentKeyboardState = Keyboard.GetState();
#endif
                    shipListen1.Position = camera.Position;
                    shipEmit1.Position = ship.Position;
                    shipEmit2.Position = ship2.Position;

                for (PlayerIndex p = PlayerIndex.One; p <= PlayerIndex.Two; p++)
                {
                    switch(p)
                    {
                        case PlayerIndex.One: lastGamePadState2 = currentGamePadState;
                            break;
                        case PlayerIndex.Two: lastGamePadState1 = currentGamePadState;
                            break;
                    }

                    currentGamePadState = GamePad.GetState(p);
                    currentMouseState = Mouse.GetState();

                    bool touchTopLeft = currentMouseState.LeftButton == ButtonState.Pressed &&
                            lastMousState.LeftButton != ButtonState.Pressed &&
                            currentMouseState.X < ScreenManager.GraphicsDevice.Viewport.Width / 10 &&
                            currentMouseState.Y < ScreenManager.GraphicsDevice.Viewport.Height / 10;

                    

#if Windows
                    // Pressing the A button or key toggles the spring behavior on and off
                    if (lastKeyboardState.IsKeyUp(Keys.A) &&
                        (currentKeyboardState.IsKeyDown(Keys.A)) ||
                        (lastGamePadState1.Buttons.A == ButtonState.Released &&
                        currentGamePadState.Buttons.A == ButtonState.Pressed) ||
                        touchTopLeft)
                    {
                        switch(p)
                        {
                            case PlayerIndex.One: camera2SpringEnabled = !camera2SpringEnabled;
                                break;
                            case PlayerIndex.Two: cameraSpringEnabled = !cameraSpringEnabled;
                                break;
                        }
                    }

                    lastGamePadState1 = currentGamePadState;
#else
                    switch(p)
                    {
                        case PlayerIndex.One:
                        if (lastGamePadState1.Buttons.A == ButtonState.Released &&
                            currentGamePadState.Buttons.A == ButtonState.Pressed)
                        {
                            camera2SpringEnabled = !camera2SpringEnabled;
                        }
                        break;
                        case PlayerIndex.Two:
                        if (lastGamePadState2.Buttons.A == ButtonState.Released &&
                            currentGamePadState.Buttons.A == ButtonState.Pressed)
                        {
                            cameraSpringEnabled = !cameraSpringEnabled;
                        }
                        break;
                    }
#endif
                }
                /*
                particleEffect.Emit(gameTime, ship2Pos);
                particleEffect.Update(gameTime);
                */
                //ship.shipHealth = 0;
                if (!ScreenManager.MusicCue.IsPlaying)
                {
                    musicPlaying = false;
                    ScreenManager.MusicCue.Stop(AudioStopOptions.Immediate);
                }
                if (!musicPlaying)
                {
                    ScreenManager.MusicCue = soundBank.GetCue("GameMusic");
                    ScreenManager.MusicCue.Play();
                    //ScreenManager.AudioEngine.Update();
                    musicPlaying = true;
                }

                // Reset the ship on R key or right thumb stick clicked
                //if (currentKeyboardState.IsKeyDown(Keys.R) ||
                //    currentGamePadState.Buttons.RightStick == ButtonState.Pressed)
                //{
                //    ship.Reset(ship1Pos);
                //    camera.Reset();
                //}

                // Update the ship
                ship.Update(gameTime, shipModel, cubeModel,bulletModel,ship2.World,2);
                ship2.Update(gameTime, shipModel, cubeModel, bulletModel, ship.World,1);

                AsteroidCollision(ship,ship2);
                AsteroidCollision(ship2,ship);
                for(int i = 0;i<ship.bullets.Length;i++)
                {
                    if (ship.bullets[i].isAlive)
                    {
                        BulletCollision(ship.bullets[i]);
                    }
                    if (ship2.bullets[i].isAlive)
                    {
                        BulletCollision(ship2.bullets[i]);
                    }
                }
                // Update the camera to chase the new target
                UpdateCameraChaseTarget(ship,camera);
                UpdateCameraChaseTarget(ship2,camera2);

                // The chase camera's update behavior is the springs, but we can
                // use the Reset method to have a locked, spring-less camera
                if (cameraSpringEnabled)
                    camera.Update(gameTime);
                else
                    camera.Reset();

                if (camera2SpringEnabled)
                    camera2.Update(gameTime);
                else
                    camera2.Reset();

                //One of the ships dies
                if (ship.shipHealth <= 0 || ship2.shipHealth <= 0)
                {
                    ScreenManager.MusicCue.Stop(AudioStopOptions.Immediate);
                    ScreenManager.AudioEngine.Update();
                    ScreenManager.MainMenu.Stop(AudioStopOptions.Immediate);
                    ScreenManager.MainMenu = ScreenManager.SoundBank.GetCue("WinMusic");
                    ScreenManager.MainMenu.Play();
                    ScreenManager.AddScreen(new GameOverScreen(ScreenManager.SoundBank), PlayerIndex.One);

                    GamePad.SetVibration(PlayerIndex.Two, 0.0f, 0.0f);
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
                }

            }
            ScreenManager.AudioEngine.Update();
        }

        public void AsteroidCollision(Ship s,Ship otherS)
        {
            for (int i = 0; i < shipModel.Meshes.Count; i++)
            {
                BoundingSphere shipBoundingSphere = shipModel.Meshes[i].BoundingSphere;
                shipBoundingSphere.Center += s.Position;
                shipBoundingSphere.Radius *= 12.0f;

                DebugShapeRenderer.AddBoundingSphere(shipBoundingSphere, Color.Red);

                for (int j = 0; j < rockPos.Length; j++)
                {
                    if (Vector3.Distance(rockPos[j], s.Position) <= 50000)
                    {
                        //for (int k = 0; k < rockModel.Meshes.Count; k++)
                        //{
                            BoundingSphere rockBoundingSphere = rockModel.Meshes[0].BoundingSphere;
                            rockBoundingSphere.Center += rockPos[j];
                            rockBoundingSphere.Radius *= 2.0f*rockScale[j];

                            DebugShapeRenderer.AddBoundingSphere(rockBoundingSphere, Color.Yellow);

                            if (shipBoundingSphere.Intersects(rockBoundingSphere))
                            {
                                //s.ReverseVelocity();
                                s.BackUp();
                                s.ReverseVelocity();
                                otherS.shipHealth -= 5;
                                soundBank.GetCue("Boing").Play();
                            }
                        //}
                    }
                }

            }

            ////BoundingBox bb = new BoundingBox(new Vector3(s.Position.X - 650, s.Position.Y - 200, s.Position.Z - 1200), new Vector3(s.Position.X + 650, s.Position.Y + 200, s.Position.Z + 700));
            //BoundingSphere shipBS = shipModel.Meshes[0].BoundingSphere;
            //shipBS.Center = s.Position;
            //shipBS.Radius *= 15.0f;

            ////DebugShapeRenderer.AddBoundingBox(bb, Color.Red);
            //DebugShapeRenderer.AddBoundingSphere(shipBS, Color.Red);

            //for (int r = 0; r < rockPos.Length; r++)
            //{
            //    BoundingSphere bs = rockModel.Meshes[0].BoundingSphere;
            //    bs.Center = rockPos[r];
            //    bs.Radius *= 150.0f;

            //    if(Vector3.Distance(rockPos[r], s.Position) <= 50000)
            //        DebugShapeRenderer.AddBoundingSphere(bs, Color.Yellow);

            //    if (shipBS.Intersects(bs))
            //    {
            //        s.ReverseVelocity();
            //        s.BackUp();
            //        s.ReverseVelocity();
            //        return;
            //    }
            //}
        }
        public void BulletCollision(ShipBullet Sb)
        {
            for (int i = 0; i < bulletModel.Meshes.Count; i++)
            {
                BoundingSphere bulletBoundingSphere = bulletModel.Meshes[i].BoundingSphere;
                bulletBoundingSphere.Center += Sb.Position;
                bulletBoundingSphere.Radius *= 12.0f;

                DebugShapeRenderer.AddBoundingSphere(bulletBoundingSphere, Color.Pink);

                for (int j = 0; j < rockPos.Length; j++)
                {
                    if (Vector3.Distance(rockPos[j], Sb.Position) <= 50000)
                    {
                        //for (int k = 0; k < rockModel.Meshes.Count; k++)
                        //{
                        BoundingSphere rockBoundingSphere = rockModel.Meshes[0].BoundingSphere;
                        rockBoundingSphere.Center += rockPos[j];
                        rockBoundingSphere.Radius *= 220.0f;

                        DebugShapeRenderer.AddBoundingSphere(rockBoundingSphere, Color.Yellow);

                        if (bulletBoundingSphere.Intersects(rockBoundingSphere))
                        {
                            //Pos for not drawing
                            //rockPos[j] = new Vector3(123, 123, 123);
                            Sb.Reset();
                        }
                        //}
                    }
                }
            }
        }
        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            PlayerIndex player;
            if (pauseAction.Evaluate(input, ControllingPlayer, out player) || gamePadDisconnected)
            {
#if WINDOWS_PHONE
                ScreenManager.AddScreen(new PhonePauseScreen(), ControllingPlayer);
#else

                ScreenManager.AddScreen(new PauseMenuScreen(ScreenManager.SoundBank), ControllingPlayer);
                GamePad.SetVibration(PlayerIndex.Two, 0.0f, 0.0f);
                GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
#endif
            }
            else
            {
                
            }

        }

        private void UpdateCameraChaseTarget(Ship ships,ChaseCamera camera)
        {
            camera.ChasePosition = ships.Position;
            camera.ChaseDirection = ships.Direction + ships.Up/5;
            camera.Up = ships.Up;
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.DarkBlue, 0, 0);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            Vector3 ship1IndicatorPos = ship.Position + ship.Up * 1000;
            Vector3 ship1IndicatorHPR = Vector3.Zero;
            Vector3 ship1Dir = ship.Position - ship2.Position;
            float ship1Len = ship1Dir.Length();
            ship1IndicatorHPR.X = (float)Math.Atan2(ship1Dir.X, ship1Dir.Z);
            ship1IndicatorHPR.Y = -(float)Math.Asin(ship1Dir.Y / ship1Len);

            Vector3 ship2IndicatorPos = ship2.Position + ship2.Up * 1000;
            Vector3 ship2IndicatorHPR = Vector3.Zero;
            Vector3 ship2Dir = ship2.Position - ship.Position;
            float ship2Len = ship2Dir.Length();
            ship2IndicatorHPR.X = (float)Math.Atan2(ship2Dir.X, ship2Dir.Z);
            ship2IndicatorHPR.Y = -(float)Math.Asin(ship2Dir.Y / ship2Len);

            bloom.BeginDraw();
            #region Bottom Screen

            ScreenManager.GraphicsDevice.Viewport = bottomViewport;

            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            ScreenManager.GraphicsDevice.Clear(Color.Black);

            for (int i = 0; i < ship.bullets.Length; i++)
            {
                if (ship.bullets[i].isAlive)
                {
                    DrawModel(bulletModel, Matrix.CreateScale(10) * Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * ship.bullets[i].World, camera);
                }
            }

            for (int i = 0; i < ship2.bullets.Length; i++)
            {
                if (ship2.bullets[i].isAlive)
                {
                    DrawModel(bulletModel, Matrix.CreateScale(10) * Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * ship2.bullets[i].World, camera);
                }
            }

            if (ScreenManager.shipChosenbool2 == false)         
            {
                DrawModel(shipModel2, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship2.World, envEffect2, camera);
                if (ScreenManager.shipChosenbool1 == false)
                {
                    DrawModel(shipModel2, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship.World, envEffect2, camera);
                }
                if (ScreenManager.shipChosenbool1 == true)
                {
                    DrawModel(shipModel, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship.World, envEffect1, camera);
                }
            }
            else if (ScreenManager.shipChosenbool2 == true)
            {
                DrawModel(shipModel, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship2.World, envEffect1, camera);
                if (ScreenManager.shipChosenbool1 == false)
                {
                    DrawModel(shipModel2, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship.World, envEffect2, camera);
                }
                if (ScreenManager.shipChosenbool1 == true)
                {
                    DrawModel(shipModel, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship.World, envEffect1, camera);
                }

            }

            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            for (int i = 0; i < rockPos.Length; i++)
            {
                if (Vector3.Distance(ship1Pos, rockPos[i]) < 10000000)
                {
                    DrawModel(rockModel, Matrix.CreateScale(rockScale[i]) * Matrix.CreateRotationY(MathHelper.ToRadians(rockRotation[i])) * Matrix.CreateTranslation(rockPos[i]), camera);
                }
            }

            DrawModel(indicatorModel, Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * Matrix.CreateScale(10) * Matrix.CreateFromYawPitchRoll(ship1IndicatorHPR.X, ship1IndicatorHPR.Y, ship1IndicatorHPR.Z) * Matrix.CreateTranslation(ship1IndicatorPos), camera);
            DrawModel(skyBoxModel, Matrix.CreateScale(10000) * Matrix.CreateTranslation(camera.Position), camera);


            
            #endregion

            #region Top Screen

            ScreenManager.GraphicsDevice.Viewport = topViewport;
            
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            for (int i = 0; i < ship.bullets.Length; i++)
            {
                if (ship.bullets[i].isAlive)
                {
                    DrawModel(bulletModel, Matrix.CreateScale(10) * Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * ship.bullets[i].World, camera2);
                }
            }

            for (int i = 0; i < rockPos.Length; i++)
            {
                if (Vector3.Distance(ship2Pos, rockPos[i]) < 10000000)
                {
                    DrawModel(rockModel, Matrix.CreateScale(rockScale[i]) * Matrix.CreateRotationY(MathHelper.ToRadians(rockRotation[i])) * Matrix.CreateTranslation(rockPos[i]), camera2);
                }
            }

            for (int i = 0; i < ship2.bullets.Length; i++)
            {
                if (ship2.bullets[i].isAlive)
                {
                    DrawModel(bulletModel, Matrix.CreateScale(10) * Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * ship2.bullets[i].World, camera2);
                }
            }

            if (ScreenManager.shipChosenbool1 == false)
            {
                DrawModel(shipModel2, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship.World, envEffect2, camera2);
                if (ScreenManager.shipChosenbool2 == true)
                {
                    DrawModel(shipModel, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship2.World, envEffect1, camera2);
                }
                if (ScreenManager.shipChosenbool2 == false)
                {
                    DrawModel(shipModel2, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship2.World, envEffect2, camera2);
                }
            }
            else if (ScreenManager.shipChosenbool1 == true)
            {
                DrawModel(shipModel, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship.World, envEffect1, camera2);
                if (ScreenManager.shipChosenbool2 == true)
                {
                    DrawModel(shipModel, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship2.World, envEffect1, camera2);
                }
                if (ScreenManager.shipChosenbool2 == false)
                {
                    DrawModel(shipModel2, Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(25) * ship2.World, envEffect2, camera2);
                }
            }

            DrawModel(indicatorModel, Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * Matrix.CreateScale(10) * Matrix.CreateFromYawPitchRoll(ship2IndicatorHPR.X, ship2IndicatorHPR.Y, ship2IndicatorHPR.Z) * Matrix.CreateTranslation(ship2IndicatorPos), camera2);
            DrawModel(skyBoxModel, Matrix.CreateScale(10000) * Matrix.CreateTranslation(camera2.Position), camera2);

            //DrawModel(rockModel, Matrix.CreateScale(100) * Matrix.Identity, camera2);
            //DrawModel(groundModel, Matrix.Identity, camera2);
            //DrawModel(shipModel, Matrix.CreateTranslation(50, 50, 100) * Matrix.CreateScale(10));


            #endregion
            bloom.EndDraw();

            #region No Bloom Draw

            bloom.ShowBuffer = BloomComponent.IntermediateBuffer.FinalResult;
            spriteBatch.Begin();
            spriteBatch.Draw(bloom.FinalRenderTarget, new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            // draw GUI:

            ScreenManager.GraphicsDevice.Viewport = bottomViewport;
            //particleEffect.Draw(ScreenManager.GraphicsDevice, camera.View, camera.Projection);

            spriteBatch.Begin();
            //spriteBatch.DrawString(gameFont, ship.Position.ToString(), new Vector2(50, 50), Color.White);
            spriteBatch.Draw(TLifeBarBar, new Rectangle(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.X + 50, 40, (int)ship2.shipHealth * 2, 50), new Rectangle(0, 0, (int)ship2.shipHealth * 2, 50), Color.White);
            spriteBatch.Draw(TLifeBarFrame, new Rectangle(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.X + 50, 40, 200, 50), Color.White);
            
            //spriteBatch.DrawString(gameFont, "Health : " + ship2.shipHealth, new Vector2(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.X + 50, 40), Color.White);
            spriteBatch.End();

            ScreenManager.GraphicsDevice.Viewport = topViewport;

            //particleEffect.Draw(ScreenManager.GraphicsDevice, camera2.View, camera2.Projection);

            spriteBatch.Begin();
            //spriteBatch.DrawString(gameFont, ship2.Position.ToString(), new Vector2(50, 50), Color.White);
            spriteBatch.Draw(TLifeBarBar, new Rectangle(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.X + 50, 40,(int)ship.shipHealth * 2, 50), new Rectangle(0, 0, (int)ship.shipHealth * 2, 50), Color.White);
            spriteBatch.Draw(TLifeBarFrame, new Rectangle(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.X + 50, 40, 200, 50), Color.White);
            
            //spriteBatch.DrawString(gameFont, "Health : " + ship.shipHealth, new Vector2(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.X + 50, 40), Color.White);
            //spriteBatch.DrawString(gameFont, "Power  : " + (ship2.bullets.Length - ship2.currentBullet), new Vector2(ScreenManager.GraphicsDevice.Viewport.TitleSafeArea.Width - 200, 40), Color.White);
            spriteBatch.End();
            #endregion

            DebugShapeRenderer.Draw(gameTime, camera2.View, camera2.Projection);
        }

        private void DrawModel(Model model, Matrix world, EnvironmentMapEffect be, ChaseCamera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mm in model.Meshes)
            {

                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    be.EnableDefaultLighting();
                    be.DiffuseColor = new Vector3(1, 1, 1);
                    be.EmissiveColor = new Vector3(1, 1, 1);
                    be.EnvironmentMapSpecular = new Vector3(0.05f, 0.05f, 0.05f);
                    be.View = camera.View;
                    be.Projection = camera.Projection;
                    be.World = world;
                    ScreenManager.GraphicsDevice.SetVertexBuffer(mmp.VertexBuffer, mmp.VertexOffset);
                    ScreenManager.GraphicsDevice.Indices = mmp.IndexBuffer;
                    be.CurrentTechnique.Passes[0].Apply();
                    ScreenManager.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList, 0, 0,
                        mmp.NumVertices, mmp.StartIndex, mmp.PrimitiveCount);
                }

                //foreach (Effect effect in mm.Effects)
                //{
                //    if ((effect is BasicEffect) == false)
                //        continue;

                //    BasicEffect basicEffect = (BasicEffect)effect;

                //    //effect.SpecularColor = Color.WhiteSmoke.ToVector3();
                //    ///effect.SpecularPower = 100.0f;
                //    //effect.FogEnabled = true;
                //    //effect.FogColor = Color.White.ToVector3();
                //    //effect.FogStart = 999999.0f;
                //    //effect.FogEnd = 1000000.0f;
                //    basicEffect.EnableDefaultLighting();
                //    basicEffect.DiffuseColor = new Vector3(1, 0, 0);
                //    basicEffect.World = transforms[mm.ParentBone.Index] * world;
                //    // Use the matrices provided by the chase camera
                //    basicEffect.View = camera.View;
                //    basicEffect.Projection = camera.Projection;
                //}

                //mm.Draw();
            }
        }

        private void DrawModel(Model model, Matrix world,ChaseCamera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //effect.SpecularColor = Color.WhiteSmoke.ToVector3();
                    ///effect.SpecularPower = 100.0f;
                    //effect.FogEnabled = true;
                    //effect.FogColor = Color.White.ToVector3();
                    //effect.FogStart = 999999.0f;
                    //effect.FogEnd = 1000000.0f;
                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = new Vector3(0.7f, 0.7f, 0.7f);
                    effect.World = transforms[mesh.ParentBone.Index] * world;
                    // Use the matrices provided by the chase camera
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }

        private void RandomRockSpawner()
        {
            for (int i = 0; i < rockPos.Length; i++)
            {
                float randX = random.Next(-1000000, 1000000);
                float randY = random.Next(-1000000, 1000000);
                float randZ = random.Next(-1000000, 1000000);

                if (randX >= ship.Position.X - 500 && randX <= ship.Position.X)
                    randX -= 500;
                else if (randX >= ship.Position.X && randX <= ship.Position.X + 500)
                    randX += 500;

                if (randY >= ship.Position.Y - 500 && randY <= ship.Position.Y)
                    randY -= 500;
                else if (randY >= ship.Position.Y && randY <= ship.Position.Y + 500)
                    randY += 500;

                if (randZ >= ship.Position.Z - 500 && randZ <= ship.Position.Z)
                    randZ -= 500;
                else if (randZ >= ship.Position.Z && randZ <= ship.Position.Z + 500)
                    randZ += 500;

                if ((randX >= ship.Position.X - 500 && randX <= ship.Position.X) && (randY >= ship.Position.Y - 500 && randY <= ship.Position.Y) && (randZ >= ship.Position.Z - 500 && randZ <= ship.Position.Z) ||
                    (randX >= ship2.Position.X - 500 && randX <= ship2.Position.X) && (randY >= ship2.Position.Y - 500 && randY <= ship2.Position.Y) && (randZ >= ship2.Position.Z - 500 && randZ <= ship2.Position.Z))
                {
                    randX -= 500;
                }
                else if ((randX >= ship.Position.X + 500 && randX <= ship.Position.X) && (randY >= ship.Position.Y + 500 && randY <= ship.Position.Y) && (randZ >= ship.Position.Z + 500 && randZ <= ship.Position.Z) ||
                        (randX >= ship2.Position.X + 500 && randX <= ship2.Position.X) && (randY >= ship2.Position.Y + 500 && randY <= ship2.Position.Y) && (randZ >= ship2.Position.Z + 500 && randZ <= ship2.Position.Z))
                {
                    randX += 500;
                }

                rockPos[i] = new Vector3(randX, randY, randZ);
                rockScale[i] = random.Next(10, 200);
                rockRotation[i] = random.Next(0, 359);
            }
        }
        #endregion
    }

    
}
