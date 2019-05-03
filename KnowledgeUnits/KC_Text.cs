using System.Text;
using CSA.Core;

namespace CSA.KnowledgeUnits
{
    /*
     * KnowledgeComponent for storing a string. Typically used as textual content.  
     */
    public class KC_Text : KC_ReadOnlyString
    {

        public string Text
        {
            get => StringValue;

            set => StringValue = value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(100);
            sb.Append("(Text: " + StringValue);
            if (ReadOnly)
            {
                sb.Append(", readonly)");
            }
            else
            {
                sb.Append(")");
            }
            return sb.ToString();
        }

        public override object Clone() => new KC_Text(this);

        public KC_Text()
        {
        }

        public KC_Text(string text) : base(text)
        {
        }

        public KC_Text(string text, bool readOnly) : base(text, readOnly)
        {
        }

        protected KC_Text(KC_Text toCopy) : base(toCopy)
        {
        }
    }

    public static class KC_Text_Extensions
    {
        public static string GetText(this Unit unit)
        {
            return unit.GetStringValue<KC_Text>();
        }

        public static void SetText(this Unit unit, string text)
        {
            unit.SetStringValue<KC_Text>(text);
        }

        public static bool TextEquals(this Unit unit, string s)
        {
            return unit.StringValueEquals<KC_Text>(s);
        }
    }
}
