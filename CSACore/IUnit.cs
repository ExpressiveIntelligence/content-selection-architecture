using System.Collections.Generic;

namespace CSA.Core
{
    public interface IUnit
    {
        IDictionary<string, object> Slots { get; }

        bool HasSlot(string slotName);
    }
}