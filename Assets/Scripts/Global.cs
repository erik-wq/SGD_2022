using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    class Global : MonoBehaviour
    {
        public static Global Instance { get { return _instance; } private set { _instance = value; } }

        private static Global _instance = new Global();

        public Transform PlayerTransform { get; set; }
        public Transform HopeTransform { get; set; }
        public HopeAI HopeScript { get; set; }
        public PlayerController PlayerScript { get; set; }

        private Global() { }
    }
}
