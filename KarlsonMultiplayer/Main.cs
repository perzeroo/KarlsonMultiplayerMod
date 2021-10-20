using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using KarlsonMultiplayer.Multiplayer;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer
{
    [BepInPlugin("me.p3rz3r0.karlsonmultiplayer", "KarlsonMultiplayer", "0.1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static Main instance;

        public static Harmony harmony;

        private readonly GameObject clientGameObject = new GameObject("ClientNetworkManager");
        private GameObject clientNetworkObject;

        private string ip = "127.0.0.1";
        private string port = "8001";
        private readonly GameObject serverGameObject = new GameObject("ServerNetworkManager");
        private GameObject serverNetworkObject;

        private void Awake()
        {
            instance = this;

            harmony = new Harmony("me.p3rz3r0.karlsonmultiplayer");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Start()
        {
            RiptideLogger.Initialize(UnityEngine.Debug.Log, true);

            clientNetworkObject = Instantiate(clientGameObject, Vector3.zero, Quaternion.identity);
            clientNetworkObject.AddComponent<ClientNetworkManager>();

            serverNetworkObject = Instantiate(serverGameObject, Vector3.zero, Quaternion.identity);
            serverNetworkObject.AddComponent<ServerNetworkManager>();

            DontDestroyOnLoad(clientNetworkObject);
            DontDestroyOnLoad(serverNetworkObject);
        }

        private void FixedUpdate()
        {
            if (PlayerMovement.Instance)
            {
                var message = Message.Create(MessageSendMode.unreliable, (ushort) ClientToServerId.playerPosRot);
                message.Add(PlayerMovement.Instance.transform.position);
                message.Add(PlayerMovement.Instance.orientation.rotation);
                ClientNetworkManager.Singleton.Client.Send(message);
            }
        }

        private void OnGUI()
        {
            ip = GUILayout.TextField(ip);
            port = GUILayout.TextField(port);

            ClientNetworkManager.Singleton.ip = ip;
            ClientNetworkManager.Singleton.port = ushort.Parse(port);
            ServerNetworkManager.Singleton.port = ushort.Parse(port);

            if (GUILayout.Button("Connect")) ClientNetworkManager.Singleton.Connect();

            if (GUILayout.Button("Create Server")) ServerNetworkManager.Singleton.StartServer();
        }

        public GameObject SpawnObject(GameObject obj)
        {
            var go = Instantiate(obj);
            DontDestroyOnLoad(go);
            return go;
        }

        public void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
    
    [HarmonyPatch(typeof(Lobby), "LoadMap")]
    class LobbyPatch
    {
        [HarmonyPostfix]
        static void PostLoadMap(string s)
        {
            UnityEngine.Debug.Log(s);
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.loadScene);
            message.Add(s);
            ClientNetworkManager.Singleton.Client.Send(message);
        }
    }
}