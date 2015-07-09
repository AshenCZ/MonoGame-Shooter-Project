using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    class Explosion
    {
        Animation ani_exploAnimation;
        Vector2 v2_exploPosition;

        public bool b_exploActive;
        int i_timeToLive;  // frames for the explosion to play

        public int Width{
            get { return ani_exploAnimation.FrameWidth; }
        }
        public int Height{
            get { return ani_exploAnimation.FrameWidth; }
        }

        public void Initialize(Animation animation, Vector2 position)
        {
            ani_exploAnimation = animation;
            v2_exploPosition = position;
            b_exploActive = true;

            i_timeToLive = 30;
        }

        public void Update(GameTime gameTime)
        {
            // play the animation
            ani_exploAnimation.Update(gameTime);

            // check if the explosion should be deactivated
            i_timeToLive -= 1;
            if (i_timeToLive <= 0)
            {
                this.b_exploActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ani_exploAnimation.Draw(spriteBatch);
        }
    }
}
