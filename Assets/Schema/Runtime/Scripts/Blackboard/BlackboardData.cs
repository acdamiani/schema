using System;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class BlackboardData
{
	private SchemaAgent agent;
	private Dictionary<string, object> values = new Dictionary<string, object>();
	public void Initialize(Blackboard blackboard)
	{
		foreach (BlackboardEntry entry in blackboard.entries)
		{
			object defaultValue = GetDefault(Type.GetType(entry.type));

			values.Add(entry.Name, defaultValue);
		}
	}
	private object GetDefault(Type t)
	{
		if (t.IsValueType)
		{
			return Activator.CreateInstance(t);
		}
		return null;
	}
	public object GetValue(string entryName)
	{
		if (string.IsNullOrEmpty(entryName))
		{
			return null;
		}

		if (values.ContainsKey(entryName))
		{
			return values[entryName];
		}
		else
		{
			return null;
		}
	}
	public T GetValue<T>(string entryName)
	{
		if (values.ContainsKey(entryName))
		{
			return (T)values[entryName];
		}
		else
		{
			return default(T);
		}
	}
	public void SetValue<T>(string entryName, object value)
	{
		if (value == null)
		{
			values[entryName] = null;
			return;
		}

		if (value.GetType() == typeof(T))
		{
			values[entryName] = value;
		}
		else
		{
			throw new UnityException($"Cannot set the value of blackboard key {entryName} because the types are not equivalent.");
		}
	}
}
