using System.Text;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing a string content pool ID.
     */
    public class KC_ContentPool : KC_ReadOnlyString
    {
        [DistinguishingProperty]
        public string ContentPool
        {
            get => StringValue;

            set => StringValue = value; 
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(ContentPool: " + StringValue);
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

        public override object Clone() => new KC_ContentPool(this);

        public KC_ContentPool()
        {
        }

        public KC_ContentPool(string contentPool) : base(contentPool)
        {
        }

        public KC_ContentPool(string contentPool, bool readOnly) : base(contentPool, readOnly)
        {
        }

        protected KC_ContentPool(KC_ContentPool toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_ContentPool_Extensions
    {
        public static string GetContentPool(this Unit unit)
        {
            return unit.GetStringValue<KC_ContentPool>();
        }

        public static void SetContentPool(this Unit unit, string unitID)
        {
            unit.SetStringValue<KC_ContentPool>(unitID);
        }

        public static bool ContentPoolEquals(this Unit unit, string contentPool)
        {
            return unit.StringValueEquals<KC_ContentPool>(contentPool);
        }
    }
}
