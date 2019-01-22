using System.Collections.Generic;

namespace CSACore
{
    public class ContentUnit : Unit
    {
        public static new string TypeName { get; } = new ContentUnit().GetType().FullName;

        public IDictionary<string, object> Metadata { get; }

        public IDictionary<string, object> Content { get; }

        public bool HasContentSlot(string slotName) => Content.ContainsKey(slotName);

        public bool HasMetadataSlot(string slotName) => Metadata.ContainsKey(slotName);

        public ContentUnit()
        {
            Metadata = new Dictionary<string, object>();
            Content = new Dictionary<string, object>(); 
        }

        public ContentUnit(ContentUnit c) : base(c)
        {
            Metadata = new Dictionary<string, object>();
            Content = new Dictionary<string, object>();

            foreach(KeyValuePair<string, object> kvp in c.Metadata)
            {
                Metadata[kvp.Key] = kvp.Value;
            }

            foreach(KeyValuePair<string, object> kvp in c.Content)
            {
                Content[kvp.Key] = kvp.Value;
            }
        }
    }
}
