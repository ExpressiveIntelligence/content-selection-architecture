using System.Collections.Generic;
using System.Text;

namespace CSA.Core
{
    // Base class for all Blackboard Units.
    public class Unit : IUnit
    {
        public IDictionary<string, object> Slots { get; }

        public bool HasSlot(string propName)
        {
            return Slots.ContainsKey(propName);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);

            sb.AppendLine("Unit:");

            // Properties
            sb.AppendLine("\tProperties:");
            foreach (KeyValuePair<string, object> kvp in Slots)
            {
                sb.AppendLine("\t\t" + kvp.Key + " = " + kvp.Value);
            }

            return sb.ToString();
        }

        public Unit()
        {
            Slots = new Dictionary<string, object>();
        }

        public Unit(IUnit u)
        {
            Slots = new Dictionary<string, object>(u.Slots);
        }
    }
}