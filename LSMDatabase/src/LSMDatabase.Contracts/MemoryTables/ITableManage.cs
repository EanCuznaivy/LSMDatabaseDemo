using LSMDatabase.Contracts.Databases;

namespace LSMDatabase.Contracts.MemoryTables;

/// <summary>
/// 表管理项
/// </summary>
public interface ITableManage : IDisposable
{
    IDatabaseConfig DatabaseConfig { get; }

    /// <summary>
    /// 搜索(从新到老,从大到小)
    /// </summary>
    IKeyValue Search(string key);

    /// <summary>
    /// 获取全部key
    /// </summary>
    List<string> GetKeys();

    /// <summary>
    /// 检查数据库文件，如果文件无效数据太多，就会触发整合文件
    /// </summary>
    void Check();

    /// <summary>
    /// 创建一个新Table
    /// </summary>
    void CreateNewTable(List<IKeyValue> values, int level = 0);

    /// <summary>
    /// 清理某个级别的数据
    /// </summary>
    /// <param name="level"></param>
    public void Remove(int level);

    /// <summary>
    /// 清除数据
    /// </summary>
    public void Clear();
}