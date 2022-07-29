namespace LSMDatabase.Contracts.SortedStringsTables;

/// <summary>
/// 表文件元数据
/// </summary>
public interface ITableMetaInfo
{
    public long Version { get; set; }
    public long Time { get; set; }
    public long DataStart { get; set; }
    public long DataLength { get; set; }
    public long IndexStart { get; set; }
    public long IndexLength { get; set; }
    public int Level { get; set; }
    public List<byte> GetBytes();
}