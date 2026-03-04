using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetWorkModule
{
    public static class MathUtils
    {
        public static bool IsHitValid(
         float srcX, float srcZ, float srcRotY, // 공격자 정보
         float destX, float destZ,              // 타겟 정보
         float attackRange, float attackAngle)  // 무기 정보
        {
            float dx = destX - srcX;
            float dz = destZ - srcZ;

            float sqrDistance = (dx * dx) + (dz * dz);
            float sqrRange = attackRange * attackRange;

            if (sqrDistance > sqrRange)
                return false;

            if (sqrDistance == 0) return true;

            float dist = (float)Math.Sqrt(sqrDistance);
            float dirX = dx / dist;
            float dirZ = dz / dist;

            float rad = srcRotY * (float)(Math.PI / 180.0);
            float lookX = (float)Math.Sin(rad);
            float lookZ = (float)Math.Cos(rad);

            float dotProduct = (lookX * dirX) + (lookZ * dirZ);
            float threshold = (float)Math.Cos((attackAngle / 2.0f) * (Math.PI / 180.0));

            return dotProduct >= threshold;
        }
    }
}
