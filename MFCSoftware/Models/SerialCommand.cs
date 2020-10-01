
namespace MFCSoftware.Models
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
    }
}
