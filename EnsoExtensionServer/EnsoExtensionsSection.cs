using System;
using System.Configuration;

namespace EnsoExtension
{
    public class EnsoExtensionsSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public EnsoExtensionsCollection EnsoExtensions
        {
            get { return (EnsoExtensionsCollection)base[""]; }
        }
    }

    public class EnsoExtensionsCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "ensoExtension"; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new EnsoExtensionElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((EnsoExtensionElement)element).Type;
        }
    }

    public class EnsoExtensionElement : ConfigurationElement
    {
        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }
    }
}
