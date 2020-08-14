using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace SerialDataDisplay
{
    public class SerialCommand
    {
        public SendType SendType { get; }
        public CommandType CommandType { get; }

        public string Unit { get; set; }
        public string YAxisTitle { get => $"{CommandName}({Unit})"; }

        public object StartCommand { get; set; }
        public object StopCommand { get; set; }

        public SerialCommand(SendType sendType, CommandType commandType)
        {
            SendType = sendType;
            CommandType = commandType;
        }

        public string CommandName 
        { 
            get
            {
                var name = Enum.GetName(typeof(SendType), SendType);
                if (!string.IsNullOrEmpty(name))
                {
                    var field = typeof(SendType).GetField(name);
                    if (field != null)
                    {
                        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                            return attr.Description;
                    }
                }
                return string.Empty;
            } 
        }

        public static List<SerialCommand> GetCommands()
        {
            var ret = new List<SerialCommand>();
            var caliCommand = new SerialCommand(SendType.CaliFlowV, CommandType.Ascii)
            {
                StartCommand = "CALI_FLOW_V!",
                StopCommand = "CALI_FLOW_V!",
                Unit = "mV"
            };
            var windSpeedCommand = new SerialCommand(SendType.WindSpeed, CommandType.Hex)
            {
                StartCommand = new byte[] { 0xF1 },
                StopCommand = new byte[] { 0xED },
                Unit = "m/s"
            };
            var originOfWindSpeedCommand = new SerialCommand(SendType.OriginDataOfWindSpeed, CommandType.Hex)
            {
                StartCommand = new byte[] { 0xCA },
                StopCommand = new byte[] { 0xED },
                Unit = "mV"
            };
            var temperatureCommand = new SerialCommand(SendType.Temperature, CommandType.Hex)
            {
                StartCommand = new byte[] { 0xF2 },
                StopCommand = new byte[] { 0xED },
                Unit = "℃"
            };
            var originOfTemperatureCommand = new SerialCommand(SendType.OriginDataOfTemperature, CommandType.Hex)
            {
                StartCommand = new byte[] { 0xF3 },
                StopCommand = new byte[] { 0xED },
                Unit = "mV"
            };

            ret.Add(caliCommand);
            ret.Add(windSpeedCommand);
            ret.Add(originOfWindSpeedCommand);
            ret.Add(temperatureCommand);
            ret.Add(originOfTemperatureCommand);

            return ret;
        }
    }
}
