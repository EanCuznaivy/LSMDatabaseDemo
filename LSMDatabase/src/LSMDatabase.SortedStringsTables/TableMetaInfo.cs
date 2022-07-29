using LSMDatabase.Contracts.SortedStringsTables;

namespace LSMDatabase.SSTables;

public class TableMetaInfo : ITableMetaInfo
{
    public TableMetaInfo()
    {
    }

    public TableMetaInfo(long time, long dataStart, long dataLength, long indexStart, long indexLength,
        int level = 0)
    {
        Version = DatabaseConfig.Version;
        this.Time = time;
        this.DataStart = dataStart;
        this.DataLength = dataLength;
        this.IndexStart = indexStart;
        this.IndexLength = indexLength;
        this.Level = level;
    }

    public long Version { get; set; }
    public long Time { get; set; }
    public long DataStart { get; set; }
    public long DataLength { get; set; }
    public long IndexStart { get; set; }
    public long IndexLength { get; set; }
    public int Level { get; set; }

    public static int GetDataLength()
    {
        return sizeof(long) * 6 + sizeof(int);
    }

    public List<byte> GetBytes()
    {
        List<byte> bytes = new();
        bytes.AddRange(BitConverter.GetBytes(Version));
        bytes.AddRange(BitConverter.GetBytes(Time));
        bytes.AddRange(BitConverter.GetBytes(DataStart));
        bytes.AddRange(BitConverter.GetBytes(DataLength));
        bytes.AddRange(BitConverter.GetBytes(IndexStart));
        bytes.AddRange(BitConverter.GetBytes(IndexLength));
        bytes.AddRange(BitConverter.GetBytes(Level));
        return bytes;
    }

    public static TableMetaInfo GetFileMetaInfo(byte[] bytes)
    {
        TableMetaInfo fileMetaInfo = new();
        var longSize = sizeof(long);
        var index = 0;
        fileMetaInfo.Version = BitConverter.ToInt64(bytes, index);
        fileMetaInfo.Time = BitConverter.ToInt64(bytes, index += longSize);
        fileMetaInfo.DataStart = BitConverter.ToInt64(bytes, index += longSize);
        fileMetaInfo.DataLength = BitConverter.ToInt64(bytes, index += longSize);
        fileMetaInfo.IndexStart = BitConverter.ToInt64(bytes, index += longSize);
        fileMetaInfo.IndexLength = BitConverter.ToInt64(bytes, index += longSize);
        fileMetaInfo.Level = BitConverter.ToInt32(bytes, index += longSize);
        return fileMetaInfo;
    }
}