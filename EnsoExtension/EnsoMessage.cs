using System;

namespace EnsoExtension
{
    public class EnsoMessage
    {
        private string text;
        private string subtext;

        public EnsoMessage(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            this.text = text;
        }

        public EnsoMessage(string text, string subtext)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            this.text = text;
            this.subtext = subtext;
        }

        public string Text
        {
            get { return text; }
            set
            {
                if (text == null)
                    throw new ArgumentNullException("value");
                text = value;
            }
        }

        public string Subtext
        {
            get { return subtext; }
            set { subtext = value; }
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(subtext))
                return String.Format("<p>{0}</p>", text);
            else
                return String.Format("<p>{0}</p><caption>{1}</caption>", text, subtext);
        }
    }
}
