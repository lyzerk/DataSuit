﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using DataSuit.Interfaces;
using System.Collections;
using DataSuit.Enums;
using DataSuit.Infrastructures;
using System.Reflection.Emit;
using DataSuit.Providers;

namespace DataSuit.Reflection
{
    internal class Mapper
    {
        public static T Map<T>(T val, ISettings settings, ISessionManager manager, bool recursion = true) where T : new()
        {
            // Usage of typeof(T) causes problems. The template class could be an object and the value could be anything
            // Therefore we can't get properties of an object type.
            var type = val.GetType();

            foreach (var item in type.GetTypeInfo().DeclaredProperties)
            {
                var propInfo = item.PropertyType.GetTypeInfo();

                if (propInfo.IsPrimitive || item.PropertyType == typeof(String))
                {
                    SetPrimitive(item, val, settings, manager);
                }
                else if (propInfo.IsGenericType)
                {
                    SetCollection(item, val, settings, manager, recursion);
                }
                else
                {

                }
            }

            return val;
        }

        private static object ProviderGetValue(IDataProvider provider, string name)
        {
            if (provider.Type == ProviderType.Json)
            {
                var jsonProvider = provider as IJsonProvider;
                var valueProvider = jsonProvider.TargetType.GetTypeInfo();

                if (valueProvider.IsClass)
                {
                    var subProp = valueProvider.DeclaredProperties.FirstOrDefault(i => i.Name.Equals(name));
                    return subProp.GetValue(provider.Current);
                }
            }

            return provider.Current;
        }

        private static void SetPrimitive<T>(PropertyInfo type, T val, ISettings settings, ISessionManager manager)
        {
            var provider = settings.Providers.FirstOrDefault(i => i.Key.Equals(type.Name.ToLower()));

            if (string.IsNullOrEmpty(provider.Key))
            {
                return;
            }

            provider.Value.MoveNext(manager);
            
            var value = ProviderGetValue(provider.Value, type.Name);

            type.SetValue(val, value);

        }
        private static void SetPrimitive<T>(string name, ref T val, ISettings settings, ISessionManager manager)
        {
            var provider = settings.Providers.FirstOrDefault(i => i.Key.Contains(name));

            if (string.IsNullOrEmpty(provider.Key))
            {
                //for now, it will change
                provider = settings.Providers.FirstOrDefault(i => i.Key.Contains(name));

                if (string.IsNullOrEmpty(provider.Key))
                    return;
            }

            var value = ProviderGetValue(provider.Value, name);

            val = (T)value;

            provider.Value.MoveNext(manager);
        }

        private static void SetCollection<T>(PropertyInfo type, T val, ISettings settings, ISessionManager manager, bool recursion = true)
        {
            var collectionType = typeof(List<>);

            //If property is List<T>
            if (collectionType.GetGenericTypeDefinition() == type.PropertyType.GetGenericTypeDefinition())
            {
                var arg = type.PropertyType.GenericTypeArguments[0];
                var argInfo = arg.GetTypeInfo();

                var collection = type.GetValue(val) as IList;

                // Fill the class as the relationship rule possible
                if (argInfo.IsPrimitive || arg == typeof(String))
                {
                    for (int i = 0; i < settings.Relationship.Value; i++)
                    {
                        // Fill the sub instance
                        object subIns = null;

                        SetPrimitive(type.Name, ref subIns, settings, manager);

                        collection.Add(subIns);
                    }
                }
                else if (argInfo.IsClass)
                {
                    if (recursion)
                    {
                        for (int i = 0; i < settings.Relationship.Value; i++)
                        {
                            // Fill the sub instance
                            var subIns = Activator.CreateInstance(arg);
                            Map(subIns, settings, manager, false);
                            collection.Add(subIns);
                        }
                    }

                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
