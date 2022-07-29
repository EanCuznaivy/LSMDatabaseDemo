using LSMDatabase.Contracts.Databases;

namespace LSMDatabase;

/// <summary>
/// 数据库配置
/// </summary>
public class DatabaseConfig : IDatabaseConfig
{
    public string DataDir { get; set; }
    public int Level0Size { get; set; } = 10;
    public int LevelMultiple { get; set; } = 10;
    public int LevelCount { get; set; } = 8000;
    public int MemoryTableCount { get; set; } = 10000;
    public int CheckInterval { get; set; } = 1000;
    public static long Version { get; set; } = 202207202304;
}