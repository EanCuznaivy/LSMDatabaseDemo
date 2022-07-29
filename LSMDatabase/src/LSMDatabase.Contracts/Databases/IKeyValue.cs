namespace LSMDatabase.Contracts.Databases;

public interface IKeyValue
{
    string Key { get; set; }
    byte[] DataValue { get; set; }
    bool Deleted { get; set; }

    bool IsSuccess();
    bool IsExist();

    T Get<T>() where T : class;
}