namespace LSMDatabase.Contracts.Databases;

/// <summary>
/// 数据库接口
/// </summary>
public interface IDatabase : IDisposable
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    IDatabaseConfig DatabaseConfig { get; }

    /// <summary>
    /// 获取数据
    /// </summary>
    IKeyValue Get(string key);

    /// <summary>
    /// 保存数据(或者更新数据)
    /// </summary>
    bool Set(IKeyValue keyValue);

    /// <summary>
    /// 保存数据(或者更新数据)
    /// </summary>
    bool Set(string key, object value);

    /// <summary>
    /// 获取全部key
    /// </summary>
    List<string> GetKeys();

    /// <summary>
    /// 删除指定数据，并返回存在的数据
    /// </summary>
    IKeyValue DeleteAndGet(string key);

    /// <summary>
    /// 删除数据
    /// </summary>
    void Delete(string key);

    /// <summary>
    /// 定时检查
    /// </summary>
    void Check(object state);

    /// <summary>
    /// 清除数据库所有数据
    /// </summary>
    void Clear();
}