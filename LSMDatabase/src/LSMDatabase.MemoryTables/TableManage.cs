using LSMDatabase.SSTables;
using System.Diagnostics;
using LSMDatabase.Contracts.Databases;
using LSMDatabase.Contracts.MemoryTables;
using LSMDatabase.Contracts.SortedStringsTables;

namespace LSMDatabase.MemoryTables
{
    /// <summary>
    /// 每个库都有多个表，表包含多个 索引表和数据表
    /// </summary>
    public class TableManage : ITableManage
    {
        public IDatabaseConfig DatabaseConfig { get; private set; }

        /// <summary>
        /// 从小到大排序的
        /// </summary>
        public readonly SortedList<long, ISortedStringsTable> FileTables;

        private readonly ReaderWriterLockSlim _readerWriterLock = new();

        public TableManage(IDatabaseConfig config)
        {
            DatabaseConfig = config;
            FileTables = LoadDataBaseFile();
        }

        public SortedList<long, ISortedStringsTable> LoadDataBaseFile()
        {
            var list = new SortedList<long, ISortedStringsTable>();
            foreach (var item in Directory.GetFiles(DatabaseConfig.DataDir, "*.db"))
            {
                var ssTable = new SortedStringsTable(item, false);
                ssTable.Load();
                list.Add(ssTable.FileTableName(), ssTable);
            }

            return list;
        }

        /// <summary>
        /// 检查数据库文件，如果文件无效数据太多，就会触发整合文件
        /// </summary>
        public void Check()
        {
            foreach (var item in FileTables.GroupBy(t => t.Value.GetLevel()))
            {
                var level = item.Key;
                var AllSize = item.Sum(t => t.Value.FileBytes);
                var AllCount = item.Sum(t => t.Value.Count);
                var TableSize = AllSize / 1024 / 1024; //转 MB
                var CurrentLevelMaxSize = Math.Pow(DatabaseConfig.LevelMultiple, level) * DatabaseConfig.Level0Size;
                var CurrentLevelCount = Math.Pow(DatabaseConfig.LevelMultiple, level) * DatabaseConfig.LevelCount;
                if (TableSize > CurrentLevelMaxSize || AllCount > CurrentLevelCount)
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Log.Info(
                        $"持久化表级别Level {level} 当前数据 Size:{TableSize} Count:{AllCount} 触及阈值 Level:{level} MaxSize:{CurrentLevelMaxSize} MaxCount:{CurrentLevelCount}， 开始压缩!");
                    SegmentCompaction(level, item.Select(t => t.Value).ToList());
                    stopwatch.Stop();
                    Log.Info($"持久化表级别:{level} 压缩完毕，耗时:{stopwatch.ElapsedMilliseconds} 毫秒!");
                }
            }
        }

        /// <summary>
        /// 分级压缩文件(先老后新，先小后大,级别 从0 到N )
        /// 数据满了一定阈值，就会生成一个sstable文件
        /// 压缩后，级别自动往下一级
        /// </summary>
        private void SegmentCompaction(int level, List<ISortedStringsTable> levels)
        {
            var nextLevel = level + 1;
            var dic = new Dictionary<string, IKeyValue>();
            //倒着遍历 先老后新，先小后大
            foreach (var item in levels.OrderBy(t => t.FileTableName()))
            {
                var allData = item.ReadAll(false);
                foreach (var data in allData)
                {
                    dic[data.Key] = new KeyValue(data.Key, data.DataValue, data.Deleted);
                }

                allData.Clear();
                allData = null;
            }

            if (dic.Values.Count() > 0)
            {
                CreateNewTable(dic.Values.ToList(), nextLevel);
                Remove(level);

                dic.Clear();
                dic = null;
            }
        }

        /// <summary>
        /// 搜索(从新到老,从大到小)
        /// </summary>
        public IKeyValue Search(string key)
        {
            foreach (var item in FileTables.GroupBy(t => t.Value.GetLevel()).OrderBy(t => t.Key))
            {
                foreach (var table in item.OrderByDescending(t => t.Key))
                {
                    var result = table.Value.Search(key);
                    if (result?.IsExist() == true)
                    {
                        return result;
                    }
                }
            }

            return KeyValue.Null;
        }

        public List<string> GetKeys()
        {
            _readerWriterLock.EnterReadLock();
            try
            {
                return FileTables.Values.SelectMany(t => t.GetKeys()).Distinct().ToList();
            }
            finally
            {
                _readerWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 清理某个级别的数据
        /// </summary>
        /// <param name="Level"></param>
        public void Remove(int level)
        {
            _readerWriterLock.EnterWriteLock();
            try
            {
                var filePaths = FileTables.Where(t => t.Value.GetLevel() == level).Select(t => t.Value.TableFilePath())
                    .ToList();
                var removeIds = FileTables.Where(t => t.Value.GetLevel() == level).Select(t => t.Key).ToList();
                foreach (var item in removeIds)
                {
                    FileTables[item].Dispose();
                    FileTables.Remove(item);
                }

                foreach (var file in filePaths)
                {
                    File.Delete(file);
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public void CreateNewTable(List<IKeyValue> values, int level = 0)
        {
            _readerWriterLock.EnterWriteLock();
            try
            {
                var table = SortedStringsTable.CreateFileTable(DatabaseConfig.DataDir, values, level);
                if (table != null)
                {
                    FileTables.Add(table.FileTableName(), table);
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            foreach (var item in FileTables)
            {
                item.Value.Dispose();
            }
        }

        public void Clear()
        {
            _readerWriterLock.EnterWriteLock();
            try
            {
                var result = FileTables.ToList();
                FileTables.Clear();
                foreach (var item in result)
                {
                    File.Delete(item.Value.TableFilePath());
                    item.Value.Dispose();
                }
            }
            finally
            {
                _readerWriterLock.ExitWriteLock();
            }
        }
    }
}