using LSMDatabase.Contracts.Databases;

namespace LSMDatabase.MemoryTables;

public class MemoryTableValue : IDisposable
{
    public long Time { get; set; } = IDHelper.MarkID();

    /// <summary>
    /// 是否是不可变
    /// </summary>
    public bool Immutable { get; set; } = false;

    /// <summary>
    /// 数据
    /// </summary>
    public Dictionary<string, IKeyValue> Dic { get; set; } = new();

    public void Dispose()
    {
        Dic.Clear();
    }

    public override string ToString()
    {
        return $"Time {Time} Immutable：{Immutable}";
    }
}