using System.Collections.Generic;
using System.Text;

namespace CSA.Core
{
    // Base class for all Blackboard Units.
    public class Unit : IUnit
    {
        public IDictionary<string, object> Properties { get; }

        public bool HasProperty(string propName)
        {
            return Properties.ContainsKey(propName);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);

            sb.AppendLine("Unit:");

            // Properties
            sb.AppendLine("\tProperties:");
            foreach (KeyValuePair<string, object> kvp in Properties)
            {
                sb.AppendLine("\t\t" + kvp.Key + " = " + kvp.Value);
            }

            return sb.ToString();
        }

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