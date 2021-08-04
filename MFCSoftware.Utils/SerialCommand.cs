using CommonLib.Extensions;
using System.Reflection;

namespace MFCSoftware.Utils
{
    public class SerialCommand<T>
    {
        public SerialCommandType Type { get; }
        public T Command { get; }
        public int ResponseLength { get; }

        public SerialCommand(T command, int responseLength)
        {
            Command = command;
            ResponseLength = responseLength;
        }

        public SerialCommand(T command, SerialCommandType type, int responseLength)
        {
            Command = command;
            Type = type;
            ResponseLength = responseLength;
        }

        public override string ToString()
        {
            if (typeof(SerialCommandType).GetField(Type.ToString()).GetCustomAttribute<ResolveActionAttribute>() is ResolveActionAttribute attr)
            {
                string command = (Command is byte[]) ? (Command as byte[]).ToHexString() : Command.ToString();
                return $"Type: {attr.ActionName}\tCommand: {command}\tResponseLength: {ResponseLength}";
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
