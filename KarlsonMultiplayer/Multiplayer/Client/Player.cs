using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer.Multiplayer.Client
{
    public class Player
    {
        public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

        private ushort id;
        private string username;

        private GameObject playerObject;

        private string currentLoadedScene;

        public static Player FindPlayerByName(string name)
        {
            foreach (var player in list)
            {
                if (player.Value.username.Equals(name))
                    return player.Value;
            }

            return null;
        }

        public void Move(Vector3 newPos, Quaternion newRot)
        {
            if (!SceneManager.GetActiveScene().name.Equals(currentLoadedScene))
            {
                newPos.y += 9999999;
            }
            
            playerObject.transform.position = newPos;
            playerObject.transform.rotation = newRot;
        }

        public static void Spawn(ushort id, string username, Vector3 position)
        {
            if (id == ClientNetworkManager.Singleton.Client.Id)
                return;

            Player player = new Player
            {
                username = username, id = id
            };
            player.playerObject = Main.instance.SpawnObject(GameObject.CreatePrimitive(PrimitiveType.Capsule));
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

        [MessageHandler((ushort) ServerToClientId.playerPosRot)]
        public static void PlayerPosRot(Message message)
        {
            var playerId = message.GetUShort();
            if (list.TryGetValue(playerId, out var player))
                player.Move(message.GetVector3(), message.GetQuaternion());
        }

        [MessageHandler((ushort) ServerToClientId.playerScene)]
        public static void PlayerScene(Message message)
        {
            ushort id = message.GetUShort();
            string currScene = message.GetString();
            
            if (list.TryGetValue(id, out var player))
                player.currentLoadedScene = currScene;
        }

        #endregion
    }
}