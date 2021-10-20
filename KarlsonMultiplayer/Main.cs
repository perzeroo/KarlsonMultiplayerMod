using System;
using BepInEx;
using BepInEx.Logging;
using KarlsonMultiplayer.Multiplayer;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer
{
    [BepInPlugin("me.p3rz3r0.karlsonmultiplayer", "KarlsonMultiplayer", "0.1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static Main instance;

        public ManualLogSource log;

        private GameObject clientGameObject = new GameObject("ClientNetworkManager");
        private GameObject serverGameObject = new GameObject("ServerNetworkManager");
        private GameObject clientNetworkObject;
        private GameObject serverNetworkObject;

        private void Awake()
        {
            instance = this;

            log = new ManualLogSource("KarlsonMultiplayer");
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
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.position);
                message.Add(PlayerMovement.Instance.transform.position);
                ClientNetworkManager.Singleton.Client.Send(message);
        }

        private string ip = "127.0.0.1";
        private string port = "8001";

        private void OnGUI()
        {
            ip = GUILayout.TextField(ip);
            port = GUILayout.TextField(port);

            ClientNetworkManager.Singleton.ip = ip;
            ClientNetworkManager.Singleton.port = ushort.Parse(port);
            ServerNetworkManager.Singleton.port = ushort.Parse(port);

            if (GUILayout.Button("Connect"))
            {
                ClientNetworkManager.Singleton.Connect();
            }

            if (GUILayout.Button("Create Server"))
            {
                ServerNetworkManager.Singleton.StartServer();
            }
        }

        public GameObject SpawnObject(GameObject obj)
        {
            GameObject go = Instantiate(obj);
            DontDestroyOnLoad(go);
            return go;
        }

        public void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}