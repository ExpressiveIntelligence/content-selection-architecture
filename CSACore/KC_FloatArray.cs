using System;
using System.Text;
using System.Collections.Generic;

namespace CSA.Core
{
    /*
     * Abstract KnowledgeComponent storing an array of floats.
     */
    public abstract class KC_FloatArray : KC_ReadOnly
    {
        private float[] m_floatArray;

        public IList<float> FloatArray
        {
            get
            {
                // fixme: eventually test ReadOnly and return m_floatArray wrapped in ReadOnlyCollection<float> if it is readonly
                if (ReadOnly)
                {
                    return Array.AsReadOnly(m_floatArray);
                }
                else
                {
                    return m_floatArray;
                }
            }

            set
            {
                if (!ReadOnly)
                {
                    m_floatArray = (float[])value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the FloatArray property of a readOnly KC_FloatArray");
                }
            }
        }

        protected string ToString(string propName)
        {
            StringBuilder sb = new StringBuilder(100);
            sb.AppendFormat("({0}: [{1}]", propName, FloatArray);
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

        protected KC_FloatArray()
        {
            m_floatArray = null;
        }

        protected KC_FloatArray(float[] floats)
        {
            m_floatArray = floats;
        }

        protected KC_FloatArray(float[] floats, bool readOnly) : base(readOnly)
        {
            m_floatArray = floats;
        }

        protected KC_FloatArray(KC_FloatArray toCopy) : base(toCopy)
        {
            /*
             * If the float[] is readonly, just copy the reference to the array, otherwise create a copy of the array (so that changing elements of the array copy
             * doesn't change elements of the original). 
             * fixme: for this to completely work, need to address the get accessor fixme above. 
             */
            if (ReadOnly)
            {
                m_floatArray = toCopy.m_floatArray;
            }
            else
            {
                m_floatArray = (float[])toCopy.m_floatArray.Clone();
            }
        }
    }

    public static class KC_FloatArray_Extensions
    {
        public static IList<float> GetFloatArray<T>(this Unit unit) where T : KC_FloatArray
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().FloatArray;
            }
            throw new InvalidOperationException($"GetFloatArray<{typeof(T).Name}> called on unit that does not have a component with KC_FloatArray parent.");
        }

        public static void SetFloatArray<T>(this Unit unit, float[] floats) where T : KC_FloatArray
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().FloatArray = floats;
            }
            throw new InvalidOperationException($"SetFloatArray<{typeof(T).Name}> called on unit that does not have a component with KC_FloatArray parent.");
        }
    }

}
