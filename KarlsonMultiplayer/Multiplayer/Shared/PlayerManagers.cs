using System.Collections.Generic;
using KarlsonMultiplayer.Multiplayer.Client;
using KarlsonMultiplayer.Multiplayer.Server;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer.Shared
{
    public class ClientPlayerManager
    {
        public static Dictionary<ushort, ClientPlayer> List = new Dictionary<ushort, ClientPlayer>();
        
        public static ClientPlayer FindPlayerByName(string name)
        {
            foreach (var player in List)
            {
                if (player.Value.username.Equals(name))
                    return player.Value;
            }

            return null;
        }
        
        public static void Spawn(ushort id, string username, Vector3 position)
        {
            if (id == ClientNetworkManager.Singleton.Client.Id)
                return;

            ClientPlayer player = new ClientPlayer
            {
                username = username,
                id = id,
                playerObject = Main.instance.SpawnObject(GameObject.CreatePrimitive(PrimitiveType.Capsule))
            };
            player.playerObject.transform.position = position;
            player.playerObject.transform.localScale = new Vector3(1, 1.5f, 1);
            player.playerObject.name = username;
            player.playerObject.layer = 10;

            var usernameText = Main.instance.SpawnObject(new GameObject("Text"));

            usernameText.AddComponent<TextMesh>();
            usernameText.GetComponent<TextMesh>().text = username;
            usernameText.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            usernameText.GetComponent<TextMesh>().fontSize = 32;
            usernameText.GetComponent<TextMesh>().characterSize = 0.1f;
            usernameText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            usernameText.transform.parent = player.playerObject.transform;
            usernameText.transform.localPosition = new Vector3(0, 1.5f, 0);
            usernameText.transform.localRotation = Quaternion.Euler(0, 180, 0);

            var glasses = Main.instance.SpawnObject(GameObject.CreatePrimitive(PrimitiveType.Cube));

            glasses.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            glasses.transform.parent = player.playerObject.transform;
            glasses.transform.localPosition = new Vector3(0, 0.65f, 0.22f);
            glasses.transform.localScale = new Vector3(0.8f, 0.2f, 0.8f);

            List.Add(id, player);
        }
        
    }

    public class ServerPlayerManager
    {
        public static Dictionary<ushort, ServerPlayer> List { get; set; } = new Dictionary<ushort, ServerPlayer>();
        
        public static void Spawn(ushort id, string username)
        {
            ServerPlayer player = new ServerPlayer {username = username, id = id};

            player.SendSpawn();
            List.Add(player.id, player);
            UnityEngine.Debug.Log("Player " + username + " joined with the id " + id);
        }
        
        public static void SetCurrentWeaponAndSend(ushort fromClientId, string weapon)
        {
            if (List.TryGetValue(fromClientId, out var player))
            {
                player.currentWeapon = weapon;
                player.SendCurrentWeapon();
            }
        }
        
        public static void SetCurrentLevelAndSend(ushort fromClientId, string level)
        {
            if (List.TryGetValue(fromClientId, out var player))
            {
                player.currentLevel = level;
                player.SendScene();
            }
        }

        public static void SetWeaponPosRotAndSend(ushort fromClientId, Vector3 vector3, Quaternion quaternion)
        {
            if (List.TryGetValue(fromClientId, out var player))
            {
                player.weaponPosition = vector3;
                player.weaponRotation = quaternion;
                
                Message message = Message.Create(MessageSendMode.unreliable, (ushort) ServerToClientId.weaponPosition);
                message.Add(fromClientId);
                ServerNetworkManager.Singleton.Server.SendToAll(player.GetWeaponData(message));
            }
        }
    }
}