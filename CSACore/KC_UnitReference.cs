using System;
using System.Text;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing an named reference to a knowledge unit. Used to create global variables on the blackboard. 
     */

    public class KC_UnitReference : KC_ReadOnlyString
    {
        public Unit Reference { get; set; }

        // fixme: replace with Name slot on KnowledgeComponent
        public string Name
        {
            get => StringValue;

            set => StringValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(Name: " + StringValue + ", Ref: " + Reference);
            if (ReadOnly)
            {
                sb.Append(", readonly)");
            }
            else
            {
                sb.Append(")");
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_UnitReference(this);

        public KC_UnitReference()
        {
            Reference = null;
        }

        public KC_UnitReference(string name) : base(name)
        {
            Reference = null;
        }

        public KC_UnitReference(string name, Unit reference) : base(name)
        {
            Reference = reference;
        }

        public KC_UnitReference(string name, bool readOnly) : base(name, readOnly)
        {
            Reference = null;
        }

        public KC_UnitReference(string name, bool readOnly, Unit reference) : base(name, readOnly)
        {
            Reference = reference;
        }

        protected KC_UnitReference(KC_UnitReference toCopy) : base(toCopy)
        {
            // When cloning a unit reference, it points to the same Unit as cloned unit reference with the same reference name. 
            Reference = toCopy.Reference;
        }
    }

    public static class KC_UnitReference_Extensions
    {
        public static string GetReferenceName(this Unit unit)
        {
            return unit.GetStringValue<KC_UnitReference>();
        }

        public static void SetReferenceName(this Unit unit, string name)
        {
            unit.SetStringValue<KC_UnitReference>(name);
        }

        public static bool ReferenceNameEquals(this Unit unit, string refName)
        {
            return unit.StringValueEquals<KC_UnitReference>(refName);
        }

        public static Unit GetUnitReference(this Unit unit)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                return unit.GetComponent<KC_UnitReference>().Reference;
            }
            throw new InvalidOperationException("GetUnitReference() called on Unit without a KC_UnitReference componenent.");
        }

        public static void SetUnitReference(this Unit unit, Unit reference)
        {
            if (unit.HasComponent<KC_UnitReference>())
            {
                unit.GetComponent<KC_UnitReference>().Reference = reference;
            }
            else
            {
                throw new InvalidOperationException("SetUnitReference() called on Unit without a KC_UnitReference componenent.");
            }
        }
    }
}
