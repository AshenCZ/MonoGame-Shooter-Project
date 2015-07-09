using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NPRG2_Shooter
{
    class BuildingEnemy : Enemy
    {
        // Everything is inherited from Enemy, and the logic is changed in UpdateCollision()

        public override int GetEnemyValue()
        {
            return 0; // indestructible
        }

        public override float GetEnemyMoveSpeed()
        {
            return 1f;
        }

        public override void Initialize(Animation animation, Vector2 position, float vyska)
        {
            ani_enemyAnimation = animation;
            v2_enemyPosition = position;
            b_enemyActive = true;   
            i_enemyHealth = 1;      // indestructible
        }
    }
}
