using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

/*
 * fixme: this reference to CSA.KnowledgeUnits should eventually be eliminated by folding all KCs into CSA.Core and
 * renaming Core, perhaps by renaming it something like Data (indicating classes related to storing data on the blackboard).
 */
// using CSA.KnowledgeUnits;

namespace CSA.Core
{
    // Base class for all Blackboard Units.
    public class Unit : IUnit
    {
        /*
         * fixme: keeping this slot infrastructure here for now so as not to break the code that depends on it.
         * But it should be deleted, and all code changed to use a component model.         
         */
        [Obsolete("Use KnowledgeComponents instead of slots.")]
        public IDictionary<string, object> Slots { get; }

        [Obsolete("Call HasComponent<T> instead.")]
        public bool HasSlot(string propName)
        {
            return Slots.ContainsKey(propName);
        }

        /*
         * An emumerable collection of all the KnowledgeComponent types. Used in the process of deserializing from Json. 
         */
        private static IEnumerable<Type> KcTypes { get; }

        /*
         * Creates a readable string representation of the Unit. Implicitly calls ToString on each of the KnowledgeComponents. 
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);

            sb.AppendLine("Unit:");

            // fixme: commenting out printing properties for now. Eventually properties will be completely replaced by KnowledgeComponents. 
            // Properties
            /* sb.AppendLine("\tProperties:");
            foreach (KeyValuePair<string, object> kvp in Slots)
            {
                sb.AppendLine("\t\t" + kvp.Key + " = " + kvp.Value);
            } */

            foreach(ISet<KnowledgeComponent> value in m_components.Values)
            {
                sb.Append("   [" + value.First().GetType().Name + ": ");
                foreach(KnowledgeComponent component in value)
                {
                    sb.Append(component);
                }
                sb.AppendLine("]");
            }
            return sb.ToString();
        }

        /*
         * Units are composed of components that define their behavior. Components are stored in a dictionary mapping 
         * from component type name to a set of components.         
         */
        protected IDictionary<string, ISet<KnowledgeComponent>> m_components;

        /*
         * Adds a KnowledgeComponent to the Unit. Returns true if this KnowledgeComponent is not already a member, and so
         * is added, false otherwise. 
         */
        public bool AddComponent(KnowledgeComponent component)
        {
            /*
             * If a KnowledgeComponent of this type has already been previously added, retrieve the HashSet associated
             * with this type and add the KnoweldgeComponent to the HashSet. 
             */
            if (LookupComponents(component, out ISet<KnowledgeComponent> components))
            {
                if (components.Add(component))
                {
                    component.ContainingUnit = this;
                    return true;
                }
                // The HashSet already contained this KnoweledgeComponent; return false. 
                return false;
            }

            // If a set for this component type has not been defined yet, create the set and add the component
            ISet<KnowledgeComponent> newComponents = new HashSet<KnowledgeComponent>
                {
                    component
                };
            m_components.Add(GetUnitTypeName(component), newComponents);
            component.ContainingUnit = this;
            return true;
        }

        /*
         * Removes a KnowledgeComponent from the Unit. Returns true if the Unit contained the KnowledgeComponent, and
         * thus it was successfully removed, false otherwise. 
         */
        public bool RemoveComponent(KnowledgeComponent component)
        {
            /*
             * Look up the HashSet associated with the specific type of the KnowledgeComponent and remove the KnowledgeComponent
             * from the HashSet. 
             */
            if (LookupComponents(component, out ISet<KnowledgeComponent> components))
            {
                if (components.Remove(component))
                {
                    component.ContainingUnit = null;
                    if (components.Count == 0)
                    {
                        m_components.Remove(GetUnitTypeName(component));
                    }
                    return true;
                }
            }
            /*
             * If either no KnowledgeComponent of this type has been previously added, or the attempt to remove
             * the KnowledgeComponent failed (so components.Remove() returns false), fall through to here and return false
             * meaning that the Unit did not contain the KnowledgeComponent that was requested to be removed. 
             */
            return false;
        } 

        /*
         * Returns the KnowledgeComponent of type T that has been registered with the Unit. This method assumes
         * singleton components. It throws an InvalidOperationException if more than one Component of this type
         * has been added. Returns the default value for T (which is null) if no KnowledgeComponent of type T has been
         * added to the Unit.  
         */
        public T GetComponent<T>() where T : KnowledgeComponent
        {
            if (m_components.TryGetValue(typeof(T).FullName, out ISet<KnowledgeComponent> components))
            {
                // Found at least one KnowledgeComponent of type T
                if (components.Count > 1)
                {
                    throw new InvalidOperationException("Unit.GetComponent called for KnowledgeComponent type with >1 components added to the unit.");
                }
                Debug.Assert(components.Count == 1);
                return (T)components.First();
            }
            else
            {
                return default(T); // No KnowledgeComponent of type T found in the Unit
            }
        }

        /*
         * Return the set of KnowledgeComponents of type T that have been added to this Unit. Creates a copy of the set associated
         * with type T so that the caller can't change the set within the Unit. 
         */
        public ISet<T> GetComponents<T>() where T : KnowledgeComponent
        {
            return m_components.TryGetValue(typeof(T).FullName, out ISet<KnowledgeComponent> components) ? new HashSet<T>(components.Cast<T>()) : new HashSet<T>();
        }

        /*
         * Returns true if the Unit contains a component of type T, false otherwise. 
         */
        public bool HasComponent<T>() where T : KnowledgeComponent
        {
            return m_components.TryGetValue(typeof(T).FullName, out var _);
        }

