using System;
using System.Linq;
using System.Reflection;
using Schema.Utilities;
using UnityEngine;

namespace Schema
{
    public abstract class EntryType
    {
        public static string GetName(Type entryType)
        {
            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            string customName = entryType.GetCustomAttribute<NameAttribute>()?.name;

            if (string.IsNullOrEmpty(customName))
                customName = entryType.GetFriendlyTypeName();

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
            if (entryType == null)
                return null;

            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            Type mappedType = entryType.GetCustomAttribute<UseExternalTypeDefinitionAttribute>()?.other;

            return mappedType ?? entryType;
        }

        public static string[] GetExcludedPaths(Type entryType)
        {
            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            string[] paths = entryType.GetCustomAttributes<ExcludePathsAttribute>()?.Select(x => x.excludedPaths)
                .SelectMany(x => x).ToArray();

            return paths ?? new string[0];
        }

        public static Type[] GetExcludedTypes(Type entryType)
        {
            if (!typeof(EntryType).IsAssignableFrom(entryType))
                throw new ArgumentException($"{entryType.Name} does not inherit from EntryType");

            Type[] types = entryType.GetCustomAttributes<ExcludeTypesAttribute>()?.Select(x => x.excludedTypes)
                .SelectMany(x => x).ToArray();

            return types ?? new Type[0];
        }

        /// <summary>
        ///     Use this attribute to use an external type's fields and properties instead of this class.
        ///     Use if you want to define an EntryType for a type you do not have access to.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class UseExternalTypeDefinitionAttribute : Attribute
        {
            public Type other;

            /// <summary>
            ///     Use this attribute to use an external type's fields and properties instead of this class.
            ///     Use if you want to define an EntryType for a type you do not have access to.
            /// </summary>
            /// <param name="other">Other type that you want to use to define the entry</param>
            public UseExternalTypeDefinitionAttribute(Type other)
            {
                this.other = other;
            }
        }

        /// <summary>
        ///     Define a custom type name for this EntryType
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class NameAttribute : Attribute
        {
            public string name;

            /// <summary>
            ///     Define a custom type name for this EntryType
            /// </summary>
            /// <param name="name">Custom name to use for this EntryType</param>
            public NameAttribute(string name)
            {
                this.name = name;
            }
        }

        /// <summary>
        ///     Define paths to exclude from Dynamic Properties
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        protected class ExcludePathsAttribute : Attribute
        {
            public string[] excludedPaths;

            /// <summary>
            ///     Define paths to exclude from Dynamic Properties
            /// </summary>
            /// <param name="excludedPaths">Paths to exclude from Dynamic Properties</param>
            public ExcludePathsAttribute(params string[] excludedPaths)
            {
                this.excludedPaths = excludedPaths;
            }
        }

        /// <summary>
        ///     Define types to exclude form Dynamic Properties
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        protected class ExcludeTypesAttribute : Attribute
        {
            public Type[] excludedTypes;

            /// <summary>
            ///     Define types to exclude form Dynamic Properties
            /// </summary>
            /// <param name="excludedTypes">Types to exclude from Dynamic Properties</param>
            public ExcludeTypesAttribute(params Type[] excludedTypes)
            {
                this.excludedTypes = excludedTypes;
            }
        }

        /// <summary>
        ///     Define a custom color to use in the inspector
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class ColorAttribute : Attribute
        {
            public Color32 color;

            /// <summary>
            ///     Define a custom color to use in the inspector
            /// </summary>
            /// <param name="r">The red component of the color (0-255)</param>
            /// <param name="g">The green component of the color (0-255)</param>
            /// <param name="b">The blue component of the color (0-255)</param>
            /// <param name="a">The alpha component of the color (0-255)</param>
            public ColorAttribute(byte r, byte g, byte b, byte a = 255)
            {
                color = new Color32(r, g, b, a);
            }

            /// <summary>
            ///     Define a custom color to use in the inspector
            /// </summary>
            /// <param name="hex">The hex value to use for the color</param>
            public ColorAttribute(string hex)
            {
                bool b = ColorUtility.TryParseHtmlString(hex, out Color col);
                color = b ? col : Color.white;
            }
        }
    }
}