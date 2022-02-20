using System.Collections.Generic;
using System.Linq;

namespace AutoCalibrationTool.Models
{
    public class DeviceDataCollection
    {
        public DeviceDataCollection()
        {
            HighTemperatureDatas = new List<DeviceData>();
            MidTemperatureDatas = new List<DeviceData>();
            LowTemperatureDatas = new List<DeviceData>();
        }

        public List<DeviceData> HighTemperatureDatas { get; }
        public List<DeviceData> MidTemperatureDatas { get; }
        public List<DeviceData> LowTemperatureDatas { get; }
        public bool IsEmpty => this.HighTemperatureDatas.Count == 0 && this.MidTemperatureDatas.Count == 0 && this.LowTemperatureDatas.Count == 0;

        public void Clear()
        {
            this.HighTemperatureDatas.Clear();
            this.MidTemperatureDatas.Clear();
            this.LowTemperatureDatas.Clear();
        }

        public int TotalDeviceCount()
        {
            return this.HighTemperatureDatas.Select(htd => htd.DeviceCode)
                    .Union(this.MidTemperatureDatas.Select(mtd => mtd.DeviceCode))
                    .Union(this.LowTemperatureDatas.Select(ltd => ltd.DeviceCode))
                    .Count();
        }

        public int TotalFlowCount()
        {
            return this.HighTemperatureDatas.Sum(htd => htd.FlowDatas.Count) +
                this.MidTemperatureDatas.Sum(mtd => mtd.FlowDatas.Count) +
                this.LowTemperatureDatas.Sum(ltd => ltd.FlowDatas.Count);
        }

        public void SetValue(TemperatureType tempType, ValueType valueType, string deviceCode, float flow, float value)
        {
            var deviceDatas = tempType == TemperatureType.High ? this.HighTemperatureDatas :
                (tempType == TemperatureType.Mid ? this.MidTemperatureDatas :
                (tempType == TemperatureType.Low ? this.LowTemperatureDatas : null));
            
            if (deviceDatas.FirstOrDefault(d => d.DeviceCode == deviceCode) is DeviceData deviceData)
            {
                if (deviceData.FlowDatas.FirstOrDefault(f => f.Flow == flow) is FlowTemperatureVolts fd)
                {
                    fd.SetValue(valueType, value);
                }
                else
                {
                    var flowData = new FlowTemperatureVolts { Flow = flow };
                    flowData.SetValue(valueType, value);
                    deviceData.FlowDatas.Add(flowData);
                }
            }
            else
            {
                var flowData = new FlowTemperatureVolts { Flow = flow };
                flowData.SetValue(valueType, value);
                var newDeviceData = new DeviceData { DeviceCode = deviceCode };
                newDeviceData.FlowDatas.Add(flowData);
                deviceDatas.Add(newDeviceData);
            }
        }
    }
}
