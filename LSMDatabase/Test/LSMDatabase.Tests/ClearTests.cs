namespace LSMDatabase.Tests;

public class ClearTests
{
    [Fact]
    public void ClearTest()
    {
        var dataBase = new Database(new DatabaseConfig()
            { DataDir = Path.Combine(AppContext.BaseDirectory, "TempDB") });
        dataBase.Clear();
    }
}