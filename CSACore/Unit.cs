using System.Collections.Generic;

namespace CSACore
{
    // Base class for all Blackboard Units.
    public class Unit : IUnit
    {
        public static string TypeName { get; } = new Unit().GetType().FullName;

        public IDictionary<string, object> Properties { get; }

        public Unit()
        {
            Properties = new Dictionary<string, object>();
        }

        public Unit(IUnit u)
        {
            Properties = new Dictionary<string, object>(u.Properties);
        }    
    }
}