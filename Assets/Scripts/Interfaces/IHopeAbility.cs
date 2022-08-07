using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets
{
    public interface IHopeAbility
    {
        public bool Activate();
        public float GetCost();
    }
}
