using Assets.TestingAssets.TestScripts;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public interface IShadowFollow : IFollow
    { 
        public IShadowFollow CreateShadowFollow(Func<float> getRotation, Func<float, float> setRotation, Seeker seeker, ContactFilter2D contactFilter, Collider2D aggroCollider);

        public void Init(Func<float> getRotation, Func<float, float> setRotation, ContactFilter2D contactFilter, Collider2D aggroCollider, Rigidbody2D rigidBody);
    }
}
