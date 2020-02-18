using System;
using System.Reflection;

namespace CSA.Core
{
    /*
     * Custom Attribute used to mark a property of a KnowledgeComponent to indicate that this property 
     * uniquely identifies the KnowledgeComponent type. 
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class DistinguishingPropertyAttribute : Attribute
    {
        /*
         * Given a Type t (which must be a sublcass of KnowlegdgeComponent), return the PropertyInfo for the property marked
         * with [DistingushingProperty], null otherwise. 
         */
        public static PropertyInfo GetDistinguishingProperty(Type t)
        {
            // Make sure t is a subtype of KnowledgeComponent.
            if (!t.IsSubclassOf(typeof(KnowledgeComponent)))
            {
                throw new ArgumentException("The Type parameter to GetDistinguishingProperty must be a sublcass of KnoweledgeComponent. " + t);
            }

            // Iterate throught the properties, testing to see if one of them has DistinguishingProperty. 
            PropertyInfo[] properties = t.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach(PropertyInfo prop in properties)
            {
                var attr = prop.GetCustomAttribute<DistinguishingPropertyAttribute>();
                if (attr != null)
                {
                    // Found a marked property - return the property info
                    return prop;
                }
            }

            // Didn't find a DistinguishingProperty in the properties - return null.
            return null;
        }

        public DistinguishingPropertyAttribute()
        {
            
        }
    }
}
