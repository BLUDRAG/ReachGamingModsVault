using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BossRaids.Scripts
{
    public class CommandInterpreter : ConsoleCmdAbstract
    {
        private static CommandInterpreterRunner _runner;
        
        public static void Init()
        {
            GameObject sceneObject = new GameObject($"{Assembly.GetExecutingAssembly().FullName} - Command Interpreter");
            Object.DontDestroyOnLoad(sceneObject);
            _runner = sceneObject.AddComponent<CommandInterpreterRunner>();
        }

        protected override string[] getCommands()
        {
            return new[]
                   {
                       "bossraids"
                   };
        }

        protected override string getDescription()
        {
            return "Allows the spawning of raid bosses from the console.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            if(_params.Count == 0)
            {
                Log.Out("Usage: bossraids spawn random");
                return;
            }

            if(_params[0] != "spawn") return;

            if(_params.Count == 1)
            {
                Log.Out("Usage: bossraids spawn random");
                return;
            }

            if(_params[1] == "random")
            {
                _runner.StartCoroutine(GameInteractions.SpawnRandomBoss());
            }
        }
    }

    public class CommandInterpreterRunner : MonoBehaviour
    {
    }
}