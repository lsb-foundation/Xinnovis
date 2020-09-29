
namespace MFCSoftware.Models
{
    public class SerialCommand<T>
    {
        public T Command { get; }
        public int ResponseLength { get; }

        public SerialCommand(T command, int responseLength)
        {
            Command = command;
            ResponseLength = responseLength;
        }
    }
}
