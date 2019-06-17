using System;

namespace CSA.Core
{
    public abstract class KC_Double : KC_ReadOnly, IComparable
    {
        private double m_double; 

        public double DoubleValue
        {
            get
            {
                return m_double;
            }

            set
            {
                if (!ReadOnly)
                {
                    m_double = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the DoubleValue properity of a readOnly KC_Double");
                }

            }
        }

        public int CompareTo(object obj)
        {
            if (obj is KC_Double otherObject)
            {
                return m_double.CompareTo(otherObject.DoubleValue);
            }
            throw new ArgumentException("Object: " + obj + " is not a KC_Double");
        }

        protected KC_Double()
        {
            m_double = 0;
        }

        protected KC_Double(double intValue)
        {
            m_double = intValue;
        }

        protected KC_Double(double intValue, bool readOnly) : base(readOnly)
        {
            m_double = intValue;
        }

        protected KC_Double(KC_Double toCopy) : base(toCopy)
        {
            m_double = toCopy.m_double;
        }
    }

    public static class KC_ReadOnlyDouble_Extensions
    {
        public static double GetDoubleValue<T>(this Unit unit) where T : KC_Double
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().DoubleValue;
            }
            throw new InvalidOperationException($"GetDoubleValue<{typeof(T).Name}> called on unit that does not have a component with KC_Double parent.");
        }

        public static void SetDoubleValue<T>(this Unit unit, double doubleToStore) where T : KC_Double
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().DoubleValue = doubleToStore;
            }
            else
            {
                throw new InvalidOperationException($"SetDoubleValue<{typeof(T).Name}> called on unit that does not have a component with KC_Double parent.");
            }
        }
    }

}
