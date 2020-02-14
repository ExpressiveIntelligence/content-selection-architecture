using System;
namespace CSA.Core
{
    /*
     * Custom Attribute used to mark a property of a KnowledgeComponent to indicate that this property 
     * uniquely identifies the KnowledgeComponent type. 
     */
    [AttributeUsage(AttributeTargets.Property)]
    public class DistinguishingPropertyAttribute : Attribute
    {
        public DistinguishingPropertyAttribute()
        {
            
        }
    }
}
