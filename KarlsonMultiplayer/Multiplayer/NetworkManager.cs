using System;
using System.Linq;
using KarlsonMultiplayer.Multiplayer.Server;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer
{
    public enum ServerToClientId : ushort
    {
        spawnPlayer = 3,
        playerPosition,
    }

    public enum ClientToServerId : ushort
    {
        playerName = 1,
        position,
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
                    Main.instance.log.LogWarning("NetworkManager instance already exists");
                    Destroy(value);
                }
            }
        }

        public string ip;
        public ushort port;

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
            Client.Connect(ip, port, 1000, 2);
        }

        private void DidConnect(object sender, EventArgs e)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ClientToServerId.playerName);
            message.Add("test");
            Client.Send(message);
        }

        private void FailedToConnect(object sender, EventArgs e)
        {
            UnityEngine.Debug.Log("the connection bruhed");
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            Multiplayer.Client.Player.list[e.Id].RemovePlayer();
        }

        private void DidDisconnect(object sender, EventArgs e)
        {
            Main.instance.log.LogDebug("Disconnected");
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
                    Main.instance.log.LogWarning("NetworkManager instance already exists");
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
            foreach (var player in Player.List.Values.Where(player => player.id != e.Client.Id))
            {
                player.SendSpawn(e.Client);
            }
        }

        private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
        {
            Player.List.Remove(e.Id);
        }
    }
}