using System.Collections.Generic;
using UnityEngine;

namespace KarlsonMultiplayer
{
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager instance;
        public List<Command> commands = new List<Command>();

        public void Awake()
        {
            instance = this;
            
            SetupCommands();
        }
        
        public void SetupCommands()
        {
            commands.Add(new ChangeVelocity("changevel"));
        }

        public void HandleCommand(string commandString)
        {
            UnityEngine.Debug.Log(commandString);
            
            string[] cmd = commandString.Substring(1).Split(' ');

            foreach (var command in commands)
            {
                if (command.name.Equals(cmd[0].ToLower()))
                {
                    command.OnCommand(commandString.Substring(1).Substring(cmd[0].Length).Split(' '));
                }
            }
        }
    }
}