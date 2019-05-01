using System;

namespace CSA.Core
{
    /*
     * KnowledgeComponent for storing a string content pool ID.
     */
    public class KC_ContentPool : KC_ImmutableString
    {
        public string ContentPool
        {
            get => StringValue;

            set => StringValue = value; 
        }

        public KC_ContentPool()
        {
        }

        public KC_ContentPool(string contentPool) : base(contentPool)
        {
        }

        public KC_ContentPool(string contentPool, bool immutable) : base(contentPool, immutable)
        {
        }
    }

    public static class KC_ContentPool_Extensions
    {
        public static string GetContentPool(this Unit unit)
        {
            return unit.GetStringValue();
        }

        public static void SetContentPool(this Unit unit, string unitID)
        {
            unit.SetStringValue(unitID);
        }

        public static bool ContentPoolEquals(this Unit unit, string contentPool)
        {
            return unit.StringValueEquals(contentPool);
        }
    }
}
