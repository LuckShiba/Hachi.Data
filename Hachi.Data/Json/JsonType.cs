using System;
using System.Linq;
using System.Text.Json;
using Hachi.Data.Interfaces;

namespace Hachi.Data.Json;

public class JsonType : IDataType
{
    public string FieldName { get; }
    public string Type { get; }
    public bool Nullable { get; set; }
    private readonly bool _isEnumerable;

    private JsonType(string field, string type, bool nullable)
    {
        FieldName = field;
        Type = type;
        Nullable = nullable;
    }
    
    public JsonType(string field, JsonElement jsonElement, ClassEmitter emitter)
    {
        FieldName = field;
        
        switch (jsonElement.ValueKind)
        {
            case JsonValueKind.Undefined or JsonValueKind.Null:
                Type = "object";
                Nullable = true;
                break;
            case JsonValueKind.Object:
                Type = emitter.Emit(field, new JsonObject(jsonElement, emitter));
                break;
            case JsonValueKind.Array:
                _isEnumerable = true;

                var jsonArray = jsonElement.EnumerateArray();
                
                if (jsonArray.Select(x => x.ValueKind).Distinct().Count() != 1)
                {
                    Type = "object";
                    Nullable = true;
                    break;
                }

                var first = jsonArray.First();
                if (first.ValueKind == JsonValueKind.Object)
                {
                    var merged = jsonArray.Select(x => new JsonObject(x, emitter))
                        .Aggregate((x, y) => x.Merge(y));
                    
                    Type = emitter.Emit(field, merged);
                    break;
                }
                
                Type = new JsonType(field, first, emitter).Type;
                
                break;
            case JsonValueKind.String:
                Type = DateTime.TryParse(jsonElement.GetString(), out _) ? "DateTimeOffset" : "string";
                break;
            case JsonValueKind.Number:
                if (jsonElement.GetDouble() % 1 == 0)
                {
                    Type = "int";
                    break;
                }
                
                Type = "double";
                break;
            case JsonValueKind.True or JsonValueKind.False:
                Type = "bool";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(jsonElement));
        }
    }
    
    public string GetTypeText()
    {
        var typeText = Type;
        
        if (Nullable)
            typeText += "?";
        
        return _isEnumerable ? $"IEnumerable<{typeText}>" : typeText;
    }
    
    public static JsonType Unknown(string field) => new(field, "object", true);
}