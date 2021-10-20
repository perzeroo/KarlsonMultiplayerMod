using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer.Multiplayer.Client
{
    public class Player
    {

        public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
        
        private ushort id;
        private string username;

        private GameObject playerObject;

        public void Move(Vector3 newPos)
        {
            playerObject.transform.position = newPos;
        }

        public static void Spawn(ushort id, string username, Vector3 position)
        {
            if(id == ClientNetworkManager.Singleton.Client.Id)
                return;

            Player player = new Player
            {
                username = username, id = id
            };

            player.playerObject = Main.instance.SpawnObject(GameObject.CreatePrimitive(PrimitiveType.Capsule));
            player.playerObject.transform.position = position;
            
            list.Add(id, player);
        }

        public void RemovePlayer()
        {
            Main.instance.DestroyObject(playerObject);
            list.Remove(id);
        }

        #region Messages
        
        [MessageHandler((ushort) ServerToClientId.spawnPlayer)]
        public static void SpawnPlayer(Message message)
        {
            Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }
        
        [MessageHandler((ushort) ServerToClientId.playerPosition)]
        public static void PlayerPosition(Message message)
        {
            
            var playerId = message.GetUShort();
            if (list.TryGetValue(playerId, out var player))
                player.Move(message.GetVector3());
        }

        #endregion
    }
}