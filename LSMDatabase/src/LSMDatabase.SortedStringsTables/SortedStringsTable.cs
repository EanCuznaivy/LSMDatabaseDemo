using System.Buffers;
using System.Text;
using LSMDatabase.Contracts.Databases;
using LSMDatabase.Contracts.SortedStringsTables;

namespace LSMDatabase.SSTables;

public class SortedStringsTable : ISortedStringsTable
{
    private readonly string _dbFile;
    private readonly bool _isDir;

    public SortedStringsTable(string dbFile, bool isDir = true)
    {
        this._dbFile = dbFile;
        this._isDir = isDir;
    }

    public string TableFilePath()
    {
        if (!_isDir)
        {
            return _dbFile;
        }

        if (FileTableMetaInfo == null)
        {
            return null;
        }

        return Path.Combine(_dbFile, $"{GetLevel()}_{FileTableMetaInfo.Time}.db");
    }

    public ITableMetaInfo FileTableMetaInfo { get; private set; }

    public Dictionary<string, IDataPosition> DataPositions { get; private set; }

    public long FileBytes
    {
        get { return new FileInfo(TableFilePath()).Length; }
    }

    public int Count => DataPositions.Count;

    public int GetLevel()
    {
        return FileTableMetaInfo.Level;
    }

    public long FileTableName()
    {
        return FileTableMetaInfo.Time;
    }

    public List<string> SortIndexes()
    {
        var list = DataPositions.Keys.ToList();
        list.Sort();
        return list;
    }

    public void Load()
    {
        var path = TableFilePath();
        FileTableMetaInfo = new TableMetaInfo();
        var size = TableMetaInfo.GetDataLength();
        using var read = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var shard = ArrayPool<byte>.Shared;
        var rentLength = size;
        var metaInfoBytes = shard.Rent(rentLength);
        read.Seek(read.Length - size, SeekOrigin.Begin);
        read.Read(metaInfoBytes, 0, rentLength);

        FileTableMetaInfo = TableMetaInfo.GetFileMetaInfo(metaInfoBytes.Take(rentLength).ToArray());
        shard.Return(metaInfoBytes);
        //读取索引部分
        DataPositions = new Dictionary<string, IDataPosition>();
        rentLength = DataPosition.GetDataLength();
        read.Seek(FileTableMetaInfo.IndexStart, SeekOrigin.Begin);
        while (read.Position < FileTableMetaInfo.IndexStart + FileTableMetaInfo.IndexLength)
        {
            var dataPositionBytes = shard.Rent(rentLength);
            read.Read(dataPositionBytes, 0, rentLength);
            var dataPosition = DataPosition.GetDataPosition(dataPositionBytes.Take(rentLength).ToArray());
            shard.Return(dataPositionBytes);

            var keyRentLength = (int)dataPosition.KeyLength;
            var keyBytes = shard.Rent(keyRentLength);
            read.Read(keyBytes, 0, keyRentLength);
            var key = Encoding.UTF8.GetString(keyBytes.Take(keyRentLength).ToArray());
            shard.Return(keyBytes);
            DataPositions.Add(key, dataPosition);
        }
    }

    /// <summary>
    /// 重写文件(会采用冗余的方式，实现重写文件)
    /// </summary>
    public void Write(List<IKeyValue> values, int level = 0)
    {
        if (values.Any() == false)
        {
            return;
        }

        var tableId = IDHelper.MarkID();
        DataPositions = new Dictionary<string, IDataPosition>();
        var ValueBytes = new List<byte>();

        foreach (var item in values)
        {
            var value = item.DataValue;
            DataPositions.Add(item.Key, new DataPosition(ValueBytes.Count(), value.Length, item.Deleted));
            ValueBytes.AddRange(value);
        }

        //赋予index
        var index = 1;
        var DataPositionBytes = new List<byte>();
        var indexRoot = ValueBytes.Count();
        foreach (var item in DataPositions)
        {
            var keyBytes = Encoding.UTF8.GetBytes(item.Key);
            item.Value.KeyLength = keyBytes.Length;
            item.Value.IndexStart = indexRoot + DataPositionBytes.Count();

            DataPositionBytes.AddRange(item.Value.GetBytes());
            DataPositionBytes.AddRange(keyBytes);
            index++;
        }

        FileTableMetaInfo = new TableMetaInfo(tableId, 0, ValueBytes.Count(), ValueBytes.Count(),
            DataPositionBytes.Count, level);
        var MetaInfoBytes = FileTableMetaInfo.GetBytes();

        var path = TableFilePath();
        using var writer = File.OpenWrite(path);
        writer.Write(ValueBytes.ToArray());
        writer.Write(DataPositionBytes.ToArray());
        writer.Write(MetaInfoBytes.ToArray());

        //clear
        ValueBytes.Clear();
        ValueBytes = null;
        DataPositionBytes.Clear();
        DataPositionBytes = null;
        MetaInfoBytes.Clear();
        MetaInfoBytes = null;
    }

    public IKeyValue Search(string key)
    {
        if (DataPositions == null)
        {
            return null;
        }

        var keyvalue = KeyValue.Null;
        keyvalue.Key = key;
        if (DataPositions.TryGetValue(key, out var position))
        {
            var result = ReadValue(position);
            keyvalue.DataValue = result;
            keyvalue.Deleted = position.Deleted;
        }

        return keyvalue;
    }

    public List<string> GetKeys()
    {
        return DataPositions.Select(t => t.Key).ToList();
    }

    public IDataPosition GetDataPosition(string key)
    {
        DataPositions.TryGetValue(key, out var position);
        return position;
    }

    public byte[] ReadValue(IDataPosition position)
    {

        if (position == null || position.Deleted || position.Length == 0)
        {
            return null;
        }

        var path = TableFilePath();
        using var read = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        read.Seek(position.Start, SeekOrigin.Begin);

        var values = new byte[position.Length];
        read.Read(values, 0, values.Length);

        return values;
    }

    public List<IKeyValue> ReadAll(bool incloudDeleted = true)
    {
        List<IKeyValue> keyValues = new();
        var path = TableFilePath();
        using var read = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        List<KeyValuePair<string, IDataPosition>> positions;
        positions = incloudDeleted ? DataPositions.ToList() : DataPositions.Where(t => !t.Value.Deleted).ToList();

        foreach (var keyValue in positions)
        {
            read.Seek(keyValue.Value.Start, SeekOrigin.Begin);
            var valueBytes = new byte[keyValue.Value.Length];
            read.Read(valueBytes, 0, valueBytes.Length);
            keyValues.Add(new KeyValue(keyValue.Key, valueBytes, keyValue.Value.Deleted));
        }

        return keyValues;
    }

    public static SortedStringsTable CreateFileTable(string dbFile, List<IKeyValue> values, int Level = 0)
    {
        if (values?.Any() == false)
        {
            return null;
        }

        var ssTable = new SortedStringsTable(dbFile);
        ssTable.Write(values, Level);
        if (ssTable.FileTableMetaInfo != null)
        {
            return ssTable;
        }

        return null;
    }

    public void Dispose()
    {
        DataPositions.Clear();
    }

    public override string ToString()
    {
        return $"{FileTableName()} level:{GetLevel()}";
    }
}
