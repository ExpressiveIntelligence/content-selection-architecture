using System.Collections.Generic;

namespace CSA.Core
{
    public interface IUnit
    {
        IDictionary<string, object> Slots { get; }

        bool HasSlot(string slotName);

        bool AddComponent(KnowledgeComponent component);
        bool RemoveComponent(KnowledgeComponent component);
        T GetComponent<T>() where T : KnowledgeComponent;
        ISet<T> GetComponents<T>() where T : KnowledgeComponent;
        bool HasComponent<T>() where T : KnowledgeComponent;
        IDictionary<string, ISet<KnowledgeComponent>> CopyComponents();
    }
}