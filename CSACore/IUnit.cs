using System.Collections.Generic;

namespace CSA.Core
{
    public interface IUnit { 
        IDictionary<string, object> Properties { get; }

        bool HasProperty(string propName);
     }
}