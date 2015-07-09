using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    class Player : IFlier
    {
        public bool b_playerActive;
        public Vector2 v2_playerPosition;           
        public int i_playerHealth;                  
        public float f_playerSpeed;
        public int i_playerScore;
        public Animation ani_playerAnimation;

        public int Width {
            get { return ani_playerAnimation.FrameWidth; }
        } // Get the width of the player ship
        public int Height {
            get { return ani_playerAnimation.FrameHeight; }
        } // Get the height of the player ship

        // IFlier interface methods
        public Vector2 GetPosition()
        {
            return new Vector2(v2_playerPosition.X,v2_playerPosition.Y);
        }
        public Vector2 GetGunOffset()
        {
            return new Vector2(50, 10);
        }

        // What happens at the beginning
        public void Initialize(Animation animation, Vector2 position, float movspeed)
        {
            ani_playerAnimation = animation;
            v2_playerPosition = position;
            b_playerActive = true;
            i_playerHealth = 100;
            f_playerSpeed = movspeed;
        }

        // Update every Timer tick
        public void Update(GameTime gameTime)
        {
            ani_playerAnimation.Position = v2_playerPosition;
            ani_playerAnimation.Update(gameTime); 
        }

        // Graphics
        public void Draw(SpriteBatch spriteBatch)
        {
            ani_playerAnimation.Draw(spriteBatch);
        } 
    }
}
