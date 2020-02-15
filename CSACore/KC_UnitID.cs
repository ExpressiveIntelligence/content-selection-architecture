using System.Text;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing an unit ID. 
     */
    public class KC_UnitID : KC_ReadOnlyString
    {
        public string UnitID
        {
            get => StringValue;

            set => StringValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(UnitID: " + StringValue);
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

        public override object Clone() => new KC_UnitID(this);

        public KC_UnitID()
        {
        }

        public KC_UnitID(string targetID) : base(targetID)
        {
        }

        public KC_UnitID(string targetID, bool readOnly) : base(targetID, readOnly)
        {
        }

        protected KC_UnitID(KC_UnitID toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_UnitID_Extensions
    {
        public static string GetUnitID(this Unit unit)
        {
            return unit.GetStringValue<KC_UnitID>();
        }

        public static void SetUnitID(this Unit unit, string unitID)
        {
            unit.SetStringValue<KC_UnitID>(unitID);
        }

        public static bool UnitIDEquals(this Unit unit, string unitID)
        {
            return unit.StringValueEquals<KC_UnitID>(unitID);
        }

    }
}
