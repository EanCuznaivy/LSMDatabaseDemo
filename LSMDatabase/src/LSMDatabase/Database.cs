using LSMDatabase.WalLogs;
using System.Diagnostics;
using LSMDatabase.Contracts.Databases;
using LSMDatabase.Contracts.MemoryTables;
using LSMDatabase.Contracts.WriteAheadLog;
using LSMDatabase.MemoryTables;

namespace LSMDatabase;

/// <summary>
/// LSM数据库实现
/// </summary>
public partial class Database : IDatabase
{
    public IDatabaseConfig DatabaseConfig { get; private set; }
    private IWriteAheadLog WalLog { get; set; }
    private IMemoryTable MemoryTable { get; set; }
    private ITableManage TableManage { get; set; }
    private ReaderWriterLockSlim ReaderWriterLock = new ReaderWriterLockSlim();
    private Timer Timer;

    public Database(IDatabaseConfig config)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Log.Info($"启动数据服务!");
        if (!Directory.Exists(config.DataDir))
        {
            Directory.CreateDirectory(config.DataDir);
        }

        Log.Info($"数据库文件目录:{config.DataDir}");

        DatabaseConfig = config;
        WalLog = new WriteAheadLog(config);
        TableManage = new TableManage(config);
        MemoryTable = WalLog.LoadToMemory();
        Timer = new Timer(new TimerCallback(Check), null, 0, config.CheckInterval);
        stopwatch.Stop();
        Log.Info($"数据库服务启动成功，耗时:{stopwatch.ElapsedMilliseconds} 毫秒!");
    }

    public IKeyValue Get(string key)
    {
        ReaderWriterLock.EnterReadLock();
        try
        {
            var result = MemoryTable.Search(key);
            if (result.IsExist())
            {
                return result;
            }

            return TableManage.Search(key);
        }
        finally
        {
            ReaderWriterLock.ExitReadLock();
        }
    }

    public List<string> GetKeys()
    {
        HashSet<string> keys = new HashSet<string>();
        foreach (var item in MemoryTable.GetKeys())
        {
            keys.Add(item);
        }

        foreach (var item in TableManage.GetKeys())
        {
            keys.Add(item);
        }

        return keys.ToList();
    }

    public bool Set(IKeyValue keyValue)
    {
        ReaderWriterLock.EnterWriteLock();
        try
        {
            WalLog.Write(keyValue);
            MemoryTable.Set(keyValue);
            return true;
        }
        finally
        {
            ReaderWriterLock.ExitWriteLock();
        }
    }

    public bool Set(string key, object value)
    {
        return Set(new KeyValue(key, value));
    }

    public void Delete(string key)
    {
        ReaderWriterLock.EnterWriteLock();
        try
        {
            var deleteKV = new KeyValue(key, null, true);
            WalLog.Write(deleteKV);
            MemoryTable.Delete(deleteKV);
        }
        finally
        {
            ReaderWriterLock.ExitWriteLock();
        }
    }

    public IKeyValue DeleteAndGet(string key)
    {
        var oldValue = Get(key);
        Delete(key);
        return oldValue;
    }

    private bool _clearState = false;

    public void Clear()
    {
        ReaderWriterLock.EnterWriteLock();
        try
        {
            _clearState = true;
            SpinWait.SpinUntil(() => !_isProcess, 10 * 1000);
            WalLog.Reset();
            MemoryTable.Clear();
            TableManage.Clear();
        }
        finally
        {
            _clearState = false;
            ReaderWriterLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        Timer.Dispose();
        WalLog?.Dispose();
        MemoryTable?.Dispose();
        TableManage?.Dispose();
    }
}