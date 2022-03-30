using System.Collections.Generic;
using System.Linq;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class CollectionContainer
{
    public List<CalibrationData> HighCollection { get; }
    public List<CalibrationData> MidCollection { get; }
    public List<CalibrationData> LowCollection { get; }

    public CollectionContainer()
    {
        HighCollection = new();
        MidCollection = new();
        LowCollection = new();
    }

    public void Insert(DataContainer data)
    {
        var collection = data.TemperatureType switch
        {
            TemperatureType.High => HighCollection,
            TemperatureType.Mid => MidCollection,
            TemperatureType.Low => LowCollection,
            _ => null
        };

        if (collection is null) return;

        if (collection.FirstOrDefault(c => c.SerialNumber == data.SerialNumber) is CalibrationData caliData)
        {
            if (caliData.FlowDatas.FirstOrDefault(f => f.Flow == data.Flow) is CalibrationVolts fd)
            {
                fd.Set(data);
            }
            else
            {
                //不存在流量数据，新增
                var voltData = new CalibrationVolts();
                voltData.Set(data);
                caliData.FlowDatas.Add(voltData);
            }
        }
        else
        {
            //不存在设备数据，新增
            var voltData = new CalibrationVolts();
            voltData.Set(data);
            caliData = new CalibrationData { SerialNumber = data.SerialNumber };
            caliData.FlowDatas.Add(voltData);
            collection.Add(caliData);
        }
    }
}
