using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using Newtonsoft.Json;

namespace APG.Common.Commands
{
    [Serializable]
    public class Command
    {
        [JsonIgnore] private Guid id;
        [JsonIgnore] public Guid ID { get => id; }

        public string Name;

        public bool IsRequestFromGame = false;
        public string RequestMessage = string.Empty;

        public bool HasCooldown = true;
        public float CooldownTime = 1;

        public bool SendScreenshot;
        public float ScreenshotDelay = 0;

        public CommandParameter[] Parameters;

        [JsonIgnore]
        private DateTime lastTimeIssued = DateTime.Now;

        public Command()
        {
            id = Guid.NewGuid();
        }

        public bool HasFinishedCooldown(out double timeRemaining)
        {
            var timeDiff  = DateTime.Now.Subtract(lastTimeIssued).TotalSeconds;

            if (timeDiff > CooldownTime)
            {
                lastTimeIssued = DateTime.Now;
                timeRemaining = 0;
                return true;
            }

            timeRemaining = CooldownTime - timeDiff;
            return false;
        }



    }

    [Serializable]
    public class CommandParameter
    {
        public string Name;
        public ParameterType Type;
        public bool IsRequired = false;
        public string DefaultValue;
        public string Value = string.Empty;


        public string GetString() => Value;
        public int GetInt() => int.Parse(Value);
        public bool GetBool() => bool.Parse(Value);

        public bool IsSameType(string value)
        {
            switch (Type)
            {
                case ParameterType.String:
                    return true;

                case ParameterType.Int:
                    return int.TryParse(value, out int i);

                case ParameterType.Bool:
                    return bool.TryParse(value, out var b);
            }

            return false;
        }

    }

    public enum ParameterType
    {
        String,
        Int,
        Bool
    }
}
