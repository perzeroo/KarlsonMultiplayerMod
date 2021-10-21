using System.Collections.Generic;
using KarlsonMultiplayer.Multiplayer;
using KarlsonMultiplayer.Multiplayer.Server;
using RiptideNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KarlsonMultiplayer.Shared
{
    public class ServerHandlers
    {
        #region ServerMessages
        
        [MessageHandler((ushort) ClientToServerId.playerName)]
        public static void PlayerName(ServerClient fromClient, Message message)
        {
            ServerPlayerManager.Spawn(fromClient.Id, message.GetString());
        }

        [MessageHandler((ushort) ClientToServerId.playerPosRot)]
        public static void PlayerPosRot(ServerClient fromClient, Message message)
        {
            ServerPlayer player = ServerPlayerManager.List[fromClient.Id];
            Vector3 position = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            player.SetPosRot(position, rotation);
        }

        [MessageHandler((ushort) ClientToServerId.loadScene)]
        public static void LoadScene(ServerClient fromClient, Message message)
        {
            ServerPlayerManager.SetCurrentLevelAndSend(fromClient.Id, message.GetString());
        }

        [MessageHandler((ushort) ClientToServerId.playerPickup)]
        public static void PlayerPickup(ServerClient fromClient, Message message)
        {
            ServerPlayerManager.SetCurrentWeaponAndSend(fromClient.Id, message.GetString());
        }

        [MessageHandler((ushort) ClientToServerId.weaponPosition)]
        public static void WeaponPosition(ServerClient fromClient, Message message)
        {
            ServerPlayerManager.SetWeaponPosRotAndSend(fromClient.Id, message.GetVector3(), message.GetQuaternion());
        }

        [MessageHandler((ushort) ClientToServerId.weaponShoot)]
        public static void WeaponShoot(ServerClient fromClient, Message message)
        {
            Message message2 = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.weaponShoot);
            message2.Add(fromClient.Id);
            message2.Add(message.GetVector3());
            ServerNetworkManager.Singleton.Server.SendToAll(message2);
        }

        [MessageHandler((ushort) ClientToServerId.playerCrouchState)]
        public static void PlayerCrouchState(ServerClient fromClient, Message message)
        {
            if (ServerPlayerManager.List.TryGetValue(fromClient.Id, out var player))
            {
                player.SetCrouchStateAndSend(message.GetBool());
            }
        }
        
        #endregion
    }

    public class ClientHandlers
    {
        [MessageHandler((ushort) ServerToClientId.spawnPlayer)]
        public static void SpawnPlayer(Message message)
        {
            ClientPlayerManager.Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
        }

        [MessageHandler((ushort) ServerToClientId.playerPosRot)]
        public static void PlayerPosRot(Message message)
        {
            var playerId = message.GetUShort();
            if (ClientPlayerManager.List.TryGetValue(playerId, out var player))
                player.Move(message.GetVector3(), message.GetQuaternion());
        }

        [MessageHandler((ushort) ServerToClientId.playerScene)]
        public static void PlayerScene(Message message)
        {
            ushort id = message.GetUShort();
            string currScene = message.GetString();
            
            if (ClientPlayerManager.List.TryGetValue(id, out var player))
                player.currentLoadedScene = currScene;
        }

        [MessageHandler((ushort) ServerToClientId.playerPickup)]
        public static void PlayerPickup(Message message)
        {
            ushort id = message.GetUShort();
            string currWep = message.GetString();

            if(ClientNetworkManager.Singleton.Client.Id == id) return;
            
            if (ClientPlayerManager.List.TryGetValue(id, out var player))
            {
                player.currentWeapon = currWep;

                if (currWep.Equals("none"))
                {
                    if(!player.weaponObject) return;
                    
                    Main.instance.DestroyObject(player.weaponObject);
                    return;
                }

                if (SceneManager.GetActiveScene().name.Equals(player.currentLoadedScene))
                {
                    player.SpawnWeapon();
                }
            }
                
        }

        [MessageHandler((ushort) ServerToClientId.weaponPosition)]
        public static void WeaponPosition(Message message)
        {
            ushort id = message.GetUShort();
            Vector3 position = message.GetVector3();
            Quaternion rotation = message.GetQuaternion();
            
            if(ClientNetworkManager.Singleton.Client.Id == id) return;

            if (ClientPlayerManager.List.TryGetValue(id, out var player))
            {
                if (player.weaponObject)
                {
                    player.weaponObject.transform.position = position;
                    player.weaponObject.transform.rotation = rotation;
                }
            }
        }

        [MessageHandler((ushort) ServerToClientId.weaponShoot)]
        public static void WeaponShoot(Message message)
        {
            ushort id = message.GetUShort();
            Vector3 position = message.GetVector3();
            
            if(ClientNetworkManager.Singleton.Client.Id == id) return;

            if (ClientPlayerManager.List.TryGetValue(id, out var player))
            {
                if (player.weaponObject)
                {
                    player.ShootProjectile(position);
                }
                else
                {
                    if (player.currentLoadedScene.Equals(SceneManager.GetActiveScene().name))
                    {
                        player.SpawnWeapon();
                    }
                }
            }
        }

        [MessageHandler((ushort) ServerToClientId.playerCrouchState)]
        public static void PlayerCrouchState(Message message)
        {
            ushort id = message.GetUShort();
            bool crouchState = message.GetBool();
            
            if(ClientNetworkManager.Singleton.Client.Id == id) return;

            if (ClientPlayerManager.List.TryGetValue(id, out var player))
            {
                player.Crouch(crouchState);
            }
        }
    }
}