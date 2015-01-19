#region File Description
//-----------------------------------------------------------------------------
// Ship.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace ChaseCameraSample
{
    class Ship
    {
        #region Fields

        private const float MinimumAltitude = 350.0f;

        /// <summary>
        /// A reference to the graphics device used to access the viewport for touch input.
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Location of ship in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Direction ship is facing.
        /// </summary>
        public Vector3 Direction;

        /// <summary>
        /// Ship's up vector.
        /// </summary>
        public Vector3 Up;

        
        private Vector3 right;
        /// <summary>
        /// Ship's right vector.
        /// </summary>
        public Vector3 Right
        {
            get { return right; }
        }

        /// <summary>
        /// Full speed at which ship can rotate; measured in radians per second.
        /// </summary>
        private const float RotationRate = 1.5f;

        /// <summary>
        /// Mass of ship.
        /// </summary>
        private const float Mass = 1.0f;

        public float shipHealth = 100.0f;

        private KeyboardState prevKeyboardState;

        /// <summary>
        /// Maximum force that can be applied along the ship's direction.
        /// </summary>
        private const float ThrustForce = 24000.0f;

        /// <summary>
        /// Velocity scalar to approximate drag.
        /// </summary>
        private const float DragFactor = 0.99f;

        /// <summary>
        /// Current ship velocity.
        /// </summary>
        public Vector3 Velocity;

        float maxVelocity = 20000;

        public ShipBullet[] bullets = new ShipBullet[1000];

        /// <summary>
        /// Ship world transform matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
        }
        private Matrix world;

        private Vector3 oldPos;

        MouseState oldMouse;

        Random rand;
        Cue pewCue;
        SoundBank shipSounds;
        float pewTimer = 0.0f;
        bool pewBool = false;

        AudioEmitter Emitter1, Emitter2;
        AudioListener Listener;

        public int currentBullet;

        float crashTimer = 0.0f;
        bool crashBool = false;

        #endregion

        #region Initialization

        public Ship(GraphicsDevice device, Vector3 NewPosition, SoundBank sounds,
            AudioEmitter Emit1, AudioEmitter Emit2, AudioListener listen)
        {
            shipSounds = sounds;
            graphicsDevice = device;
            Reset(NewPosition);
            currentBullet = 0;
            for (int i = 0; i < bullets.Length; i++)
            {
                bullets[i] = new ShipBullet(device, Position);
            }
            rand = new Random();
            Emitter1 = Emit1;
            Emitter2 = Emit2;
            Listener = listen;

            pewCue = sounds.GetCue("ShotFx");
            pewCue.Apply3D(Listener, Emitter2);
        }

        

        /// <summary>
        /// Restore the ship to its original starting state
        /// </summary>
        public void Reset(Vector3 NewPosition)
        {
            Position = NewPosition;
            Direction = Vector3.Forward;
            Up = Vector3.Up;
            right = Vector3.Right;
            Velocity = Vector3.Zero;
            world = Matrix.Identity;
            world.Forward = Direction;
            world.Up = Up;
            world.Right = right;
            world.Translation = Position;
        }



        #endregion

        bool TouchLeft()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.X <= graphicsDevice.Viewport.Width / 3;
        }

        bool TouchRight()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.X >= 2 * graphicsDevice.Viewport.Width / 3;
        }

        bool TouchDown()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.Y <= graphicsDevice.Viewport.Height / 3;
        }

        bool TouchUp()
        {
            MouseState mouseState = Mouse.GetState();
            return mouseState.LeftButton == ButtonState.Pressed &&
                mouseState.Y >= 2 * graphicsDevice.Viewport.Height / 3;
        }

        

        /// <summary>
        /// Applies a simple rotation to the ship and animates position based
        /// on simple linear motion physics.
        /// </summary>
        public void Update(GameTime gameTime, Model ship, Model model,Model bulletModel,Matrix OtherShipMtx,int playerNum)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState1 = GamePad.GetState(PlayerIndex.One);
            GamePadState gamePadState2 = GamePad.GetState(PlayerIndex.Two);
            MouseState mouseState = Mouse.GetState();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            

            
            // Determine rotation amount from input
            Vector2 rotationAmount = new Vector2(0.0f,0.0f);
            if (playerNum == 2)
            {
                rotationAmount = -gamePadState2.ThumbSticks.Left;
            } 
            else if (playerNum == 1)
            {
                rotationAmount = -gamePadState1.ThumbSticks.Left;
            }
            
                


            
            // Scale rotation amount to radians per second
            rotationAmount = rotationAmount * RotationRate * elapsed;

            // Correct the X axis steering when the ship is upside down
            if (Up.Y < 0)
                rotationAmount.X = -rotationAmount.X;


            // Create rotation matrix from rotation amount
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                Matrix.CreateRotationY(rotationAmount.X);

            // Rotate orientation vectors
            Direction = Vector3.TransformNormal(Direction, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            Direction.Normalize();
            Up.Normalize();

            // Re-calculate Right
            right = Vector3.Cross(Direction, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Direction);

            double randNum = rand.NextDouble();
            
            // Determine thrust amount from input
            float thrustAmount = 0.0f;

            if (playerNum == 2)
            {
                thrustAmount = gamePadState2.Triggers.Right;
                if (randNum <= 0.5f)
                {
                    if (gamePadState2.Buttons.X == ButtonState.Pressed || mouseState.LeftButton == ButtonState.Pressed)
                    {
                        bullets[currentBullet++].isAlive = true;
                        if (pewBool == false)
                        {
                            if (!pewCue.IsPlaying)
                            {
                                pewCue = shipSounds.GetCue("ShotFx");
                                pewCue.Apply3D(Listener, Emitter1);
                                pewCue.Play();
                                pewBool = true;
                            }
                        }
                    }
                }
            }
            else if (playerNum == 1)
            {
                thrustAmount = gamePadState1.Triggers.Right;
                if (randNum <= 0.5f)
                {
                    if (gamePadState1.Buttons.X == ButtonState.Pressed)
                    {
                        bullets[currentBullet++].isAlive = true;
                        if (pewBool == false)
                        {
                            if (!pewCue.IsPlaying)
                            {
                                pewCue = shipSounds.GetCue("ShotFx");
                                pewCue.Apply3D(Listener, Emitter2);
                                pewCue.Play();
                                pewBool = true;
                            }
                        }
                    }
                }
            }

            if (pewBool) 
            {
                pewTimer += elapsed*1;
                if (pewTimer >= 0.13) 
                {
                    pewTimer = 0.0f;
                    pewBool = false;
                    pewCue.Stop(AudioStopOptions.Immediate);
                }
            }
            
            //if (playerNum == 1)
            //{
            //    if (randNum <= 0.5f)
            //    {
            //        if (keyboardState.IsKeyDown(Keys.E) || mouseState.LeftButton == ButtonState.Pressed)
            //        {
            //            bullets[currentBullet++].isAlive = true;
            //            if (pewBool == false)
            //            {
            //                if (!shipSounds.GetCue("ShotFx").IsPlaying)
            //                {
            //                    shipSounds.GetCue("ShotFx").Play();
            //                    pewBool = true;
            //                }
            //            }
            //        }
            //    }
            //}


            // Calculate force from thrust amount
            Vector3 force = Direction * thrustAmount * ThrustForce;

            // Apply acceleration
            Vector3 acceleration = force / Mass;
            Velocity += acceleration * elapsed;

            // Apply psuedo drag
            Velocity *= DragFactor;

            if (Velocity.X >= maxVelocity)
            {
                Velocity.X = maxVelocity;
            }
            if (Velocity.Y >= maxVelocity)
            { 
                Velocity.Y = maxVelocity;
            }
            if (Velocity.Z >= maxVelocity)
            { 
                Velocity.Z = maxVelocity;
            }
            if (Velocity.X <= -maxVelocity)
            {
                Velocity.X = -maxVelocity;
            }
            if (Velocity.Y <= -maxVelocity)
            {
                Velocity.Y = -maxVelocity;
            }
            if (Velocity.Z <= -maxVelocity)
            {
                Velocity.Z = -maxVelocity;
            }
            // Apply velocity
            oldPos = Position;
            Position += Velocity * elapsed;

            if (!crashBool)
            {
                ShipCollision(ship, model, bulletModel, OtherShipMtx, 30, 0.7f);
            }
            else 
            {
                crashTimer += elapsed * 1;
                if (crashTimer >= 5)
                {
                    crashBool = false;
                    shipSounds.GetCue("CrashFx").Stop(AudioStopOptions.Immediate);
                    crashTimer = 0.0f;
                }
            }

            

            // Prevent ship from flying under the ground
            Position.Y = Math.Max(Position.Y, MinimumAltitude);

            if (currentBullet > bullets.Length-1)
            {
                currentBullet = 0;
            }

            for (int i = 0; i < bullets.Length; i++)
            {
                if (!bullets[i].isAlive)
                {
                    bullets[i].Direction = Direction;
                    bullets[i].Position = this.Position + (Direction * 1000);
                    bullets[i].Up = this.Up;
                    bullets[i].right = this.right;
                }
                else
                {
                    shipHealth = bullets[i].BulletCollision(bulletModel, ship, shipHealth,OtherShipMtx,10,1.5f);
                }
                bullets[i].Update(gameTime);
            }

            // Reconstruct the ship's world matrix
            world = Matrix.Identity;
            world.Forward = Direction;
            world.Up = Up;
            world.Right = right;
            world.Translation = Position;

            prevKeyboardState = keyboardState;
            oldMouse = mouseState;
        }
        void ShipCollision(Model ship, Model model,Model bulletModel,Matrix OtherShipMtx, int CollisionScale, float radius) 
        {
            for (int i = 0; i < ship.Meshes.Count; i++)
            {
                world.Translation = Position;

                BoundingSphere shipBound = ship.Meshes[i].BoundingSphere;
                shipBound = shipBound.Transform(Matrix.CreateRotationY(MathHelper.ToRadians(-90.0f)) * Matrix.CreateScale(CollisionScale));
                shipBound.Center = Position;
                shipBound.Radius *= 0.7f;
                //shipBound = shipBound.Transform(world);

                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    BoundingSphere modelBound = model.Meshes[j].BoundingSphere;
                    modelBound = modelBound.Transform(Matrix.CreateScale(CollisionScale) * OtherShipMtx);
                    modelBound.Radius *= radius;

                    if (shipBound.Intersects(modelBound))
                    {
                        shipHealth -= 10;
                        shipSounds.GetCue("CrashFx").Play();
                        crashBool = true;
                    }
                }
            }
        }
    }
}
