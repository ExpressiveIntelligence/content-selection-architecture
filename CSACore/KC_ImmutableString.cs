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
        public static string GetStringValue<T>(this Unit unit) where T : KC_ImmutableString
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().StringValue;
            }
            throw new InvalidOperationException("GetStringValue<" + typeof(T).Name + "> called on unit that does not have a component with KC_ImmutableString parent.");
        }

        public static void SetStringValue<T>(this Unit unit, string stringToStore) where T : KC_ImmutableString
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().StringValue = stringToStore;
            }
            else
            {
                throw new InvalidOperationException("SetStringValue<" + typeof(T).Name + "> called on unit that does not have a component with KC_ImmutableString parent.");
            }
        }

        public static bool StringValueEquals<T>(this Unit unit, string s) where T : KC_ImmutableString
        {
            return unit.GetStringValue<T>().Equals(s);
        }
    }

}
