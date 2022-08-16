using Assets.Scripts.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Totems
{
    class TotemOfDespairOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject Rocks;
        [SerializeField] private GateControl Gate;
        private TotemAI _totemAI;
        
        public void Start()
        {
            _totemAI = GetComponent<TotemAI>();
            _totemAI.RegisterOnDestroy(OnDestroy);
        }

        private void OnDestroy()
        {
            Rocks.SetActive(false);
            Gate.Open();
        }
    }
}
