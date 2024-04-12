using Hachi.Data.Interfaces;

namespace Hachi.Data.Json;

public class JsonClassEmitter : ClassEmitter
{
    public JsonClassEmitter(string namespaceName, string className) : base(namespaceName, className)
    {
        StringBuilder.AppendLine("using System.Text.Json.Serialization;");
    }
    protected override string[]? GenerateAttributes(IDataType dataType)
    {
        return
        [
            $"[JsonPropertyName(\"{dataType.FieldName}\")]"
        ];
    }
}