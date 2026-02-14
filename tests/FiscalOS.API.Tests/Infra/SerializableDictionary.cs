namespace FiscalOS.API.Tests.Infra;

public interface ISerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IXunitSerializable
{
}

public class SerializableDictionary<TKey, TValue> 
  : Dictionary<TKey, TValue>, ISerializableDictionary<TKey, TValue> where TKey : notnull
{
  public SerializableDictionary() { }

  public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary)
  {
  }

  public void Deserialize(IXunitSerializationInfo info)
  {
    Clear();

    var keysJson = info.GetValue<string>("_DictKeys") ?? "[]";
    var valuesJson = info.GetValue<string>("_DictValues") ?? "[]";
    var keys = JsonSerializer.Deserialize<List<TKey>>(keysJson);
    var values = JsonSerializer.Deserialize<List<TValue>>(valuesJson);

    if (keys is null || values is null || keys.Count != values.Count)
    {
      return;
    }

    for (var i = 0; i < keys.Count; i++)
    {
      Add(keys[i], values[i]);
    }
  }

  public void Serialize(IXunitSerializationInfo info)
  {
    info.AddValue("_DictKeys", JsonSerializer.Serialize(Keys));
    info.AddValue("_DictValues", JsonSerializer.Serialize(Values));
  }
}