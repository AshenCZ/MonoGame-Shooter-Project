using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    class Enemy : IFlier
    {
        public Animation ani_enemyAnimation;
        public Vector2 v2_enemyPosition;
        public bool b_enemyActive;
        public int i_enemyHealth;        
        public TimeSpan TS_prevShot;

        // all enemies share these values, making them variables would be a waste of space
        public virtual int GetEnemyDamage()
        {
            return 50;
        }
        public virtual int GetEnemyValue()
        {
            return 50;
        }
        public virtual float GetEnemyMoveSpeed()
        {
            return 2f;
        }

        public int Width {
            get { return ani_enemyAnimation.FrameWidth; }
        } // Get the width of the enemy
        public int Height {
            get { return ani_enemyAnimation.FrameHeight; }
        } // Get the height of the enemy
        
        // IFlier interface methods
        public Vector2 GetPosition()
        {
            return new Vector2(v2_enemyPosition.X, v2_enemyPosition.Y);
        }
        public Vector2 GetGunOffset()
        {
            return new Vector2(10, 10);
        }

        public virtual void Initialize(Animation animation, Vector2 position, float placeholder)
        {
            ani_enemyAnimation = animation;
            v2_enemyPosition = position;
            b_enemyActive = true;
            i_enemyHealth = 50;

            // random previous shot, this is to make the first enemies not shoot at the same time
            Random rnd = new Random((int)v2_enemyPosition.Y);
            TS_prevShot = TimeSpan.FromMilliseconds(rnd.Next(-5000, -1000));
        }

        public virtual void Update(GameTime gameTime)
        {
            // Move the enemy
            v2_enemyPosition.X -= GetEnemyMoveSpeed();
            ani_enemyAnimation.Position = v2_enemyPosition;
            ani_enemyAnimation.Update(gameTime);

            // Check if off-screen or dead
            if (v2_enemyPosition.X < -Width || i_enemyHealth <= 0)
            {
                b_enemyActive = false;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            ani_enemyAnimation.Draw(spriteBatch);
        }
    }
}
