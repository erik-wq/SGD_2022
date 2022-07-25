using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets.TestScripts
{
    public interface IFollow
    {
        void SetTarget(Transform transform);
        bool Paused { get; set; }
    }
}
