using System;

namespace AutoCommander.AutoUI.Linkers;

public interface IActionHandler
{
    event Action Completed;

    void Initialize();

    void Receive(string text);
}
