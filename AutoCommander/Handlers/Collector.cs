using System;
using System.Text;

namespace AutoCommander.Handlers;

public class Collector<T> where T : class
{
    private ReadOnlyMemory<char> splitter = "\n".AsMemory();
    private readonly StringBuilder _container = new();

    public Func<string, T> Parser { get; set; }
    public Action<T> Handler { get; set; }

    public string Splitter
    {
        get => splitter.ToString();
        set => splitter = value.AsMemory();
    }

    public void Insert(string text)
    {
        int index;
        ReadOnlyMemory<char> memory = text.AsMemory();
        while ((index = memory.Span.IndexOf(splitter.Span)) > -1)
        {
            _container.Append(memory.Slice(0, index));
            T data = Parser?.Invoke(_container.ToString());
            if (data is not null) Handler?.Invoke(data);

            int startIndex = index + splitter.Length;
            memory = startIndex < memory.Length ?
                memory.Slice(startIndex) : ReadOnlyMemory<char>.Empty;

            _container.Clear();
        }
        _container.Append(memory);
    }
}
