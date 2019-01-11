using System.Dynamic;
using System.Collections.Generic;

namespace CSACore
{
    public class ContentUnit
    {
        public ExpandoObject Metadata { get; }

        public ExpandoObject Content { get; }

        public bool HasContentSlot(string slotName) => ((IDictionary<string, object>)Content).ContainsKey(slotName);

        public bool HasMetadataSlot(string slotName) => ((IDictionary<string, object>)Metadata).ContainsKey(slotName);

        public ContentUnit()
        {
            Metadata = new ExpandoObject();
            Content = new ExpandoObject(); 
        }

    }
}
