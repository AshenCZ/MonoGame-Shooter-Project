using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    class BossEnemy : TopDownEnemy
    {
        public override int GetEnemyDamage()
        {
            return 100;
        }
        public override int GetEnemyValue()
        {
            return 2500;
        }
        public override float GetEnemyMoveSpeed()
        {
            return 2f;
        }

        public override void Initialize(Animation animation, Vector2 position, float f_windowHeight)
        {
            ani_enemyAnimation = animation;
            v2_enemyPosition = position;
            b_enemyActive = true;
            i_enemyHealth = 300;
            f_enemyTDMoveSpeedY = 2f;

            f_enemyTDLowBound = f_windowHeight - 30 - Height/2;
            f_enemyTDUpBound = Height + 20;
        }

        public override void Update(GameTime gameTime)
        {
            // boss has to stop at like 75%, this is hacked a bit because ideally
            // the Enemy (or BossEnemy) class doesn't know about the window width
            // even though he already knows about f_windowHeight
            if (v2_enemyPosition.X + Width > 1150)
            {
                v2_enemyPosition.X -= GetEnemyMoveSpeed();
            }
            else
            {
                if (b_smerNahoru) // if the direction is up, we move the boss up
                {
                    v2_enemyPosition.Y -= f_enemyTDMoveSpeedY;

                    // check if he shouldn't be going the other direction
                    if (v2_enemyPosition.Y <= f_enemyTDUpBound) 
                    {
                        b_smerNahoru = false;
                    }
                }
                else  // same for going down
                {
                    v2_enemyPosition.Y += f_enemyTDMoveSpeedY;
                    if (v2_enemyPosition.Y >= f_enemyTDLowBound)
                    {
                        b_smerNahoru = true;
                    }
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            ani_enemyAnimation.Draw(spriteBatch);
        }
    }
}
