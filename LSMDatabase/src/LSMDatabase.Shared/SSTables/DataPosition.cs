using LSMDatabase.Contracts.SortedStringsTables;

namespace LSMDatabase.SSTables;

public class DataPosition : IDataPosition
{
    public DataPosition()
    {
    }

    public DataPosition(long start, long length, bool deleted = false)
    {
        this.Start = start;
        this.Length = length;
        this.Deleted = deleted;
    }

    public long IndexStart { get; set; }
    public long Start { get; set; }
    public long Length { get; set; }
    public long KeyLength { get; set; }
    public bool Deleted { get; set; }

    public static int GetDataLength()
    {
        return sizeof(long) * 4 + sizeof(bool);
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = new();
        bytes.AddRange(BitConverter.GetBytes(IndexStart));
        bytes.AddRange(BitConverter.GetBytes(Start));
        bytes.AddRange(BitConverter.GetBytes(Length));
        bytes.AddRange(BitConverter.GetBytes(KeyLength));
        bytes.AddRange(BitConverter.GetBytes(Deleted));
        return bytes.ToArray();
    }

    public static DataPosition GetDataPosition(byte[] bytes)
    {
        DataPosition dataPosition = new();
        var longSize = sizeof(long);
        var index = 0;
        dataPosition.IndexStart = BitConverter.ToInt64(bytes, index += index);
        dataPosition.Start = BitConverter.ToInt64(bytes, index += longSize);
        dataPosition.Length = BitConverter.ToInt64(bytes, index += longSize);
        dataPosition.KeyLength = BitConverter.ToInt64(bytes, index += longSize);
        dataPosition.Deleted = BitConverter.ToBoolean(bytes, index += longSize);
        return dataPosition;
    }
}