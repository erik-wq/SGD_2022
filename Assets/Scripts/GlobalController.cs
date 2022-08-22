using Assets.Scripts.Totems;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Assets.Scripts
{
    class GlobalController : MonoBehaviour
    {
        [SerializeField] public TotemAI Totem1;
        [SerializeField] public TotemAI Totem2;
        [SerializeField] public TotemAI Totem3;
        [SerializeField] public TotemAI Totem4;
        [SerializeField] public TotemAI Pandora;

        [SerializeField] public GameObject Group1;
        [SerializeField] public GameObject Group2;
        [SerializeField] public GameObject Group3;
        [SerializeField] public GameObject Group4;
        [SerializeField] public GameObject Group5;

        [SerializeField] public GameObject Menu;
        [SerializeField] public GameObject UI;
        [SerializeField] public GameObject EndScreen;
        [SerializeField] public GameObject EndgameLights;
        //[SerializeField] public GameObject HintsUI;

        //[SerializeField] public Image Hint1;
        //[SerializeField] public Image Hint2;
        //[SerializeField] public Image Hint3;
        //[SerializeField] public Image Hint4;
        //[SerializeField] public Image Hint5;

        [SerializeField] public Transform Hero;
        [SerializeField] public GameObject Hope;


        private Vector3 _startLocation = new Vector3(46.1f, -46.4f, 0);
        private Vector3 _firstCoords = new Vector3(45.13f, -3.07f, 0);
        private Vector3 _secondCoords = new Vector3(-26.55f, 42.04f, 0);
        private Vector3 _thirdCoords = new Vector3(98.02f, 67.57f, 0);
        private Vector3 _fourthCoords = new Vector3(43.28f, 199.25f, 0);

        private int Stage = 0;
        private List<TotemAI> _totems;
        private List<GameObject> _groups;
        private List<Vector3> _respawnPoints;
        //private List<Image> _hints;

        private PlayerController _playerScript;
        private HopeAI _hopeScript;
        //private bool _isHintActive = false;

        public void Awake()
        {
            _totems = new List<TotemAI>();
            _totems.Add(Totem1);
            _totems.Add(Totem2);
            _totems.Add(Totem3);
            _totems.Add(Totem4);
            _totems.Add(Pandora);

            _groups = new List<GameObject>();
            _groups.Add(Group1);
            _groups.Add(Group2);
            _groups.Add(Group3);
            _groups.Add(Group4);
            _groups.Add(Group5);

            _respawnPoints = new List<Vector3>();
            _respawnPoints.Add(_startLocation);
            _respawnPoints.Add(_firstCoords);
            _respawnPoints.Add(_secondCoords);
            _respawnPoints.Add(_thirdCoords);
            _respawnPoints.Add(_fourthCoords);

            //_hints = new List<Image>();
            //_hints.Add(Hint1);
            //_hints.Add(Hint2);
            //_hints.Add(Hint3);
            //_hints.Add(Hint4);
            //_hints.Add(Hint5);

            _playerScript = Hero.GetComponent<PlayerController>();
            _hopeScript = Hope.GetComponent<HopeAI>();
        }

        //public void HideHint()
        //{
        //    HideHints();
        //    _isHintActive = false;
        //    Time.timeScale = 1;
        //}

        //public void ShowHint(int index)
        //{
        //    _isHintActive = true;
        //    _hints[index].enabled = true;
        //    HintsUI.SetActive(true);
        //    Time.timeScale = 0;
        //}

        //private void HideHints()
        //{
        //    Hint1.enabled = false;
        //    Hint2.enabled = false;
        //    Hint3.enabled = false;
        //    Hint4.enabled = false;
        //    Hint5.enabled = false;

        //    HintsUI.SetActive(false);
        //}


        public void Die()
        {
            ShowMenu();
        }

        public void EndSequence()
        {
            Hero.gameObject.SetActive(false);
            Hope.gameObject.SetActive(false);

            EndScreen.SetActive(true);
            UI.SetActive(false);
            Menu.SetActive(false);
        }

        public void Win()
        {
            KillEnemies();
            _hopeScript.Win();
            EndgameLights.SetActive(true);
        }

        public void Exit()
        {
            Time.timeScale = 1;
            GameData.sceneManagement.LoadScene("Menu", "MainLevel");
        }

        public void Respawn()
        {
            _playerScript.TurnOffEasyMode();
            RunRespawn();
        }

        public void RespawnEasyMode()
        {
            _playerScript.TurnOnEasyMode();
            RunRespawn();
        }

        public void RunRespawn()
        {
            var possition = _respawnPoints[Stage];
            Hero.transform.position = possition;
            possition.y += 1;
            Hope.transform.position = possition;
            KillAndRespawn();
            ResetTotem();
            HideMenu();
            Time.timeScale = 1;
            _playerScript.Reset();
            _hopeScript.Reset();
        }

        private void HideMenu()
        {
            Time.timeScale = 1;
            Menu.SetActive(false);
            UI.SetActive(true);
        }
        private void ShowMenu()
        {
            Menu.SetActive(true);
            UI.SetActive(false);
            Time.timeScale = 0;
        }
        private void KillEnemies()
        {
            GameObject[] objs;
            objs = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var item in objs)
            {
                if (item.GetComponentInChildren<TotemAI>() != null)
                    continue;

                Destroy(item);
            }
        }

        private void KillAndRespawn()
        {
            GameObject[] objs;

            KillEnemies();

            objs = GameObject.FindGameObjectsWithTag("Projectile");
            foreach (var item in objs)
            {
                Destroy(item);
            }

            objs = GameObject.FindGameObjectsWithTag("LightMana");
            foreach (var item in objs)
            {
                Destroy(item);
            }

            for (int i = Stage; i < _groups.Count; i++)
            {
                Instantiate(_groups[i], Vector3.zero, Quaternion.identity);
            }
        }

        private void ResetTotem()
        {
            _totems[Stage].Reset();
        }

        public void MoveStage()
        {
            Stage++;
        }
    }
}
