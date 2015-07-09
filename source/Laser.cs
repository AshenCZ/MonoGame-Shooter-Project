using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace NPRG2_Shooter
{
    class Laser
    {
        public Animation ani_laserAnimation;
        public float f_laserMoveSpeed = 15f;     // horizontal movement of the laser
        public float f_laserMoveSpeedY = 0f;     // vertical movement of the laser
        public Vector2 v2_laserPosition;
        public int i_laserDamage = 50;
        public bool b_laserActive;

        public int Width
        {
            get { return ani_laserAnimation.FrameWidth; }
        }
        public int Height
        {
            get { return ani_laserAnimation.FrameHeight; }
        }


        public void Initialize(Animation animation, Vector2 position)
        {
            ani_laserAnimation = animation;
            v2_laserPosition = position;
            b_laserActive = true;
        } 

        public void Update(GameTime gameTime)
        {
            // move the laser in both axes
            v2_laserPosition.X += f_laserMoveSpeed;
            v2_laserPosition.Y += f_laserMoveSpeedY;

            ani_laserAnimation.Position = v2_laserPosition;
            ani_laserAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ani_laserAnimation.Draw(spriteBatch);
        }

    }
}
