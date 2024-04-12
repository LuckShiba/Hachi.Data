using System;

namespace Hachi.Data.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class JsonGeneratedAttribute(string source) : Attribute
{
    public string Source { get; } = source;
}