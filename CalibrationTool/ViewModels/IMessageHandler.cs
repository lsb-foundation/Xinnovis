using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationTool.ViewModels
{
    public interface IMessageHandler<TMessage>
    {
        Action<TMessage> MessageHandler { get; set; }
    }
}
