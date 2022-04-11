using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Schema;
using UnityEngine;

namespace SchemaEditor
{
    public static class EditorInspector
    {
        public static List<string> GetValuesGUI(this Decorator decorator)
        {
            List<string> ret = new List<string>();
            ret.AddRange(decorator.GetFieldValuesGUI());
            ret.AddRange(decorator.GetPropValuesGUI());

            return ret;
        }
        private static IEnumerable<string> GetPropValuesGUI(this Decorator decorator)
        {
            Type dType = decorator.GetType();

            IEnumerable<PropertyInfo> properties = dType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(decorator.IsInfoAttributeDefined);

            return (from property in properties select property.GetValue(decorator) into propValue where propValue != null select propValue.ToString()).ToList();
        }

        private static IEnumerable<string> GetFieldValuesGUI(this Decorator decorator)
        {
            Type dType = decorator.GetType();

            IEnumerable<FieldInfo> fields = dType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(decorator.IsInfoAttributeDefined);

            return (from field in fields select field.GetValue(decorator) into fieldValue where fieldValue != null select fieldValue.ToString()).ToList();
        }
    }
}