﻿using LSMDatabase.Contracts;
using LSMDatabase.Contracts.Databases;

namespace LSMDatabase;

/// <summary>
/// 数据信息 kv
/// </summary>
public class KeyValue : IKeyValue
{
    public string Key { get; set; }
    public byte[] DataValue { get; set; }
    public bool Deleted { get; set; }
    private object _value;

    public KeyValue()
    {
    }

    public KeyValue(string key, object value, bool deleted = false)
    {
        Key = key;
        _value = value;
        DataValue = value.AsBytes();
        this.Deleted = deleted;
    }

    public KeyValue(string key, byte[] dataValue, bool deleted)
    {
        Key = key;
        DataValue = dataValue;
        Deleted = deleted;
    }

    /// <summary>
    /// 是否存在有效数据,非删除状态
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess()
    {
        return !Deleted || DataValue != null;
    }

    /// <summary>
    /// 值存不存在，无论删除还是不删除
    /// </summary>
    /// <returns></returns>
    public bool IsExist()
    {
        if (DataValue != null && !Deleted || DataValue == null && Deleted)
        {
            return true;
        }

        return false;
    }

    public T Get<T>() where T : class
    {
        if (_value == null)
        {
            _value = DataValue.AsObject<T>();
        }

        return (T)_value;
    }

    public static readonly IKeyValue Null = new KeyValue { DataValue = null };
}