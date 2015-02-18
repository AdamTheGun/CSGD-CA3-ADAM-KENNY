using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Particles.Particles
{
    public class Particle
    {
        private readonly TextureQuad textureQuad;

        public Particle(int lifespan, Texture2D texture, GraphicsDevice graphicsDevice)
        {
            textureQuad = new TextureQuad(graphicsDevice, texture, texture.Width, texture.Height);
            Lifespan = lifespan;
        }

        public void Update(double totalMilliseconds)
        {
            Position += Velocity;

            if(Lifespan < (totalMilliseconds - Inception))
                IsAlive = false;
        }

        public void Draw(GraphicsDevice graphicsDevice, Matrix viewMatrix, Matrix projectionMatrix)
        {
            textureQuad.Draw(viewMatrix, projectionMatrix, Matrix.CreateTranslation(Position));
        }

        public bool IsAlive { get; set; }
        public float Inception { get; set; }
        public float Lifespan { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }

        public float Alpha
        {
            get { return textureQuad.Alpha; }
            set { textureQuad.Alpha = value; }
        }
    }
}
