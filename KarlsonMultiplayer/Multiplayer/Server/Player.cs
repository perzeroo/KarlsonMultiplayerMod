using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer.Multiplayer.Server
{
    public class Player
    {
        public Vector3 position;
        public Quaternion rotation;
        
        public static Dictionary<ushort, Player> List { get; private set; } = new Dictionary<ushort, Player>();

        public ushort id { get; private set; }
        public string username { get; private set; }

        public string currentLevel = "MainMenu";

        public static void Spawn(ushort id, string username)
        {
            Player player = new Player {username = username, id = id};

            player.SendSpawn();
            List.Add(player.id, player);
            UnityEngine.Debug.Log("Player " + username + " joined with the id " + id);
        }
        
        public void SendSpawn(ServerClient toClient)
        {
            ServerNetworkManager.Singleton.Server.Send(
                GetSpawnData(Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.spawnPlayer)), toClient);
        }
        
        public void SendSpawn()
        {
            ServerNetworkManager.Singleton.Server.SendToAll(
                GetSpawnData(Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.spawnPlayer)));
        }

        public void SendScene(ServerClient toClient)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerScene);
            message.Add(currentLevel);
            ServerNetworkManager.Singleton.Server.Send(message, toClient);
        }
        
        public void SendScene()
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerScene);
            message.Add(id);
            message.Add(currentLevel);
            ServerNetworkManager.Singleton.Server.SendToAll(message);
        }

        public void SendPosRot(Vector3 position, Quaternion rotation, ushort id)
        {
            Message message = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientId.playerPosRot);
            message.Add(id);
            message.Add(position);
            message.Add(rotation);
            ServerNetworkManager.Singleton.Server.SendToAll(message);
        }

        public void SetPosRot(Vector3 position, Quaternion rotation, ushort id)
        {
            this.position = position;
            this.rotation = rotation;
            SendPosRot(position, rotation, id);
        }
        
        private static void SetCurrentLevelAndSend(ServerClient fromClient, ushort fromClientId, string level)
        {
            if (List.TryGetValue(fromClientId, out var player))
            {
                player.currentLevel = level;
                player.SendScene();
            }
        }

        public Message GetSpawnData(Message message)
        {
            message.Add(id);
            message.Add(username);
            message.Add(position);
            return message;
        }

        #region Messages
        
        [MessageHandler((ushort) ClientToServerId.playerName)]
        public static void PlayerName(ServerClient fromClient, Message message)
        {
            Spawn(fromClient.Id, message.GetString());
        }

        [MessageHandler((ushort) ClientToServerId.playerPosRot)]
        public static void PlayerPosRot(ServerClient fromClient, Message message)
        {
            Player player = List[fromClient.Id];
            Vector3 position = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            player.SetPosRot(position, rotation, fromClient.Id);
        }

        [MessageHandler((ushort) ClientToServerId.loadScene)]
        public static void LoadScene(ServerClient fromClient, Message message)
        {
            SetCurrentLevelAndSend(fromClient, fromClient.Id, message.GetString());
        }

        #endregion
    }
}