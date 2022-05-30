using System;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer
{
    [BepInPlugin("me.p3rz3r0.karlsonmultiplayer", "KarlsonMultiplayer", "0.3.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static Main instance;

        public static Harmony harmony;

        private readonly GameObject clientGameObject = new GameObject("ClientNetworkManager");
        private GameObject clientNetworkObject;

        private string ip = "127.0.0.1";
        private string port = "8001";
        private string name = Environment.MachineName;
        private readonly GameObject serverGameObject = new GameObject("ServerNetworkManager");
        private GameObject serverNetworkObject;

        public string currentWeapon = "none";
        
        private readonly GameObject uiManagerObject = new GameObject("UIManager");
        private GameObject uiManager;
        
        private readonly GameObject prefabManagerObject = new GameObject("PrefabManager");
        private GameObject prefabManager;

        private void Awake()
        {
            instance = this;

            harmony = new Harmony("me.p3rz3r0.karlsonmultiplayer");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            RiptideLogger.Initialize(UnityEngine.Debug.Log, true);

            clientNetworkObject = Instantiate(clientGameObject, Vector3.zero, Quaternion.identity);
            clientNetworkObject.AddComponent<ClientNetworkManager>();

            serverNetworkObject = Instantiate(serverGameObject, Vector3.zero, Quaternion.identity);
            serverNetworkObject.AddComponent<ServerNetworkManager>();
            serverNetworkObject.AddComponent<CommandManager>();

            uiManager = Instantiate(uiManagerObject, Vector3.zero, Quaternion.identity);
            uiManager.AddComponent<UI.UI>();
            
            prefabManager = Instantiate(prefabManagerObject, Vector3.zero, Quaternion.identity);

            DontDestroyOnLoad(clientNetworkObject);
            DontDestroyOnLoad(serverNetworkObject);
            DontDestroyOnLoad(uiManager);
            DontDestroyOnLoad(prefabManager);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnityEngine.Debug.Log("Changing Scenes... Current Scene: " + scene.name);

            if (!PrefabManagerMP.instance)
            {
                if (!scene.name.Equals("0Tutorial"))
                {
                    SceneManager.LoadScene("0Tutorial");
                }
                else
                {
                    prefabManager.AddComponent<PrefabManagerMP>();
                    SceneManager.LoadScene(0);
                }
            } 
            
            if(mode == LoadSceneMode.Additive) return;
            
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.loadScene);
            message.Add(scene.name);
            ClientNetworkManager.Singleton.Client.Send(message);

            Message unequip = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerPickup);
            unequip.Add("none");
            ClientNetworkManager.Singleton.Client.Send(unequip);
        }
        
        private void FixedUpdate()
        {
            if (PlayerMovement.Instance)
            {
                SendPositionData();
                print("sending pos");

                if (PlayerMovement.Instance.HasGun())
                {
                    SendWeaponData();
                }
            }
        }

        private void Update()
        {
            foreach (var player in ClientPlayerManager.List)
            {
                player.Value.LerpPosition();
            }
        }

        public void SendWeaponData()
        {
            var weaponPosition =
                Message.Create(MessageSendMode.unreliable, (ushort) ClientToServerId.weaponPosition);

            weaponPosition.Add(PlayerMovement.Instance.gun.position);
            weaponPosition.Add(PlayerMovement.Instance.gun.rotation);

            ClientNetworkManager.Singleton.Client.Send(weaponPosition);
        }

        public void SendPositionData()
        {
            var message = Message.Create(MessageSendMode.unreliable, (ushort) ClientToServerId.playerPosRot);
            message.Add(PlayerMovement.Instance.transform.position);
            message.Add(PlayerMovement.Instance.orientation.rotation);
            ClientNetworkManager.Singleton.Client.Send(message);
        }

        private void OnGUI()
        {
            if (ClientNetworkManager.Singleton.Client.IsConnected) return;
            
            ip = GUILayout.TextField(ip);
            port = GUILayout.TextField(port);
            name = GUILayout.TextField(name);

            ClientNetworkManager.Singleton.ip = ip;
            ClientNetworkManager.Singleton.port = ushort.Parse(port);
            ClientNetworkManager.Singleton.name = name;
            ServerNetworkManager.Singleton.port = ushort.Parse(port);

            if (GUILayout.Button("Connect")) ClientNetworkManager.Singleton.Connect();

            if (GUILayout.Button("Create Server")) ServerNetworkManager.Singleton.StartServer();
        }

        public GameObject SpawnObject(GameObject obj, bool destroyOnLoad = false)
        {
            var go = Instantiate(obj);
            if(!destroyOnLoad) DontDestroyOnLoad(go);
            return go;
        }

        public void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }
        
        public void DestroyObject(Component obj)
        {
            Destroy(obj);
        }
    }

    [HarmonyPatch(typeof(DetectWeapons))]
    class DetectWeaponsPatch
    {
        [HarmonyPatch("Pickup")]
        [HarmonyPostfix]
        static void PostPickup(GameObject ___gun)
        {
            try
            {
                string gunName = ___gun.name.Replace("(Clone)", "");
                string[] gunNameStripped = gunName.Split(' ');
                
                Main.instance.currentWeapon = gunNameStripped[0];

                Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerPickup);
                message.Add(gunNameStripped[0]);
                ClientNetworkManager.Singleton.Client.Send(message);
                UnityEngine.Debug.Log("Changing guns... current gun: " + gunNameStripped[0]);
            }
            catch (NullReferenceException)
            {
                Main.instance.currentWeapon = "none";
            }
        }

        [HarmonyPatch("Throw")]
        [HarmonyPostfix]
        static void PostThrow()
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerPickup);
            message.Add("none");
            ClientNetworkManager.Singleton.Client.Send(message);
        }
    }
    
    [HarmonyPatch(typeof(RangedWeapon))]
    class RangedWeaponPatch
    {
        
        [HarmonyPatch("SpawnProjectile")]
        [HarmonyPostfix]
        static void PostShoot(Vector3 attackDirection, bool ___player)
        {
            if(!PlayerMovement.Instance.HasGun()) return;
            if (!___player) return;
            
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.weaponShoot);
            message.Add(attackDirection);
            ClientNetworkManager.Singleton.Client.Send(message);
        }
    }

    [HarmonyPatch(typeof(PlayerMovement))]
    class PlayerMovementPatch
    {
        [HarmonyPatch("StartCrouch")]
        [HarmonyPostfix]
        static void PostStartCrouch()
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerCrouchState);

            message.Add(true);
            
            ClientNetworkManager.Singleton.Client.Send(message);
        }
        
        [HarmonyPatch("StopCrouch")]
        [HarmonyPostfix]
        static void PostStopCrouch()
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerCrouchState);

            message.Add(false);
            
            ClientNetworkManager.Singleton.Client.Send(message);
        }
    }
}