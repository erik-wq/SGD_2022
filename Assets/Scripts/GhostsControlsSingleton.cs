using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BasicShadowAI;

namespace Assets.Scripts
{
    public class GhostsControlsSingleton
    {
        private const float MINIMAL_RANGE = 7;
        private const float AVERAGE_RANGE = 12;
        private const float STEP = 1.5f;

        private class ShadowCircleInfo
        {
            public ShadowCircleFollow Follow { get; set; }
            public float Range { get; set; }
        }

        private static GhostsControlsSingleton _instance = new GhostsControlsSingleton();
        private List<ShadowCircleInfo> _activeCircleingShadowsPlayer = new List<ShadowCircleInfo>();
        private List<ShadowCircleInfo> _activeCircleingShadowsHope = new List<ShadowCircleInfo>();
        private GhostsControlsSingleton()
        {

        }

        public static GhostsControlsSingleton GetInstance()
        {
            return _instance;
        }

        public float RegisterMe(ShadowCircleFollow follow, TargetTypes target)
        {
            List<ShadowCircleInfo> list;
            if (target == TargetTypes.None)
                return -1;

            if (target == TargetTypes.Player)
                list = _activeCircleingShadowsPlayer;
            else
                list = _activeCircleingShadowsHope;

            var exist = list.Find(a => a.Follow == follow);
            if (exist != null)
            {
                return exist.Range;
            }

            var range = FindFreeLane(list);
            list.Add(new ShadowCircleInfo() { Follow = follow, Range = range });

            return range;
        }

        private float FindFreeLane(List<ShadowCircleInfo> list)
        {
            var up = UnityEngine.Random.Range(0, 1);

            if (up == 1)
            {
                return FindFarFreeLane(list);
            }
            else
            {
                var found = FindClosestFreeLane(list);
                if (found > 0)
                    return found;
            }
            return FindFarFreeLane(list);
        }

        private float FindClosestFreeLane(List<ShadowCircleInfo> list)
        {
            float activeDistance = AVERAGE_RANGE;
            while (true)
            {
                if (list.Find(a => a.Range == activeDistance) == null)
                    return activeDistance;
                activeDistance -= STEP;

                if (activeDistance < MINIMAL_RANGE)
                    return -1;
            }
        }

        private float FindFarFreeLane(List<ShadowCircleInfo> list)
        {
            float activeDistance = AVERAGE_RANGE;
            while (true)
            {
                if (list.Find(a => a.Range == activeDistance) == null)
                    return activeDistance;
                activeDistance += STEP;
            }
        }

        public void RemoveMe(ShadowCircleFollow follow)
        {
            _activeCircleingShadowsHope.RemoveAll(a => a.Follow == follow);
            _activeCircleingShadowsPlayer.RemoveAll(a => a.Follow == follow);
        }
    }
}
