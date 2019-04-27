namespace CSA.Core
{
    /*
     * Components define groups of related Properties and Methods that can be added to a unit. Knowledge sources trigger
     * on the presence of an appropriate component    
     */
    public class KnowledgeComponent
    {
        public Unit ContainingUnit { get; set; }

        // fixme: keep the constructor for now until I decide what else might live in component.
        public KnowledgeComponent()
        {
            
        }
    }
}
