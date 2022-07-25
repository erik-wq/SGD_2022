using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.TestingAssets
{
    public interface IShadowEnemy
    {
        void SetFree();
        bool TakeDamage(float damage);
        float Damage { get; set; }
    }
}
