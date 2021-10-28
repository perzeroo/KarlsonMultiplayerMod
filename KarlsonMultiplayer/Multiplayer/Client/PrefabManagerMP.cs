using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer
{
    public class PrefabManagerMP : MonoBehaviour
    {
        public static PrefabManagerMP instance;

        public List<GameObject> prefabs = new List<GameObject>();

        public GameObject ak47;
        public GameObject pistol;
        public GameObject shotgun;
        public GameObject boomer;
        public GameObject enemy;
        public GameObject barrel;
        public GameObject grappler;
        
        private void Awake()
        {
            instance = this;
            
            prefabs.Add(ak47 = Instantiate(GameObject.Find("Ak47")));
            prefabs.Add(pistol = Instantiate(GameObject.Find("Pistol")));
            prefabs.Add(shotgun = Instantiate(GameObject.Find("Shotgun")));
            prefabs.Add(boomer = Instantiate(GameObject.Find("Boomer")));
            prefabs.Add(enemy = Instantiate(GameObject.Find("Enemy")));
            prefabs.Add(barrel = Instantiate(GameObject.Find("Barrel")));
            prefabs.Add(grappler = Instantiate(GameObject.Find("Grappler")));

            foreach (var go in prefabs)
            {
                go.name = go.name.Replace("(Clone)", "") + " [Prefab]";
                DontDestroyOnLoad(go);
                go.SetActive(false);
                
                UnityEngine.Debug.Log(go.name + " created.");
            }

            SceneManager.LoadScene("MainMenu");
        }
    }
}