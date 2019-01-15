/*
 * fixme: REMOVED from solution!! Added a Properties property to Unit that is an IDictionary<string, object>. Directly use IDictionary's methods
 * rather than going through the middle man of DynamicProperties.
 */

using System.Collections.Generic;

namespace CSACore
{
    public class DynamicProperties
    {
        public IDictionary<string, object> Properties { get; }

        public bool HasProperty(string propertyName) => Properties.ContainsKey(propertyName);

        public void SetProperty(string propertyName, object value) => Properties[propertyName] = value;

        public object GetProperty(string propertyName) => Properties[propertyName];

        public DynamicProperties()
        {
            Properties = new Dictionary<string, object>();
        }

        public DynamicProperties(DynamicProperties d)
        {
            foreach (KeyValuePair<string, object> kvp in d.Properties)
            {
                Properties[kvp.Key] = kvp.Value;
            }
        }
    }
}
