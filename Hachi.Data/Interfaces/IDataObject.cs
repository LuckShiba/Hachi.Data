using System.Collections.Generic;

namespace Hachi.Data.Interfaces;

public interface IDataObject
{
    public IDictionary<string, IDataType> Properties { get; }
}