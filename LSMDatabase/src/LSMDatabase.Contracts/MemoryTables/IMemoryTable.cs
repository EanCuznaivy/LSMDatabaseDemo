using LSMDatabase.Contracts.Databases;

namespace LSMDatabase.Contracts.MemoryTables;

/// <summary>
/// 内存表(排序树，二叉树)
/// </summary>
public interface IMemoryTable : IDisposable
{
    IDatabaseConfig DatabaseConfig { get; }

    /// <summary>
    /// 获取总数
    /// </summary>
    int GetCount();

    /// <summary>
    /// 搜索(从新到旧，从大到小)
    /// </summary>
    IKeyValue Search(string key);

    /// <summary>
    /// 设置新值
    /// </summary>
    void Set(IKeyValue keyValue);

    /// <summary>
    /// 删除key
    /// </summary>
    void Delete(IKeyValue keyValue);

    /// <summary>
    /// 获取所有 key 数据列表
    /// </summary>
    /// <returns></returns>
    IList<string> GetKeys();

    /// <summary>
    /// 获取所有数据
    /// </summary>
    /// <returns></returns>
    (List<IKeyValue> keyValues, List<long> times) GetKeyValues(bool Immutable);

    /// <summary>
    /// 获取不变表的数量
    /// </summary>
    /// <returns></returns>
    int GetImmutableTableCount();

    /// <summary>
    /// 开始交换
    /// </summary>
    void Swap(List<long> times);

    /// <summary>
    /// 清空全部数据
    /// </summary>
    void Clear();
}