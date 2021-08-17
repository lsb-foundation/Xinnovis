namespace AutoCommander.UIModels
{
    public interface IAutoBuilder { }

    public interface IAutoBuild<T> : IAutoBuilder
    {
        T Build();
    }
}
