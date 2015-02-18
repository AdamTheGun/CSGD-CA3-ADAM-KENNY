using Microsoft.Xna.Framework;

namespace Particles.Particles.Modifiers
{
    public class AlphaAgeModifier : Modifier
    {
        public override void Update(Particle particle, float particleAge)
        {
            particle.Alpha = MathHelper.Lerp(1, 0, particleAge);
        }
    }
}
