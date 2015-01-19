using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ChaseCameraSample;

namespace ChaseCameraSample
{
    class ShipBullet
    {

        /// <summary>
        /// A reference to the graphics device used to access the viewport for touch input.
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// Location of bullet in world space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Direction bullet is facing.
        /// </summary>
        public Vector3 Direction;

        public bool isAlive;
        /// <summary>
        /// State of the Bullet
        /// </summary>

        /// <summary>
        /// bullet's up vector.
        /// </summary>
        public Vector3 Up;

        public Vector3 right;
        /// <summary>
        /// bullet's right vector.
        /// </summary>
        public Vector3 Right
        {
            get { return right; }
        }

        public const float LifeSpan = 1.0f;
        public float LifeTime = 0.0f;

        /// <summary>
        /// Mass of bullet.
        /// </summary>
        private const float Mass = 1.0f;

        /// <summary>
        /// Maximum force that can be applied along the bullet's direction.
        /// </summary>
        private const float ThrustForce = 40000.0f;

        /// <summary>
        /// Current bullet velocity.
        /// </summary>
        public Vector3 Velocity;

        public Vector3 SpawnLocation;
        ///<summary>
        ///Bullet initial spawn location
        ///</summary>
        
        /// <summary>
        /// Bullet world transform matrix.
        /// </summary>
        public Matrix World
        {
            get { return world; }
        }
        private Matrix world;

        public ShipBullet(GraphicsDevice device,Vector3 spawn)
        {
            graphicsDevice = device;
            SpawnLocation = spawn;
            Reset();
        }

        private Vector3 oldPos;

        /// <summary>
        /// Restore the ship to its original starting state
        /// </summary>
        public void Reset()
        {
            isAlive = false;
            Position = SpawnLocation;
            Direction = Vector3.Forward;
            Up = Vector3.Up;
            right = Vector3.Right;
            Velocity = Vector3.Zero;
        }

        public float BulletCollision(Model BulletModel,Model CollisionModel,float inHealth,Matrix shipMtx,int CollisionScale,float radius)
        {
            if(isAlive){
                for (int i = 0; i < BulletModel.Meshes.Count; i++)
                {
                    world.Translation = Position;
                    BoundingSphere BulletBound = BulletModel.Meshes[i].BoundingSphere;
                    BulletBound.Center = Position;
                    BulletBound.Radius *= 0.7f;
                    //shipBound = shipBound.Transform(world);

                    for (int j = 0; j < CollisionModel.Meshes.Count; j++)
                    {
                        BoundingSphere modelBound = CollisionModel.Meshes[j].BoundingSphere;
                        modelBound = modelBound.Transform( Matrix.CreateScale(CollisionScale)*shipMtx);
                        modelBound.Radius *= radius;

                        if (BulletBound.Intersects(modelBound))
                        {
                            //Events during collision
                            Position = oldPos;
                            BulletBound.Center = Position;
                            Reset();
                            LifeTime = 0.0f;
                            inHealth -= 1;
                        }
                    }
                }    
            }
            return inHealth;
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (isAlive)
            {
                // Calculate force from thrust amount
                Vector3 force = Direction * ThrustForce;

                // Apply acceleration
                //Vector3 acceleration = force / Mass;
                Velocity = force*2 ;

                // Apply velocity
                oldPos = Position;
                Position += Velocity * elapsed;

                if (LifeTime <= LifeSpan)
                {
                    LifeTime += 1 * elapsed;
                }
                else 
                {
                    Reset();
                    LifeTime = 0.0f;
                }

            }

            // Reconstruct the ship's world matrix
            world = Matrix.Identity;
            world.Forward = Direction;
            world.Up = Up;
            world.Right = right;
            world.Translation = Position;
        }
    }
}
