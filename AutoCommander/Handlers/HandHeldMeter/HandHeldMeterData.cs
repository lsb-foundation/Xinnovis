using System;

namespace AutoCommander.Handlers.HandHeldMeter;

public class HandHeldMeterData
{
    public int Floor { get; set; }
    public string Department { get; set; }
    public int Room { get; set; }
    public int BedNumber { get; set; }
    public float Flow { get; set; }
    public float Pressure { get; set; }
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public DateTime RecordTime { get; set; }

    public static HandHeldMeterData Parse(string text)
    {
        var parts = text.Split(';');
        if (!text.StartsWith("A") || parts.Length != 9) return null;
        var data = new HandHeldMeterData
        {
            Floor = int.Parse(parts[0].Substring(1)),
            Department = parts[1].Substring(1),
            Room = int.Parse(parts[2].Substring(1)),
            BedNumber = int.Parse(parts[3].Substring(1)),
            Flow = float.Parse(parts[4].Substring(1)),
            Pressure = float.Parse(parts[5].Substring(1)),
            Temperature = float.Parse(parts[6].Substring(1)),
            Humidity = float.Parse(parts[7].Substring(1))
        };
        var datetime = parts[8].Split(',');
        var date = DateTime.Parse(datetime[0]);
        var time = DateTime.Parse(datetime[1]);
        data.RecordTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        return data;
    }
}
