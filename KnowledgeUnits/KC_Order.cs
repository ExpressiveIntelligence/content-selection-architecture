using System.Text;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing an integer order (for ordering units). 
     */
    public class KC_Order : KC_Integer
    {
        public int Order
        {
            get => IntValue;

            set => IntValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(Order: " + IntValue);
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

        public override object Clone() => new KC_Order(this);

        public KC_Order()
        {
        }

        public KC_Order(int order) : base(order)
        {
        }

        public KC_Order(int order, bool readOnly) : base(order, readOnly)
        {
        }

        protected KC_Order(KC_Order toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_Order_Extensions
    {
        public static int GetOrder(this Unit unit)
        {
            return unit.GetIntValue<KC_Order>();
        }

        public static void SetOrder(this Unit unit, int order)
        {
            unit.SetIntValue<KC_Order>(order);
        }
    }
}
