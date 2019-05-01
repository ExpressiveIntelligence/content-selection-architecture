using System;
namespace CSA.Core
{
    public abstract class KC_ImmutableString : KC_Immutable
    {
        private string m_storedString;

        public string StringValue
        {
            get
            {
                return m_storedString;
            }

            set
            {
                if (!Immutable)
                {
                    m_storedString = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the StoredString properity of an immutable KC_ImmutableString");
                }
            }
        }

        protected KC_ImmutableString()
        {
            m_storedString = null;
        }

        protected KC_ImmutableString(string storedString)
        {
            m_storedString = storedString;
        }

        protected KC_ImmutableString(string storedString, bool immutable) : base(immutable)
        {
            m_storedString = storedString;
        }
    }

    public static class KC_ImmutableString_Extensions
    {
        public static string GetStringValue(this Unit unit)
        {
            if (unit.HasComponent<KC_ImmutableString>())
            {
                return unit.GetComponent<KC_ImmutableString>().StringValue;
            }
            throw new InvalidOperationException("GetStringValue called on unit that does not have a KC_ImmutableString component.");
        }

        public static void SetStringValue(this Unit unit, string stringToStore)
        {
            if (unit.HasComponent<KC_ImmutableString>())
            {
                unit.GetComponent<KC_ImmutableString>().StringValue = stringToStore;
            }
            throw new InvalidOperationException("SetStringValue called on unit that does not have a KC_ImmutableString component.");
        }

        public static bool StringValueEquals(this Unit unit, string s)
        {
            return unit.GetStringValue().Equals(s);
        }
    }

}
