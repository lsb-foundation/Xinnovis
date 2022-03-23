using AutoCommander.Handlers.AutoCalibration.Extensions;
using AutoCommander.Handlers.AutoCalibration.Models;
using System;

namespace AutoCommander.Handlers.AutoCalibration;

public class AutoCalibrationCollector
{
    private readonly Collector<CalibrationDataHolder> _collector;
    private readonly CalibrationDataCollection _collection = new();

    public event Action Completed;
    public CalibrationDataCollection Collection => _collection;

    public AutoCalibrationCollector()
    {
        _collector = new()
        {
            Splitter = "\r\n",
            Parser = ParseText,
            Handler = data => _collection.Insert(data)
        };
    }

    public void Insert(string text) => _collector.Insert(text);

    private CalibrationDataHolder ParseText(string text)
    {
        if (text.StartsWith("MID") || text.StartsWith("HIGH") || text.StartsWith("LOW"))
        {
            CalibrationDataHolder holder = new();
            foreach (string kvPair in text.Split(';'))
            {
                string[] kv = kvPair.Split(':');
                if (kv.Length != 2) continue;
                holder.SetValueForType(kv[0].CalibrationValueType(), kv[1]);
            }
            return holder;
        }
        else if (text.Equals("CALIBRATION_OVER!"))  //标定结束
        {
            Completed?.Invoke();
        }
        return null;
    }
}
