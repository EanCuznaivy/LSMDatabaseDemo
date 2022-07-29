using System.Collections.Concurrent;

namespace LSMDatabase;

/// <summary>
/// 生成时间序 不重复 ID
/// </summary>
public static class IDHelper
{
    private static readonly ConcurrentDictionary<long, int> Times = new();

    /// <summary>
    /// 生成ID
    /// 时间戳13位，扩展4位为随机位，一共17位的 时间有序不重ID
    /// </summary>
    /// <returns></returns>
    public static long MarkID()
    {
        var now = GenerateTimestamp();
        var id = Times.AddOrUpdate(now, 0, (k, v) => v + 1);
        if (Times.Count > 5 * 60)
        {
            var removeIds = Times.Keys.Where(k => k < now);
            foreach (var item in removeIds)
            {
                Times.TryRemove(item, out _);
            }
        }

        return now * 10000 + id;
    }

    public static long GenerateTimestamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
    }
}
