using System;

namespace EnsoExtension
{
    public class EnsoCommand : IComparable<EnsoCommand>
    {
        private string name;
        private string postfix;
        private string description;
        private string help;
        private EnsoPostfixType postfixType;
        private string expression;

        public EnsoCommand(string name, string postfix, string description, string help, EnsoPostfixType postfixType)
        {
            if (name == null)
                throw new ArgumentNullException("command");

            if (postfix == null && (postfixType == EnsoPostfixType.Arbitrary || postfixType == EnsoPostfixType.Bounded))
                throw new ArgumentNullException("postfix");

            if (description == null)
                throw new ArgumentNullException("description");

            if (help == null)
                throw new ArgumentNullException("help");

            this.name = name;
            this.postfix = postfix;
            this.description = description;
            this.help = help;
            this.postfixType = postfixType;
        }

        public string Name
        {
            get { return name; }
        }

        public string Postfix
        {
            get { return postfix; }
        }

        public string Description
        {
            get { return description; }
        }

        public string Help
        {
            get { return help; }
        }

        public EnsoPostfixType PostfixType
        {
            get { return postfixType; }
        }

        public override string ToString()
        {
            if (expression == null)
            {
                if (postfixType == EnsoPostfixType.None)
                    expression = name.ToLower();
                else
                    expression = String.Format("{0} {{{1}}}", name.ToLower(), postfix.ToLower());
            }
            return expression;
        }

        public override bool Equals(Object obj)
        {
            if (Object.ReferenceEquals(this, obj))
                return true;
            EnsoCommand other = obj as EnsoCommand;
            if (other == null)
                return false;
            return String.Equals(ToString(), other.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        #region IComparable<EnsoCommand> Members

        int IComparable<EnsoCommand>.CompareTo(EnsoCommand other)
        {
            return String.CompareOrdinal(ToString(), other.ToString());
        }

        #endregion
    }
}
