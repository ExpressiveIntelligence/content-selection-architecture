using System.Text;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing a utility (or weight) for selecting a unit. 
     */
    public class KC_Utility : KC_Double
    {
        /*
         * This is the property that uniquely distinguishes KC_Utility for Json deserialization.
         */
        [DistinguishingProperty]
        public double Utility
        {
            get => DoubleValue;

            set => DoubleValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(Utility: " + Utility);
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

        public override object Clone() => new KC_Utility(this);

        public KC_Utility()
        {
        }

        public KC_Utility(double utility) : base(utility)
        {
        }

        public KC_Utility(double utility, bool readOnly) : base(utility, readOnly)
        {
        }

        protected KC_Utility(KC_Utility toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_Utility_Extensions
    {
        public static double GetUtility(this Unit unit)
        {
            return unit.GetDoubleValue<KC_Utility>();
        }

        public static void SetUtility(this Unit unit, double utility)
        {
            unit.SetDoubleValue<KC_Utility>(utility);
        }
    }

}
