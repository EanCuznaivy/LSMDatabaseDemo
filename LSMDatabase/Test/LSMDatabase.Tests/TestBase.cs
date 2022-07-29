namespace LSMDatabase.Tests;

public class TestBase
{
    /// <summary>
    /// 获取文件夹的大小
    /// </summary>
    /// <returns>返回MB</returns>
    protected static long GetDirSize(string path)
    {
        long size = 0;
        foreach (var item in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            size += new FileInfo(item).Length;
        }

        return size / 1024 / 1024;
    }
}