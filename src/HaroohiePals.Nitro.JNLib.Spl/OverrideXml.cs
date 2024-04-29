/*
 * Copyright (C) 2014 Ivan Krivyakov, http://www.ikriv.com/
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 */

using System.Reflection;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl
{
    /// <summary>
    /// Creates XmlAttributeOverrides instance using an easy-to-use fluent interface
    /// </summary>
    internal class OverrideXml
    {
        private Type _currentType;
        private string _currentMember = "";
        private XmlAttributes _attributes;
        private readonly XmlAttributeOverrides _overrides = new XmlAttributeOverrides();

        /// <summary>
        /// Specifies that subsequent attributes wil be applied to type t
        /// </summary>
        public OverrideXml Override(Type t)
        {
            Commit();
            _currentType = t;
            _currentMember = "";
            return this;
        }

        /// <summary>
        /// Specifies that subsequent attributes wil be applied to type T
        /// </summary>
        public OverrideXml Override<T>()
        {
            return Override(typeof(T));
        }

        /// <summary>
        /// Specifies that subsequent attributes wil be applied to the given member of the current type
        /// </summary>
        public OverrideXml Member(string name)
        {
            Commit();
            if (_currentType == null) throw new InvalidOperationException("Current type is not defined. Use Override<T>() to define current type");

            // attempt to verify that such member indeed exists
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            if (_currentType.GetProperty(name, flags) == null && _currentType.GetField(name, flags) == null)
            {
                throw new InvalidOperationException("Property or field '" + name + "' does not exist in type " + _currentType.Name + " or is not public");
            }

            _currentMember = name;
            return this;
        }

        /// <summary>
        /// Constructs XmlAttributeOverrides instance from previously specified attributes
        /// </summary>
        public XmlAttributeOverrides Commit()
        {
            if (_attributes != null)
            {
                _overrides.Add(_currentType, _currentMember, _attributes);
                _currentMember = "";
                _attributes = null;
            }

            return _overrides;
        }

        /// <summary>
        /// Adds [XmlRoot(elementName)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlRoot(string elementName)
        {
            Open();
            _attributes.XmlRoot = new XmlRootAttribute(elementName);
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlRootAttribute for current type or member
        /// </summary>
        public OverrideXml Attr(XmlRootAttribute xmlRoot)
        {
            Open();
            _attributes.XmlRoot = xmlRoot;
            return this;
        }

        /// <summary>
        /// Adds [XmlAttribute] attribute to current type or member
        /// </summary>
        public OverrideXml XmlAttribute()
        {
            Open();
            _attributes.XmlAttribute = new XmlAttributeAttribute();
            return this;
        }

        /// <summary>
        /// Adds [XmlAttribute(name)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlAttribute(string name)
        {
            Open();
            _attributes.XmlAttribute = new XmlAttributeAttribute(name);
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlAttributeAttribute to current type or member
        /// </summary>
        public OverrideXml Attr(XmlAttributeAttribute attribute)
        {
            Open();
            _attributes.XmlAttribute = attribute;
            return this;
        }

        /// <summary>
        /// Adds [XmlElement] attribute to current type or member
        /// </summary>
        public OverrideXml XmlElement()
        {
            Open();
            _attributes.XmlElements.Add(new XmlElementAttribute());
            return this;
        }

        /// <summary>
        /// Adds [XmlElement(name)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlElement(string name)
        {
            Open();
            _attributes.XmlElements.Add(new XmlElementAttribute(name));
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlElementAttribute to current type or member
        /// </summary>
        public OverrideXml Attr(XmlElementAttribute attribute)
        {
            Open();
            _attributes.XmlElements.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds [XmlIgnore] attribute to current type or member
        /// </summary>
        /// <param name="bIgnore"></param>
        public OverrideXml XmlIgnore(bool bIgnore = true)
        {
            Open();
            _attributes.XmlIgnore = bIgnore;
            return this;
        }

        /// <summary>
        /// Adds [XmlAnyAttribute] attribute to current type or member
        /// </summary>
        public OverrideXml XmlAnyAttribute()
        {
            Open();
            _attributes.XmlAnyAttribute = new XmlAnyAttributeAttribute();
            return this;
        }

        /// <summary>
        /// Adds [XmlAnyElement] attribute to current type or member
        /// </summary>
        public OverrideXml XmlAnyElement()
        {
            Open();
            _attributes.XmlAnyElements.Add(new XmlAnyElementAttribute());
            return this;
        }

        /// <summary>
        /// Adds [XmlAnyElement(name)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlAnyElement(string name)
        {
            Open();
            _attributes.XmlAnyElements.Add(new XmlAnyElementAttribute(name));
            return this;
        }

        /// <summary>
        /// Adds [XmlAnyElement(name,ns)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlAnyElement(string name, string ns)
        {
            Open();
            _attributes.XmlAnyElements.Add(new XmlAnyElementAttribute(name, ns));
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlAnyElementAttribute to current type or member
        /// </summary>
        public OverrideXml Attr(XmlAnyElementAttribute attribute)
        {
            Open();
            _attributes.XmlAnyElements.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds [XmlArray] attribute to current type or memeber
        /// </summary>
        public OverrideXml XmlArray()
        {
            Open();
            _attributes.XmlArray = new XmlArrayAttribute();
            return this;
        }

        /// <summary>
        /// Adds [XmlArray(elementName)] attribute to current type or memeber
        /// </summary>
        public OverrideXml XmlArray(string elementName)
        {
            Open();
            _attributes.XmlArray = new XmlArrayAttribute(elementName);
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlArrayAttribute to current type or memeber
        /// </summary>
        public OverrideXml Attr(XmlArrayAttribute attribute)
        {
            Open();
            _attributes.XmlArray = attribute;
            return this;
        }

        /// <summary>
        /// Adds [XmlArrayItem] attribute to current type or member
        /// </summary>
        public OverrideXml XmlArrayItem()
        {
            Open();
            _attributes.XmlArrayItems.Add(new XmlArrayItemAttribute());
            return this;
        }

        /// <summary>
        /// Adds [XmlArrayItem(elementName)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlArrayItem(string elementName)
        {
            Open();
            _attributes.XmlArrayItems.Add(new XmlArrayItemAttribute(elementName));
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlArrayItemAttribute to current type or member
        /// </summary>
        public OverrideXml Attr(XmlArrayItemAttribute attribute)
        {
            Open();
            _attributes.XmlArrayItems.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds [XmlDefault(value)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlDefaultValue(object value)
        {
            Open();
            _attributes.XmlDefaultValue = value;
            return this;
        }

        /// <summary>
        /// Applies or removes [XmlNamespaceDeclarations] attribute from current type or member
        /// </summary>
        public OverrideXml Xmlns(bool value)
        {
            Open();
            _attributes.Xmlns = value;
            return this;
        }

        /// <summary>
        /// Adds [XmlText] attribute to current type or member
        /// </summary>
        public OverrideXml XmlText()
        {
            Open();
            _attributes.XmlText = new XmlTextAttribute();
            return this;
        }

        /// <summary>
        /// Adds [XmlType(typeName)] attribute to current type or member
        /// </summary>
        public OverrideXml XmlType(string typeName)
        {
            Open();
            _attributes.XmlType = new XmlTypeAttribute(typeName);
            return this;
        }

        /// <summary>
        /// Adds specified instance of XmlTypeAttribute to current type or member
        /// </summary>
        public OverrideXml Attr(XmlTypeAttribute attribute)
        {
            Open();
            _attributes.XmlType = attribute;
            return this;
        }

        private void Open()
        {
            if (_attributes == null) _attributes = new XmlAttributes();
        }
    }
}