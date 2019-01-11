using System.Collections.Generic;

namespace CSACore
{
    public class ContentUnit
    {
        public IDictionary<string, object> Metadata { get; }

        public IDictionary<string, object> Content { get; }

        public bool HasContentSlot(string slotName) => Content.ContainsKey(slotName);

        public bool HasMetadataSlot(string slotName) => Metadata.ContainsKey(slotName);

        public ContentUnit()
        {
            Metadata = new Dictionary<string, object>();
            Content = new Dictionary<string, object>(); 
        }

    }
}
