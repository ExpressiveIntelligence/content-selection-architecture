using System;

namespace CSA.Core
{
    public abstract class KC_Integer : KC_ReadOnly, IComparable
    {
        private int m_int;

        public int IntValue
        {
            get
            {
                return m_int;
            }

            set
            {
                if (!ReadOnly)
                {
                    m_int = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the IntValue properity of a readOnly KC_Integer");
                }
            }
        }

        public int CompareTo(object o)
        {
            if (o is KC_Integer otherObject)
            {
                return m_int.CompareTo(otherObject.IntValue);
            }
            else
            {
                throw new ArgumentException("Object: " + o + " is not a KC_Integer");
            }
        }

        protected KC_Integer()
        {
            m_int = 0; 
        }

        protected KC_Integer(int intValue)
        {
            m_int = intValue;
        }

        protected KC_Integer(int intValue, bool readOnly) : base(readOnly)
        {
            m_int = intValue;
        }

        protected KC_Integer(KC_Integer toCopy) : base(toCopy)
        {
            m_int = toCopy.m_int;
        }
    }

    public static class KC_ReadOnlyInteger_Extensions
    {
        public static int GetIntValue<T>(this Unit unit) where T : KC_Integer
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().IntValue;
            }
            throw new InvalidOperationException($"GetIntValue<{typeof(T).Name}> called on unit that does not have a component with KC_Integer parent.");
        }

        public static void SetIntValue<T>(this Unit unit, int intToStore) where T : KC_Integer
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().IntValue = intToStore;
            }
            else
            {
                throw new InvalidOperationException($"SetIntValue<{typeof(T).Name}> called on unit that does not have a component with KC_Integer parent.");
            }
        }
    }
}
