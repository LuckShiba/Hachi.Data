using System.Collections.Generic;
using System.Text;
using Hachi.Data.Interfaces;

namespace Hachi.Data;

public abstract class ClassEmitter
{
    private readonly List<string> _existingTypes;
    protected readonly StringBuilder StringBuilder = new();

    protected ClassEmitter(string namespaceName, string className)
    {
        StringBuilder.AppendLine("#nullable enable");
        StringBuilder.AppendLine($"namespace {namespaceName};");
        _existingTypes = [className];
    }

    protected virtual string[]? GenerateAttributes(IDataType dataType) => null;

    public string Emit(string field, IDataObject dataObject, bool isFirst = false)
    {
        var pascalField = CaseConverter.ToPascalCase(field);
        var name = pascalField;

        if (!isFirst)
        {
            var i = 1;
            while (_existingTypes.Contains(name))
            {
                name = $"{pascalField}{i++}";
            }
        }

        StringBuilder.AppendLine(isFirst ? $"public partial record {name}" : $"public record {name}");

        StringBuilder.Append("{");


        foreach (var property in dataObject.Properties)
        {
            StringBuilder.AppendLine();
            if (GenerateAttributes(property.Value) is { } attributes)
                foreach (var attribute in attributes)
                    StringBuilder.AppendLine($"    {attribute}");
            
            StringBuilder.AppendLine($$"""    public {{property.Value.GetTypeText()}} {{property.Key}} { get; init; }""");
        }
        
        StringBuilder.AppendLine("}");
        
        _existingTypes.Add(name);
        
        return name;
    }
    
    public string GetResult()
    {
        return StringBuilder.ToString();
    }
}