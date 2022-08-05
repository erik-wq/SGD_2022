﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.TestingAssets
{
    public interface IEnemy
    {
        bool TakeDamage(float damage, float force, Vector2 origin);
        bool TakeDamage(float damage);
    }
}
