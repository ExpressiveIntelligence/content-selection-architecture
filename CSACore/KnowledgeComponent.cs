using Newtonsoft.Json;
using System;

namespace CSA.Core
{
    /*
     * Components define groups of related Properties and Methods that can be added to a unit. Knowledge sources trigger
     * on the presence of an appropriate component    
     */
    public abstract class KnowledgeComponent : ICloneable
    {
        [JsonIgnore]
        public Unit ContainingUnit { get; internal set; }

        /*
         * When cloning KnowledgeComponent, ContainingUnit should be set equal to null (which it will be by default constructor) so that it's not pointing at the  
         * Unit being cloned. It will be set to the correct unit by AddComponent (called in the copy constructor of Unit). 
         */
        public abstract object Clone();

        protected KnowledgeComponent()
        {
            ContainingUnit = null;
        }
    }
}
