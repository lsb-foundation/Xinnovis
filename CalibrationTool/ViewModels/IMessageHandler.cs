using System;

namespace CalibrationTool.ViewModels
{
    public interface IMessageHandler<TMessage>
    {
        Action<TMessage> MessageHandler { get; set; }
    }
}
