using LSMDatabase.Contracts.Databases;
using LSMDatabase.Contracts.MemoryTables;

namespace LSMDatabase.Contracts.WriteAheadLog;

/// <summary>
/// 日志
/// </summary>
public interface IWriteAheadLog : IDisposable
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    IDatabaseConfig DatabaseConfig { get; }

    /// <summary>
    /// 加载Wal日志到内存表
    /// </summary>
    /// <returns></returns>
    IMemoryTable LoadToMemory();

    /// <summary>
    /// 写日志
    /// </summary>
    void Write(IKeyValue data);

    /// <summary>
    /// 写日志
    /// </summary>
    void Write(List<IKeyValue> data);

    /// <summary>
    /// 重置日志文件
    /// </summary>
    void Reset();
}