using System.Collections.Generic;
using System.Linq;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class DeviceDataList : List<DeviceData>
{
    public void Insert(DataHolder holder)
    {
        if (this.FirstOrDefault(d => d.SerialNumber == holder.Device) is DeviceData deviceData)
        {
            if (deviceData.FlowDatas.FirstOrDefault(f => f.Flow == holder.Flow) is VoltData fd)
            {
                //存在流量数据，设置温度和事件差
                fd.Temperature = holder.Temperature;
                fd.TimespanSeconds = holder.TimespanSeconds;
            }
            else
            {
                //不存在流量数据，新增
                deviceData.FlowDatas.Add(new VoltData
                {
                    Flow = holder.Flow,
                    Temperature = holder.Temperature,
                    TimespanSeconds = holder.TimespanSeconds
                });
            }
        }
        else
        {
            //不存在设备数据，新增
            deviceData = new DeviceData { SerialNumber = holder.Device };
            deviceData.FlowDatas.Add(new VoltData
            {
                Flow = holder.Flow,
                Temperature = holder.Temperature,
                TimespanSeconds = holder.TimespanSeconds
            });
            Add(deviceData);
        }
    }
}
