using System;
using System.Text;
using System.Collections.Generic;

namespace CSA.Core
{
    [Obsolete("Replace with Unit using appropriate KnowledgeComponents.")]
    public class ContentUnit : Unit
    {
        // fixme: consider changing string to object so that hashes and comparisons are more efficient
        public IDictionary<string, object> Metadata { get; }

        public IDictionary<string, object> Content { get; }

        public bool HasContentSlot(string slotName) => Content.ContainsKey(slotName);

        public bool HasMetadataSlot(string slotName) => Metadata.ContainsKey(slotName);

        public ContentUnit()
        {
            Metadata = new Dictionary<string, object>();
            Content = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);

            sb.AppendLine("ContentUnit:");

            // Metadata
            sb.AppendLine("\tMetadata:");
            foreach (KeyValuePair<string, object> kvp in Metadata)
            {
                sb.AppendLine("\t\t" + kvp.Key + " = " + kvp.Value);
            }

            // Content
            sb.AppendLine("\tContent");
            foreach(KeyValuePair<string, object> kvp in Content)
            {
                sb.AppendLine("\t\t" + kvp.Key + " = " + kvp.Value);
            }

            return sb.ToString();
        }

        public ContentUnit(ContentUnit c) : base(c)
        {
            Metadata = new Dictionary<string, object>();
            Content = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kvp in c.Metadata)
            {
                Metadata[kvp.Key] = kvp.Value;
            }

            foreach (KeyValuePair<string, object> kvp in c.Content)
            {
                Content[kvp.Key] = kvp.Value;
            }
        }
    }
}
