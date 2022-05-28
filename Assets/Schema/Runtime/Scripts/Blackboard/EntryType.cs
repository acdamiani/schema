using UnityEngine;
using System;
using System.Reflection;
using Schema.Utilities;

namespace Schema
{
    public abstract class EntryType
    {
        public static string GetName(Type entryType)
        {
            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            string customName = entryType.GetCustomAttribute<NameAttribute>()?.name;

            if (String.IsNullOrEmpty(customName))
                customName = HelperMethods.GetFriendlyTypeName(entryType);

            return customName;
        }
        public static Color32 GetColor(Type entryType)
        {
            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            Color32? customColor = entryType.GetCustomAttribute<ColorAttribute>()?.color;

            if (customColor == null)
                customColor = entryType.Name.Hash().ToColor();

            return (Color32)customColor;
        }
        public static Type GetMappedType(Type entryType)
        {
            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            Type mappedType = entryType.GetCustomAttribute<UseExternalTypeDefinitionAttribute>()?.other;

            return mappedType ?? entryType;
        }
        /// <summary>
        /// Use this attribute to use an external type's fields and properties instead of this class. 
        /// Use if you want to define an EntryType for a type you do not have access to.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        protected class UseExternalTypeDefinitionAttribute : Attribute
        {
            public Type other;
            /// <summary>
            /// Use this attribute to use an external type's fields and properties instead of this class. 
            /// Use if you want to define an EntryType for a type you do not have access to.
            /// </summary>
            /// <param name="other">Other type that you want to use to define the entry</param>
            public UseExternalTypeDefinitionAttribute(Type other)
            {
                this.other = other;
            }
        }
        /// <summary>
        /// Define a custom type name for this EntryType
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        protected class NameAttribute : Attribute
        {
            public string name;
            /// <summary>
            /// Define a custom type name for this EntryType
            /// </summary>
            /// <param name="name">Custom name to use for this EntryType</param>
            public NameAttribute(string name)
            {
                this.name = name;
            }
        }
        /// <summary>
        /// Define paths to exclude from Dynamic Properties
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class ExcludePathsAttribute : Attribute
        {
            public string[] excludedPaths;
            /// <summary>
            /// Define paths to exclude from Dynamic Properties
            /// </summary>
            /// <param name="excludedPaths">Paths to exclude from Dynamic Properties</param>
            public ExcludePathsAttribute(string[] excludedPaths)
            {
                this.excludedPaths = excludedPaths;
            }
        }
        /// <summary>
        /// Define a custom color to use in the inspector
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        protected class ColorAttribute : Attribute
        {
            public Color32 color;
            public ColorAttribute(byte r, byte g, byte b, byte a = 255)
            {
                this.color = new Color32(r, g, b, a);
            }
        }
    }
}