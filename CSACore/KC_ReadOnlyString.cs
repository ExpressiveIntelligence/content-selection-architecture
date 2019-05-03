using System;
namespace CSA.Core
{
    public abstract class KC_ReadOnlyString : KC_ReadOnly
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
                if (!ReadOnly)
                {
                    m_storedString = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the StoredString properity of a readOnly KC_ReadOnlyString");
                }
            }
        }

        /*
         * Don't need CopyBaseFields on KC_ReadOnlyString because the string will always be copied by protected copy constructors on derived concrete classes.
         * So we just need CopyBaseFields on KC_ReadOnly. 
         */
        /* protected virtual void CopyBaseFields(KC_ReadOnlyString kc)
        {
            base.CopyBaseFields(kc);
            m_storedString = kc.StringValue;
        } */

        protected KC_ReadOnlyString()
        {
            m_storedString = null;
        }

        protected KC_ReadOnlyString(string storedString)
        {
            m_storedString = storedString;
        }

        protected KC_ReadOnlyString(string storedString, bool readOnly) : base(readOnly)
        {
            m_storedString = storedString;
        }

        protected KC_ReadOnlyString(KC_ReadOnlyString toCopy) : base(toCopy)
        {
            // Since strings are immutable, just returning a ref to the same string.
            m_storedString = toCopy.m_storedString;
        }
    }

    public static class KC_ReadOnlyString_Extensions
    {
        public static string GetStringValue<T>(this Unit unit) where T : KC_ReadOnlyString
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().StringValue;
            }
            throw new InvalidOperationException("GetStringValue<" + typeof(T).Name + "> called on unit that does not have a component with KC_ReadOnlyString parent.");
        }

        public static void SetStringValue<T>(this Unit unit, string stringToStore) where T : KC_ReadOnlyString
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().StringValue = stringToStore;
            }
            else
            {
                throw new InvalidOperationException("SetStringValue<" + typeof(T).Name + "> called on unit that does not have a component with KC_ReadOnlyString parent.");
            }
        }

        public static bool StringValueEquals<T>(this Unit unit, string s) where T : KC_ReadOnlyString
        {
            return unit.GetStringValue<T>().Equals(s);
        }
    }

}
