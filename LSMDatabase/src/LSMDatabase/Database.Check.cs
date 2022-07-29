using System.Diagnostics;
using LSMDatabase.Contracts;
using LSMDatabase.Contracts.Databases;

namespace LSMDatabase;

public partial class Database
{
    private volatile bool _isProcess;

    public void Check(object state)
    {
        if (_isProcess)
        {
            return;
        }

        if (_clearState)
        {
            return;
        }

        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            _isProcess = true;
            CheckMemory();
            TableManage.Check();
            stopwatch.Stop();
            GC.Collect();
            Log.Info($"定时心跳处理耗时:{stopwatch.ElapsedMilliseconds}毫秒");
        }
        finally
        {
            _isProcess = false;
        }
    }

    /// <summary>
    /// 检查内存
    /// </summary>
    private void CheckMemory()
    {
        ReaderWriterLock.EnterUpgradeableReadLock();
        try
        {
            if (MemoryTable.GetImmutableTableCount() > 0)
            {
                ReaderWriterLock.EnterWriteLock();
                try
                {
                    //获取不变表的数据
                    (List<IKeyValue> keyValues, List<long> times) = MemoryTable.GetKeyValues(true);
                    if (!keyValues.Any())
                    {
                        return;
                    }

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    var tempData = MemoryTable.GetKeyValues(false);
                    Log.Info($"内存表开始落地：{keyValues.Count}条数据");
                    TableManage.CreateNewTable(keyValues);
                    WalLog.Reset();
                    WalLog.Write(tempData.keyValues);
                    MemoryTable.Swap(times);
                    stopwatch.Stop();
                    Log.Info($"内存表落地结束耗时{stopwatch.ElapsedMilliseconds}毫秒");

                    keyValues.Clear();
                }
                finally
                {
                    ReaderWriterLock.ExitWriteLock();
                }
            }
        }
        finally
        {
            ReaderWriterLock.ExitUpgradeableReadLock();
        }
    }
}