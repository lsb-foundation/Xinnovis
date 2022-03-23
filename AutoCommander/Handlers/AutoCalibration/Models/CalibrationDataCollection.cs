using System.Collections.Generic;
using System.Linq;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class CalibrationDataCollection : List<CalibrationData>
{
    public void Insert(CalibrationDataHolder holder)
    {
        if (this.FirstOrDefault(d => d.SerialNumber == holder.SerialNumber) is CalibrationData deviceData)
        {
            if (deviceData.FlowDatas.FirstOrDefault(f => f.Flow == holder.Flow) is CalibrationVoltData fd)
            {
                //存在流量数据，设置温度和事件差
                fd.Temperature = holder.Temperature;
                fd.TimespanSeconds = holder.TimespanSeconds;
            }
            else
            {
                //不存在流量数据，新增
                deviceData.FlowDatas.Add(new CalibrationVoltData
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
            deviceData = new CalibrationData { SerialNumber = holder.SerialNumber };
            deviceData.FlowDatas.Add(new CalibrationVoltData
            {
                Flow = holder.Flow,
                Temperature = holder.Temperature,
                TimespanSeconds = holder.TimespanSeconds
            });
            Add(deviceData);
        }
    }
}
