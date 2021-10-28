using System;
using System.Linq;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer
{
    public enum ServerToClientId : ushort
    {
        spawnPlayer = 9,
        playerPosRot,
        playerScene,
        playerPickup,
        weaponPosition,
        weaponShoot,
        chatMessage,
        playerCrouchState,
        playerVelocity,
    }

    public enum ClientToServerId : ushort
    {
        playerName = 1,
        playerPosRot,
        loadScene,
        playerPickup,
        weaponPosition,
        weaponShoot,
        sendChatMessage,
        playerCrouchState,
    }
    
    public class ClientNetworkManager : MonoBehaviour
    {
        private static ClientNetworkManager _singleton;

        public static ClientNetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    UnityEngine.Debug.Log("NetworkManager instance already exists");
                    Destroy(value);
                }
            }
        }

        public string ip;
        public ushort port;
        public string name;

        public Client Client { get; private set; }

        public void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            Client = new Client();

            Client.Connected += DidConnect;
            Client.ConnectionFailed += FailedToConnect;
            Client.ClientDisconnected += PlayerLeft;
            Client.Disconnected += DidDisconnect;
        }

        private void FixedUpdate()
        {
            Client.Tick();
        }

        private void OnApplicationQuit()
        {
            Client.Disconnect();
            
            Client.Connected -= DidConnect;
            Client.ConnectionFailed -= FailedToConnect;
            Client.ClientDisconnected -= PlayerLeft;
            Client.Disconnected -= DidDisconnect;
        }

        public void Connect()
        {
            Client.Connect(ip, port);
        }

        private void DidConnect(object sender, EventArgs e)
        {
            Message name = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.playerName);
            name.Add(this.name);
            Client.Send(name);

            Message scene = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.loadScene);
            scene.Add(SceneManager.GetActiveScene().name);
            Client.Send(scene);
            
            Message weapon = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerPickup);
            scene.Add(Main.instance.currentWeapon);
            Client.Send(weapon);
            
            if(!PlayerMovement.Instance) return;
            Message crouch = Message.Create(MessageSendMode.reliable, (ushort) ClientToServerId.playerCrouchState);
            scene.Add(PlayerMovement.Instance.crouching);
            Client.Send(crouch);
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("the connection bruhed");
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            ClientPlayerManager.List[e.Id].RemovePlayer();
        }

        private void DidDisconnect(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("Disconnected");
            ClientPlayerManager.List.Clear();
        }
    }

    public class ServerNetworkManager : MonoBehaviour
    {
        private static ServerNetworkManager _singleton;

        public static ServerNetworkManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    UnityEngine.Debug.Log("NetworkManager instance already exists");
                    Destroy(value);
                }
            }
        }

        public ushort port;
        
        public Server Server { get; private set; }

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            Server = new Server();

            Server.ClientConnected += NewPlayerConnected;
            Server.ClientDisconnected += PlayerLeft;
        }

        private void FixedUpdate()
        {
            Server.Tick();
        }

        private void OnApplicationQuit()
        {
            Server.Stop();
            
            Server.ClientConnected -= NewPlayerConnected;
            Server.ClientDisconnected -= PlayerLeft;
        }

        public void StartServer()
        {
            Server.Start(port, 64);
        }

        private void NewPlayerConnected(object sender, ServerClientConnectedEventArgs e)
        {
            foreach (var player in ServerPlayerManager.List.Values.Where(player => player.id != e.Client.Id))
            {
                player.SendSpawn(e.Client);
                player.SendScene(e.Client);
                player.SendCurrentWeapon(e.Client);
                player.SendCrouchState(e.Client);
            }
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            ServerPlayerManager.List.Remove(e.Id);
        }
    }
}