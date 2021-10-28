using JetBrains.Annotations;
using RiptideNetworking;
using UnityEngine;

namespace KarlsonMultiplayer
{
    public class ChangeVelocity : Command
    {
        public ChangeVelocity(string name) : base(name)
        {
        }

        public override void OnCommand([NotNull] string[] args)
        {
            if (args[3] != null)
            {
                
                Vector3 velocity = new Vector3(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]));
                
                if (ushort.TryParse(args[1], out var id))
                {
                    if (ServerPlayerManager.List.TryGetValue(id, out var player))
                    {
                        Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerVelocity);

                        message.Add(velocity);
                        
                        ServerNetworkManager.Singleton.Server.Send(message, player.serverClient);
                    }
                }

                ServerPlayer p;
                
                if ((p = ServerPlayerManager.FindPlayerByName(args[1])) != null)
                {
                    Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.playerVelocity);

                    message.Add(velocity);
                        
                    ServerNetworkManager.Singleton.Server.Send(message, p.serverClient);
                }
            }
        }
    }
}