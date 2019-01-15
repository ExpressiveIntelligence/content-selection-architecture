using System.Collections.Generic;

namespace CSACore
{
    public interface IUnit { 
        IDictionary<string, object> Properties { get; }
    }
}