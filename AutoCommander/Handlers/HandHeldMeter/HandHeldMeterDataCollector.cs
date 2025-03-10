﻿using System;
using System.Collections.Generic;

namespace AutoCommander.Handlers.HandHeldMeter;

public class HandHeldMeterDataCollector
{
    private readonly Collector<HandHeldMeterData> _collector;
    private readonly List<HandHeldMeterData> _records = new();

    public event Action Completed;
    public List<HandHeldMeterData> Records => _records;

    public HandHeldMeterDataCollector()
    {
        _collector = new()
        {
            Splitter = "\n",
            Parser = ParseText,
            Handler = data => _records.Add(data)
        };
    }

    public void Insert(string text) => _collector.Insert(text);

    private HandHeldMeterData ParseText(string text) =>
        text switch
        {
            string txt when txt.StartsWith("Export complete") => OnExportComplete(),
            _ => HandHeldMeterData.Parse(text)
        };

    private HandHeldMeterData OnExportComplete()
    {
        Completed?.Invoke();
        return null;
    }
}
