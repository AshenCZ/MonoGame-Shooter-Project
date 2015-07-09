using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace NPRG2_Shooter
{
    // Enemy moving from top to bottom and forward
    // A lot inherited from Enemy
    class TopDownEnemy : Enemy
    {
        public float f_enemyTDMoveSpeedY = 4f;
        public bool b_smerNahoru = true;
        public float f_enemyTDLowBound = 0;
        public float f_enemyTDUpBound = 0;

        public override int GetEnemyValue()
        {
            return 100;
        }
        public override float GetEnemyMoveSpeed()
        {
            return 4f;
        }

        // Updated initialization with more random factors
        public override void Initialize(Animation animation, Vector2 position,float vyska)
        {
            ani_enemyAnimation = animation;
            v2_enemyPosition = position;
            b_enemyActive = true;
            i_enemyHealth = 50;

            Random rand = new Random();
            f_enemyTDLowBound = rand.Next((int)vyska*2 / 3, (int)vyska - 140);
            f_enemyTDUpBound = rand.Next(30, (int)vyska / 3);

            Random rnd = new Random((int)v2_enemyPosition.Y);
            TS_prevShot = TimeSpan.FromMilliseconds(rnd.Next(-5000, -2000));
        }

        // Updated logic
        public override void Update(GameTime gameTime)
        {           
            // Move the enemy
            v2_enemyPosition.X -= GetEnemyMoveSpeed();
            if (b_smerNahoru) // if the direction is up, we move the enemy up
            {
                v2_enemyPosition.Y -= f_enemyTDMoveSpeedY;
                // check if he shouldn't be going the other direction
                if (v2_enemyPosition.Y <= f_enemyTDUpBound)
                {
                    b_smerNahoru = false;
                }
            }
            else // analogic for the opposite side
            {
                v2_enemyPosition.Y += f_enemyTDMoveSpeedY;
                if (v2_enemyPosition.Y >= f_enemyTDLowBound)
                {
                    b_smerNahoru = true;
                }
            }

            ani_enemyAnimation.Position = v2_enemyPosition;
            ani_enemyAnimation.Update(gameTime);

            // Check if off-screen or dead
            if (v2_enemyPosition.X < -Width || i_enemyHealth <= 0)
            {
                b_enemyActive = false;
            }
        }
    }
}