        /*
         * Given a KnowledgeComponent, returns (in an out variable) the current set of components belonging to this Unit
         * that share the same Knowledgecomponent type. 
         */
        private bool LookupComponents(KnowledgeComponent component, out ISet<KnowledgeComponent> components)
        {
            return m_components.TryGetValue(GetUnitTypeName(component), out components);
        }

        /*
         * Returns the type name as a strong of a KnowledgeComponent. Used as a key in the Dictionary of components. 
         */
        protected string GetUnitTypeName(KnowledgeComponent component)
        {
            return component.GetType().FullName;
        }

        /*
         * Returns an Enumerable of KnowledgeComponents that have been added to this unit. The Enumerable is made from the
         * items in the dictionary, so modifyng the Enumerable doesn't change the underlying Unit. 
         */
        public IEnumerable<KnowledgeComponent> GetComponents()
        {
            List<KnowledgeComponent> knowledgeComponents = new List<KnowledgeComponent>();

            foreach (HashSet<KnowledgeComponent> hs in m_components.Values)
            {
                knowledgeComponents.AddRange(hs);
            }
            return knowledgeComponents.AsReadOnly();
        }

        /*
         * Serializes a Unit to Json. Returns the Json serialization as a string. 
         */
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(GetComponents(), Formatting.Indented);
        }

        /*
         * Deserialize a Unit from Json.
         */
        public static Unit DeserializeFromJason(string jsonUnit)
        {
            // Create a new Unit that the deserialized KnowledgeComponents will be added to. 
            Unit deserializedUnit = new Unit();

            // Get a list of JObjects from jsonUnit. 
            var joList = JsonConvert.DeserializeObject<IList<JObject>>(jsonUnit);

            // For each JObject in the list, convert it to a KnowledgeComponent and add it to the Unit.
            foreach (JObject jo in joList)
            {
                KnowledgeComponent kc = ConvertJObjectToKC(jo);
                deserializedUnit.AddComponent(kc);
            }

            return deserializedUnit;
        }

        private static KnowledgeComponent ConvertJObjectToKC(JObject jo)
        {
            // Get the Assembly that contains KnoweldgeComponents.
            Assembly csaCoreAssembly = Assembly.GetAssembly(typeof(KnowledgeComponent));

            // Get the Type object representing the base type for all KnowledgeComponents. 
            Type kcBaseType = typeof(KnowledgeComponent);

            // Create an enumerable of all Type objects that are subclasses of knowledgeComponent
            var kcTypes = from Type t in csaCoreAssembly.GetTypes()
                          where t.IsSubclassOf(kcBaseType)
                          select t;

            /*
             * Iterate through all the KnowledgeComponent subtypes, examining the properties on each KnowledgeComponent
             * type to see if one of them is a DistinguishingProperty. If a DistinguishingProperty is found, search
             * for the property name in the JObject and deserialize if found. 
             */
            foreach (Type kcType in kcTypes)
            {
                // Get the PropertyInfo of the DistinguishingProperty of the kcType
                PropertyInfo prop = DistinguishingPropertyAttribute.GetDistinguishingProperty(kcType);

                // If a DistinguishingProperty was found, check to see if that property name is found in the JObject. 
                if (prop != null)
                {
                    if (jo.ContainsKey(prop.Name))
                    {
                        return jo.ToObject(kcType) as KnowledgeComponent;
                    }
                }
            }
            throw new ArgumentException("JObject argument not recognized as a deserializable KnowledgeComponent. " + jo);

        }

        public Unit()
        {
            // fixme: remove when pre-KC-based code eliminated
#pragma warning disable CS0618 // Type or member is obsolete
            Slots = new Dictionary<string, object>();
#pragma warning restore CS0618 // Type or member is obsolete

            m_components = new Dictionary<string, ISet<KnowledgeComponent>>();
        }

        public Unit(Unit u)
        {
            // fixme: remove when pre-KC-based code eliminated
#pragma warning disable CS0618 // Type or member is obsolete
            Slots = new Dictionary<string, object>(u.Slots);
#pragma warning restore CS0618 // Type or member is obsolete

            m_components = new Dictionary<string, ISet<KnowledgeComponent>>();

            // Iterate through all the components in the Unit being copied, adding 
            foreach(var compSet in u.m_components.Values)
            {
                foreach(var component in compSet)
                {
                    AddComponent((KnowledgeComponent)component.Clone());
                }
            }
        }

        /*
         * The static constructor is used to initialize the static enumerable collection of subtypes (represented as types)
         * of KnowledgeComponent. 
         */
        static Unit()
        {
            // fixme: need to do the class migration of KnowledgeComponent classes from CSA.KnowledgeUnits to CSA.Core now.
            // We can't introduce circular dependencies, and with the move to a component-based architecture for units, it makes
            // less sense to have a separate assembly for subtypes of Unit (since there aren't any now). 
            /*Assembly csaCoreAssembly = Assembly.GetAssembly(typeof(KC_Utility));
            Assembly kuAssembly = Assembly.GetAssembly(typeof(KC_ContentPool));

            Type kcBaseType = typeof(KnowledgeComponent);

            var allKCTypes = from Type t in csaCoreAssembly.GetTypes().Concat(kuAssembly.GetTypes())
                             where t.IsSubclassOf(kcBaseType)
                             select t;
            */
        }
    }
}