using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing a string. Typically used as textual content.  
     */
    public class KC_Text : KC_ImmutableString
    {

        public string Text
        {
            get => StringValue;

            set => StringValue = value;
        }

        public KC_Text()
        {
        }

        public KC_Text(string text) : base(text)
        {
        }

        public KC_Text(string text, bool immutable) : base(text, immutable)
        {
        }
    }

    public static class KC_Text_Extensions
    {
        public static string GetText(this Unit unit)
        {
            return unit.GetStringValue();
        }

        public static void SetText(this Unit unit, string text)
        {
            unit.SetStringValue(text);
        }
    }
}
