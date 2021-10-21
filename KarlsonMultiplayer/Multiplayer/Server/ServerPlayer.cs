using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer.Multiplayer.Server
{
    public class ServerPlayer
    {
        public Vector3 position;
        public Quaternion rotation;

        public ushort id { get; set; }
        public string username { get; set; }

        public string currentLevel = "MainMenu";

        public string currentWeapon = "none";
        public Vector3 weaponPosition;
        public Quaternion weaponRotation;

        public bool isCrouched;
        
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
        
        public void SendCurrentWeapon()
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerPickup);
            message.Add(id);
            message.Add(currentWeapon);
            ServerNetworkManager.Singleton.Server.SendToAll(message);
        }
        
        public void SendCurrentWeapon(ServerClient toClient)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerPickup);
            message.Add(id);
            message.Add(currentWeapon);
            ServerNetworkManager.Singleton.Server.Send(message, toClient);
        }

        public void SendCrouchState()
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerCrouchState);
            message.Add(id);
            message.Add(isCrouched);
            ServerNetworkManager.Singleton.Server.SendToAll(message);
        }
        
        public void SendCrouchState(ServerClient toClient)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerCrouchState);
            message.Add(id);
            message.Add(isCrouched);
            ServerNetworkManager.Singleton.Server.Send(message, toClient);
        }

        public void SetPosRot(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
            SendPosRot(position, rotation, id);
        }

        public void SetCrouchStateAndSend(bool crouchState)
        {
            isCrouched = crouchState;
            SendCrouchState();
        }

        public Message GetSpawnData(Message message)
        {
            message.Add(id);
            message.Add(username);
            message.Add(position);
            return message;
        }

        public Message GetWeaponData(Message message)
        {
            message.Add(weaponPosition);
            message.Add(weaponRotation);
            return message;
        }
    }
}