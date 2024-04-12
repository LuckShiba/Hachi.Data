namespace Hachi.Data.Interfaces;

public interface IDataType
{
    string FieldName { get; }
    string GetTypeText();
}