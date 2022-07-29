namespace LSMDatabase.Tests;

public class TestValue
{
    public string Key { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public string Value { get; set; }

    public override string ToString()
    {
        return $"key:{Key} name :{Name} age:{Age} value:{Value}";
    }
}