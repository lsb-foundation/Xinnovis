using System;

namespace AutoCommander.AutoUI.Linkers;

public interface IActionHandler
{
    string Command { get; set; }

    event Action Completed;

    void Initialize();

    void Execute();

    void Receive(string text);
}
