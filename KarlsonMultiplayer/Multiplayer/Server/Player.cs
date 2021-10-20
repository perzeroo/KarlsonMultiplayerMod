using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer.Multiplayer.Server
{
    public class Player
    {
        public Vector3 position;
        
        public static Dictionary<ushort, Player> List { get; private set; } = new Dictionary<ushort, Player>();

        public ushort id { get; private set; }
        public string username { get; private set; }

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

        public void SendPosition(Vector3 position, ushort id)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientId.playerPosition);
            message.Add(id);
            message.Add(position);
            ServerNetworkManager.Singleton.Server.SendToAll(message);
        }

        public void SetPosition(Vector3 position, ushort id)
        {
            this.position = position;
            SendPosition(position, id);
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

        [MessageHandler((ushort) ClientToServerId.position)]
        public static void PlayerPosition(ServerClient fromClient, Message message)
        {
            Player player = List[fromClient.Id];
            Vector3 position = message.GetVector3();
            player.SetPosition(position, fromClient.Id);
        }
        
        #endregion
    }
}