﻿using Newtonsoft.Json;
using System;
using System.Text;

namespace CSA.Core
{
    /*
     * Abstract KnowledgeComponent storing an array of strings.
     *
     * Abstract classes do not support JSON, so no distinguishing property. 
     */
    public abstract class KC_StringArray : KC_ReadOnly
    {
        private string[] m_stringArray;

        /*
         * Don't want StringArray to be output when serializing subclasses to Json.
         */
        [JsonIgnore]
        public string[] StringArray
        {
            get
            {
                // fixme: eventually test ReadOnly and return m_stringArray wrapped in ReadOnlyCollection<string> if it is readonly
                return m_stringArray;
            }

            set
            {
                if (!ReadOnly)
                {
                    m_stringArray = value;
                }
                else
                {
                    throw new InvalidOperationException("Attempt to set the StringArray property of a readOnly KC_StringArray");
                }
            }
        }

        protected string ToString(string propName)
        {
            StringBuilder sb = new StringBuilder(100);

            sb.AppendFormat("({0}: [", propName);
            foreach (string s in StringArray)
            {
                sb.AppendFormat(" {0}", s);
            }
            sb.Append(" ]");
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

        protected KC_StringArray()
        {
            m_stringArray = null;
        }

        protected KC_StringArray(string[] strings)
        {
            m_stringArray = strings;
        }

        protected KC_StringArray(string[] strings, bool readOnly) : base(readOnly)
        {
            m_stringArray = strings;
        }

        protected KC_StringArray(KC_StringArray toCopy) : base(toCopy)
        {
            /*
             * If the string[] is readonly, just copy the reference to the array, otherwise create a copy of the array (so that changing elements of the array copy
             * doesn't change elements of the original). 
             * fixme: for this to completely work, need to address the get accessor fixme above. 
             */
            if (ReadOnly)
            {
                m_stringArray = toCopy.m_stringArray;
            }
            else
            {
                m_stringArray = (string[])toCopy.m_stringArray.Clone();
            }
        }
    }

    public static class KC_StringArray_Extensions
    {
        public static string[] GetStringArray<T>(this Unit unit) where T : KC_StringArray
        {
            if (unit.HasComponent<T>())
            {
                return unit.GetComponent<T>().StringArray;
            }
            throw new InvalidOperationException($"GetStringArray<{typeof(T).Name}> called on unit that does not have a component with KC_StringArray parent.");
        }

        public static void SetStringArray<T>(this Unit unit, string[] strings) where T : KC_StringArray
        {
            if (unit.HasComponent<T>())
            {
                unit.GetComponent<T>().StringArray = strings;
            }
            throw new InvalidOperationException($"SetStringArray<{typeof(T).Name}> called on unit that does not have a component with KC_StringArray parent.");
        }
    }
}
