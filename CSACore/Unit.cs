﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public bool AddComponent(KnowledgeComponent component)
        {

            if (LookupComponents(component, out ISet<KnowledgeComponent> components))
            {
                if (components.Add(component))
                {
                    component.ContainingUnit = this;
                    return true;
                }
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

        public bool RemoveComponent(KnowledgeComponent component)
        {
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
            return false;
        } 

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
         * fixme: just for seeing what dumping the dictionary looks like, adding a public accessor for the dictionary.
         */
        public IDictionary<string, ISet<KnowledgeComponent>> GetDictionary() => m_components;

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
    }
}