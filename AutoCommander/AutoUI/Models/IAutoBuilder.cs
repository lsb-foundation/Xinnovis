namespace AutoCommander.AutoUI.Models;

public interface IAutoBuilder { }

public interface IAutoBuild<T> : IAutoBuilder
{
    T Build();
}
