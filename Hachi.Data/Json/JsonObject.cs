using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Hachi.Data.Interfaces;

namespace Hachi.Data.Json;

public class JsonObject : IDataObject
{
    public IDictionary<string, IDataType> Properties
    {
        get
        {
            return _properties.ToDictionary(x => x.Key, x => (IDataType) x.Value);
        }
    }

    private readonly IDictionary<string, JsonType> _properties;
    
    private JsonObject(IDictionary<string, JsonType> properties)
    {
        _properties = properties;
    }
    
    public JsonObject(JsonElement jsonElement, ClassEmitter emitter)
    {
        _properties = new Dictionary<string, JsonType>();
        
        if (jsonElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("JsonElement is not an object");
        }
        
        foreach (var property in jsonElement.EnumerateObject())
        {
            _properties[CaseConverter.ToPascalCase(property.Name)] = new JsonType(property.Name, property.Value, emitter);
        }
    }

    public JsonObject Merge(JsonObject other)
    {
        if (this == other)
            return this;

        var properties = new Dictionary<string, JsonType>(_properties);

        foreach (var property in _properties.Where(x => !other._properties.ContainsKey(x.Key)))
        {
            properties[property.Key].Nullable = true;
        }
        
        foreach (var property in other._properties)
        {
            var key = property.Key;
            var value = property.Value;
            
            if (!properties.ContainsKey(key))
            {
                value.Nullable = true;
                properties[key] = value;
                continue;
            }
            
            if (properties[key].Type != value.Type)
                properties[key] = JsonType.Unknown(key);
        }
        
        return new JsonObject(properties);
    }

    public override int GetHashCode()
    {
        return _properties.GetHashCode();
    }
}