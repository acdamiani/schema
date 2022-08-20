using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

public static class DynamicProperty
{
    private static readonly Dictionary<MemberInfo, Action<object, object>> setters =
        new Dictionary<MemberInfo, Action<object, object>>();

    private static readonly Dictionary<MemberInfo, Func<object, object>> getters =
        new Dictionary<MemberInfo, Func<object, object>>();

    public static void Set(object obj, string path, object value)
    {
        if (obj == null || string.IsNullOrEmpty(path))
            throw new ArgumentNullException();

        Type type = obj.GetType();

        string assemblyName = type.AssemblyQualifiedName;
        string pathSuffix = '.' + path.Replace('/', '.').Trim('.');

        Queue<string> pathParts = new Queue<string>(path.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)));

        object currentObject = ValueTypeHolder.Wrap(obj);
        Type currentType = type;

        MemberInfo member;
        string part;

        while (pathParts.Count > 1)
        {
            part = pathParts.Dequeue();

            member = (MemberInfo)currentType.GetField(part) ?? currentType.GetProperty(part);

            if (member == null)
            {
                Debug.LogWarning($"{type.Name}{pathSuffix} does not exist");
                return;
            }

            getters.TryGetValue(member, out Func<object, object> getter);

            if (getter == null)
                getter = getters[member] = CreateGetMethod(member);

            currentObject = ValueTypeHolder.Wrap(getter(currentObject));
            currentType = (member as FieldInfo)?.FieldType ?? (member as PropertyInfo)?.PropertyType;
        }

        part = pathParts.Dequeue();
        member = (MemberInfo)currentType.GetField(part) ?? currentType.GetProperty(part);

        if (member.MemberType == MemberTypes.Property && (member as PropertyInfo).SetMethod == null)
        {
            Debug.LogWarning($"{type.Name}{pathSuffix} is not writable; it is readonly");
            return;
        }

        if (member.MemberType == MemberTypes.Field && (member as FieldInfo).DeclaringType.IsValueType)
        {
            Debug.LogWarning($"{type.Name}{pathSuffix} cannot be assigned to because it is not a variable");
            return;
        }

        setters.TryGetValue(member, out Action<object, object> setter);

        if (setter == null)
            setter = setters[member] = CreateSetMethod(member);

        setter(currentObject, value);
    }

    public static object Get(object obj, string path)
    {
        if (obj == null || string.IsNullOrEmpty(path))
            throw new ArgumentNullException();

        Type type = obj.GetType();

        string assemblyName = type.AssemblyQualifiedName;
        string pathSuffix = '.' + path.Replace('/', '.').Trim('.');

        Queue<string> pathParts = new Queue<string>(path.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)));

        object currentObject = ValueTypeHolder.Wrap(obj);
        Type currentType = type;

        MemberInfo member;
        string part;

        for (int i = pathParts.Count - 1; i >= 0; i--)
        {
            part = pathParts.Dequeue();

            member = (MemberInfo)currentType.GetField(part) ?? currentType.GetProperty(part);

            if (member == null)
            {
                Debug.LogWarning($"{type.Name}{pathSuffix} does not exist");
                return null;
            }

            getters.TryGetValue(member, out Func<object, object> getter);

            if (getter == null)
                getter = getters[member] = CreateGetMethod(member);

            currentObject = getter(currentObject);

            if (i == 0)
                return currentObject;

            currentObject = ValueTypeHolder.Wrap(currentObject);

            currentType = (member as FieldInfo)?.FieldType ?? (member as PropertyInfo)?.PropertyType;
        }

        return null;
    }

    private static Func<object, object> CreateGetMethod(MemberInfo memberInfo)
    {
        #if ENABLE_MONO && !NET_STANDARD_2_0
        string methodName = memberInfo.ReflectedType.FullName + ".get_" + memberInfo.Name;
        DynamicMethod getterMethod =
            new DynamicMethod(methodName, typeof(object), new Type[1] { typeof(object) }, true);
        ILGenerator gen = getterMethod.GetILGenerator();

        Type targetType = memberInfo.DeclaringType;
        bool shouldHandleInnerStruct = targetType.IsValueType;

        if (shouldHandleInnerStruct)
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.DeclareLocal(targetType);
            gen.Emit(OpCodes.Castclass, typeof(ValueTypeHolder));
            gen.Emit(OpCodes.Callvirt, ValueTypeHolder.GetMethod);
            gen.Emit(OpCodes.Unbox_Any, targetType);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloca_S, 0);
            gen.DeclareLocal(typeof(object));
        }
        else
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, targetType);
        }

        if (memberInfo.MemberType == MemberTypes.Field)
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;

            gen.Emit(OpCodes.Ldfld, fieldInfo);
            if (fieldInfo.FieldType.IsValueType)
                gen.Emit(OpCodes.Box, fieldInfo.FieldType);
        }
        else
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            MethodInfo getMethod = propertyInfo.GetGetMethod();

            gen.Emit(targetType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, getMethod);
            if (propertyInfo.PropertyType.IsValueType)
                gen.Emit(OpCodes.Box, propertyInfo.PropertyType);
        }

        if (shouldHandleInnerStruct)
        {
            gen.Emit(OpCodes.Stloc_1);
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, typeof(ValueTypeHolder));
            gen.Emit(OpCodes.Ldloc_0);
            if (targetType.IsValueType)
                gen.Emit(OpCodes.Box, targetType);
            gen.Emit(OpCodes.Callvirt, ValueTypeHolder.SetMethod);
            gen.Emit(OpCodes.Ldloc_1);
        }

        gen.Emit(OpCodes.Ret);

        return (Func<object, object>)getterMethod.CreateDelegate(typeof(Func<object, object>));
        #else
        if (memberInfo is FieldInfo)
        {
            return new Func<object, object>(obj => (memberInfo as FieldInfo).GetValue(obj));
        }
        else if (memberInfo is PropertyInfo)
        {
            return new Func<object, object>(obj => (memberInfo as PropertyInfo).GetValue(obj));
        }
        else
        {
            return new Func<object, object>(o => null);
        }
        #endif
    }

    private static Action<object, object> CreateSetMethod(MemberInfo memberInfo)
    {
        #if ENABLE_MONO && !NET_STANDARD_2_0
        string methodName = memberInfo.ReflectedType.FullName + ".set_" + memberInfo.Name;
        DynamicMethod setterMethod =
            new DynamicMethod(methodName, null, new Type[2] { typeof(object), typeof(object) }, true);
        ILGenerator gen = setterMethod.GetILGenerator();

        Type targetType = memberInfo.DeclaringType;
        Type memberType = (memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType;
        bool shouldHandleInnerStruct = targetType.IsValueType;

        gen.Emit(OpCodes.Ldarg_0);

        if (shouldHandleInnerStruct)
        {
            gen.DeclareLocal(targetType);
            gen.Emit(OpCodes.Castclass, typeof(ValueTypeHolder));
            gen.Emit(OpCodes.Callvirt, ValueTypeHolder.GetMethod);
            gen.Emit(OpCodes.Unbox_Any, targetType);
            gen.Emit(OpCodes.Stloc_0);
            gen.Emit(OpCodes.Ldloca_S, 0);
            gen.Emit(OpCodes.Ldarg_1);
        }
        else
        {
            gen.Emit(OpCodes.Castclass, targetType);
            gen.Emit(OpCodes.Ldarg_1);
        }

        if (memberType != typeof(object))
        {
            if (memberType.IsValueType)
                gen.Emit(OpCodes.Unbox_Any, memberType);
            else
                gen.Emit(OpCodes.Castclass, memberType);
        }

        if (memberInfo.MemberType == MemberTypes.Field)
        {
            FieldInfo fieldInfo = memberInfo as FieldInfo;
            gen.Emit(OpCodes.Stfld, fieldInfo);
        }
        else
        {
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            MethodInfo setMethod = propertyInfo.GetSetMethod();
            if (targetType.IsValueType)
                gen.Emit(OpCodes.Call, setMethod);
            else
                gen.Emit(OpCodes.Callvirt, setMethod);
        }

        if (shouldHandleInnerStruct)
        {
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Castclass, typeof(ValueTypeHolder));
            gen.Emit(OpCodes.Ldloc_0);
            if (targetType.IsValueType)
                gen.Emit(OpCodes.Box, targetType);
            gen.Emit(OpCodes.Callvirt, ValueTypeHolder.SetMethod);
        }

        gen.Emit(OpCodes.Ret);

        return (Action<object, object>)setterMethod.CreateDelegate(typeof(Action<object, object>));

        #else
        if (memberInfo is PropertyInfo)
        {
            return new Action<object, object>((o1, o2) => (memberInfo as PropertyInfo).SetValue(o1, o2));
        }
        else
        {
            return new Action<object, object>((o1, o2) => { });
        }
        #endif
    }

    public class ValueTypeHolder
    {
        public static readonly MethodInfo GetMethod =
            typeof(ValueTypeHolder).GetMethod("get_Value", BindingFlags.Public | BindingFlags.Instance);

        public static readonly MethodInfo SetMethod =
            typeof(ValueTypeHolder).GetMethod("set_Value", BindingFlags.Public | BindingFlags.Instance);

        public ValueTypeHolder(object value)
        {
            Value = (ValueType)value;
        }

        public ValueType Value { get; set; }

        public static object Wrap(object obj)
        {
            return obj.GetType().IsValueType ? new ValueTypeHolder(obj) : obj;
        }
    }
}