using System;
using System.Collections.Generic;

namespace CSA.Core
{
    public interface IUnit
    {
        [Obsolete("Use KnowledgeComponents instead of slots.")]
        IDictionary<string, object> Slots { get; }

        [Obsolete("Call HasComponent<T> instead.")]
        bool HasSlot(string slotName);

        bool AddComponent(KnowledgeComponent component);
        bool RemoveComponent(KnowledgeComponent component);
        T GetComponent<T>() where T : KnowledgeComponent;
        ISet<T> GetComponents<T>() where T : KnowledgeComponent;
        bool HasComponent<T>() where T : KnowledgeComponent;
    }
}