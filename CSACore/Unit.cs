using System.Collections.Generic;
using System.Text;

namespace CSA.Core
{
    // Base class for all Blackboard Units.
    public class Unit : IUnit
    {
        /*
         * fixme: keeping this slot infrastructure here for now so as not to break the code that depends on it.
         * But it should be deleted, and all code changed to use a component model.         
         */
        public IDictionary<string, object> Slots { get; }

        public bool HasSlot(string propName)
        {
            return Slots.ContainsKey(propName);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);

            sb.AppendLine("Unit:");

            // Properties
            sb.AppendLine("\tProperties:");
            foreach (KeyValuePair<string, object> kvp in Slots)
            {
                sb.AppendLine("\t\t" + kvp.Key + " = " + kvp.Value);
            }

            return sb.ToString();
        }

        /*
         * Units are composed of components that define their behavior. Components are stored in a dictionary mapping 
         * from component type name to a set of components.         
         */

        protected IDictionary<string, object> m_components;

        /*
        public bool AddComponent(KnowledgeComponent component)
        {

            if (LookupUnits(component, out ISet<KnowledgeComponent> components))
            {
                return components.Add(component);
            }
            else
            {
                ISet<KnowledgeComponent> newComponents = new HashSet<KnowledgeComponent>
                {
                    component
                };
                m_components.Add(GetUnitTypeName(component), newComponents);
                return true;
            }
        }


        private bool LookupUnits(KnowledgeComponent component, out ISet<KnowledgeComponent> components)
        {
            return m_components.TryGetValue(GetUnitTypeName(component), out c);
        }

        protected string GetUnitTypeName(KnowledgeComponent component)
        {
            return component.GetType().FullName;
        }
        */
        public Unit()
        {
            Slots = new Dictionary<string, object>();
            //m_components = new Dictionary<string, object>();
        }

        public Unit(IUnit u)
        {
            Slots = new Dictionary<string, object>(u.Slots);
           //m_components = u.CopyComponents(); 
        }
    }
}