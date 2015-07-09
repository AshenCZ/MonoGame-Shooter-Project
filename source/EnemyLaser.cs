using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    class EnemyLaser : Laser
    {
        // Everything is inherited from Laser
        // Only using constructors to set the movement speeds
        public EnemyLaser()
        {
            f_laserMoveSpeed = -10f;
            i_laserDamage = 25;
            f_laserMoveSpeedY = 0f;
        }

        public EnemyLaser(float movey)
        {
            f_laserMoveSpeed = -10f;
            i_laserDamage = 25;
            f_laserMoveSpeedY = movey;
        }
    }
}
