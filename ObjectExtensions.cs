using MyMapper.TypeHelpers;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MyMapper.Extensions
{
    internal static class ObjectExtensions
    {
        static ConcurrentDictionary<Type, List<PropertyInfo>> _dictionaryEntityPropertyInfos
                                            = new ConcurrentDictionary<Type, List<PropertyInfo>>();

        static ConcurrentDictionary<Type, Type> _genericTypes 
                                            = new ConcurrentDictionary<Type, Type>();

        static ConcurrentDictionary<Type, Type[]> _genericArgumentTypes 
                                            = new ConcurrentDictionary<Type, Type[]>();

        static Type _genericListType = typeof(List<>);
        static Type _genericDictionaryType = typeof(Dictionary<,>);

        public static object ToObject(this IDictionary<string, TypeValue> source, Type destinationType)
        {
            if (source == null)
            {
                return null;
            }
           
            var someObject = destinationType.IsClass && !destinationType.FullName.StartsWith("System.") ? destinationType.GetInstance() : Activator.CreateInstance(destinationType);

            var propetyInfos = _dictionaryEntityPropertyInfos.GetOrAdd(destinationType, destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList());

            foreach (KeyValuePair<string, TypeValue> item in source)
            {
                var destProp = propetyInfos.Single(pi => pi.Name == item.Key);

                if (destProp.PropertyType.IsClass && !destProp.PropertyType.FullName.StartsWith("System."))
                {
                    var obj = ToObject(item.Value.Value as IDictionary<string, TypeValue>, propetyInfos.Single(pi => pi.Name == item.Key).PropertyType);

                    destProp.SetValue(someObject, obj, null);

                    continue;
                }

                destProp.SetValue(someObject, item.Value.Value, null);
            }            

            return someObject;
        }

        public static object ToListObject(this List<IDictionary<string, TypeValue>> source, Type listType, Type listObjectType)
        {
            if (source == null)
            {
                return null;
            }

            var someObject = listType.GetInstance();

            var dList = someObject as IList;

            var orderedList = new List<OrderedItem>();

            var lockObj = new object();

            Parallel.For(0, source.Count, index =>
            {
                var sourceItem = source[index];

                var listObj = ToObject(sourceItem, listObjectType);                

                lock(lockObj)
                {
                    orderedList.Add(new OrderedItem { Order = index, Value = listObj });
                }
            });

            orderedList.OrderBy(oi => oi.Order).Select(oi => oi.Value).ToList().ForEach(o => dList.Add(o));

            return dList;            
        }

        public static object ToDictionaryObject(this List<OrderedDictionaryEntryItem> source, Type destinationDictionaryType, Type dictionaryKeyType, Type dictionaryValueType)
        {
            if (source == null)
            {
                return null;
            }

            var someObject = destinationDictionaryType.GetInstance();

            var dDictionary = someObject as IDictionary;

            var orderedList = new List<OrderedDictionaryItem>();

            var lockObj = new object();

            Parallel.For(0, source.Count, index =>
            {
                var keyItem = source[index].Key;
                var valueItem = source[index].Value;

                object keyObj = null;

                if (keyItem is Dictionary<string, TypeValue>)
                {
                    keyObj = (keyItem as Dictionary<string, TypeValue>).ToObject(dictionaryKeyType);
                }
                else
                {
                    keyObj = keyItem;
                }

                object valueObj = null;

                if (valueItem is Dictionary<string, TypeValue>)
                {
                    valueObj = (valueItem as Dictionary<string, TypeValue>).ToObject(dictionaryValueType);
                }
                else
                {
                    valueObj = valueItem;
                }                

                lock (lockObj)
                {
                    orderedList.Add(new OrderedDictionaryItem { Order = index, Key = keyObj, Value = valueObj });
                }
            });

            orderedList.OrderBy(oi => oi.Order).ToList().ForEach(oi => dDictionary.Add(oi.Key, oi.Value));

            return dDictionary;
        }

        public static T ToObject<T>(this IDictionary<string, TypeValue> source)
            where T : class, new()
        {
            if (source == null)
            {
                return default(T);
            }

            T someObject = new T();
            Type someObjectType = typeof(T);

            var destPropetyInfos = _dictionaryEntityPropertyInfos.GetOrAdd(someObjectType, someObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList());

            Parallel.ForEach(source, item =>
            {
                try
                {
                    var destPropertyInfo = destPropetyInfos.Single(pi => pi.Name == item.Key);

                    var itemValueType = item.Value.Type;

                    if (item.Value.GetType() == typeof(TypeValue)
                        && (itemValueType.IsClass && !itemValueType.FullName.StartsWith("System."))
                        && !(item.Value.Value is IList && itemValueType.IsGenericType)
                        && !(item.Value.Value is IDictionary && itemValueType.IsGenericType)
                    )
                    {
                        var o = (item.Value.Value as IDictionary<string, TypeValue>).ToObject(destPropertyInfo.PropertyType);

                        destPropertyInfo.SetValue(someObject, o, null);

                        return;
                    }

                    //Property is class     
                    if ((itemValueType.IsClass && !itemValueType.FullName.StartsWith("System.")))
                    {
                        var o = item.Value.Value.AsDictionary(itemValueType).ToObject(destPropertyInfo.PropertyType);

                        destPropertyInfo.SetValue(someObject, o, null);

                        return;
                    }

                    //Property is List
                    if (item.Value.Value is IList && itemValueType.IsGenericType
                       )
                    {
                        var destP = destPropertyInfo.PropertyType;

                        Type[] typeArgs = _genericArgumentTypes.GetOrAdd(destP, destP.GetGenericArguments());

                        var sourceListType = _genericArgumentTypes.GetOrAdd(itemValueType, itemValueType.GetGenericArguments())[0];

                        IList dList = null;

                        if (sourceListType != typeArgs[0])
                        {
                            var dest = _genericTypes.GetOrAdd(destPropertyInfo.PropertyType, _genericListType.MakeGenericType(typeArgs));

                            var sourceList = item.Value.Value as IList;

                            dList = sourceList.AsDictionary(sourceListType).ToListObject(dest, typeArgs[0]) as IList;
                        }
                        else
                            dList = item.Value.Value as IList;

                        destPropertyInfo.SetValue(someObject, dList, null);

                        return;
                    }

                    //Property is Dictionary
                    if (item.Value.Value is IDictionary && itemValueType.IsGenericType
                       )
                    {
                        var destP = destPropertyInfo.PropertyType;

                        Type[] typeArgs = _genericArgumentTypes.GetOrAdd(destP, destP.GetGenericArguments());

                        IDictionary dList = null;

                        var sourceTypes = _genericArgumentTypes.GetOrAdd(itemValueType, itemValueType.GetGenericArguments());

                        var sourceKeyType = sourceTypes[0];
                        var sourceValueType = sourceTypes[1];

                        if (sourceKeyType != typeArgs[0] || sourceValueType != typeArgs[1])
                        {
                            var dest = _genericDictionaryType.MakeGenericType(typeArgs);

                            var sourceDictionary = item.Value.Value as IDictionary;

                            dList = sourceDictionary.AsDictionary(sourceKeyType, sourceValueType).ToDictionaryObject(dest, typeArgs[0], typeArgs[1]) as IDictionary;
                        }
                        else
                        {
                            dList = item.Value.Value as IDictionary;
                        }

                        destPropertyInfo.SetValue(someObject, dList, null);

                        return;
                    }                    

                    destPropertyInfo.SetValue(someObject, item.Value.Value, null);
                }
                catch (Exception)
                {
                    return;
                }
            });

            return someObject;
        }

        public static IEnumerable<DictionaryEntry> Entries(this IDictionary dict)
        {
            foreach (var item in dict) yield return (DictionaryEntry)item;
        }

        public static IDictionary<string, TypeValue> AsDictionary(this object source, Type sourceType = null, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            if (source == null)
            {
                return null;
            }

            if (sourceType == null)
            {
                sourceType = source.GetType();
            }

            var properties = _dictionaryEntityPropertyInfos.GetOrAdd(sourceType, sourceType.GetProperties(bindingAttr).ToList());

            return properties.ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo =>
                {
                    var item = propInfo.GetValue(source, BindingFlags.GetProperty, null, null, null);

                    if (propInfo.PropertyType.IsClass && !propInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        return new TypeValue { Value = item.AsDictionary(propInfo.PropertyType), Type = propInfo.PropertyType };
                    }
                    return new TypeValue { Value = propInfo.GetValue(source, BindingFlags.GetProperty, null, null, null), Type = propInfo.PropertyType };
                }
            );
        }

        public static List<IDictionary<string, TypeValue>> AsDictionary(this IList source, Type sourceType = null, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            if (source == null)
            {
                return null;
            }

            if (sourceType == null)
            {
                sourceType = source.GetType();
            }

            var properties = _dictionaryEntityPropertyInfos.GetOrAdd(sourceType, sourceType.GetProperties(bindingAttr).ToList());

            List<OrderedListItem> list = new List<OrderedListItem>();

            var lockObj = new object();

            Parallel.For(0, source.Count, index =>
            {
                var i = source[index];

                var dic = properties.ToDictionary
                (
                    propInfo => propInfo.Name,
                    propInfo => new TypeValue { Value = propInfo.GetValue(i, BindingFlags.GetProperty, null, null, null), Type = propInfo.PropertyType }
                );

                lock(lockObj)
                {
                    list.Add(new OrderedListItem { Order = index, Value = dic });
                }                
            });

            return list.OrderBy(od => od.Order).Select(od => od.Value).ToList();            
        }

        public static List<OrderedDictionaryEntryItem> AsDictionary(this IDictionary source, Type sourceKeyType, Type sourceValueType, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            if (source == null)
            {
                return null;
            }

            if (sourceKeyType == null)
            {
                sourceKeyType = source.GetType();
            }

            var propertiesKey = _dictionaryEntityPropertyInfos.GetOrAdd(sourceKeyType, sourceKeyType.GetProperties(bindingAttr).ToList());
            var propertiesValue = _dictionaryEntityPropertyInfos.GetOrAdd(sourceValueType, sourceValueType.GetProperties(bindingAttr).ToList());

            List<OrderedDictionaryEntryItem> list = new List<OrderedDictionaryEntryItem>();            

            var s = source.Entries().ToList();

            var lockObj = new object();

            Parallel.For(0, s.Count, index =>
            {                
                var i = s[index];

                DictionaryEntry de = (DictionaryEntry)i;

                object dicKey = null;

                if (!propertiesKey.Any())
                {
                    dicKey = System.Convert.ChangeType(de.Key, sourceKeyType); //as IDictionary<string, TypeValue> //de.Key.AsDictionary();
                }
                else
                {
                    dicKey = propertiesKey.ToDictionary
                   (
                       propInfo => propInfo.Name,
                       propInfo => new TypeValue { Value = propInfo.GetValue(i.Key, BindingFlags.GetProperty, null, null, null), Type = propInfo.PropertyType }
                   );
                }

                object dicValue = null;

                if (!propertiesValue.Any())
                {
                    dicValue = System.Convert.ChangeType(de.Value, sourceValueType);//de.Value.AsDictionary();
                }
                else
                {
                    dicValue = propertiesValue.ToDictionary
                        (
                            propInfo => propInfo.Name,
                            propInfo => new TypeValue { Value = propInfo.GetValue(i.Value, BindingFlags.GetProperty, null, null, null), Type = propInfo.PropertyType }
                        );
                }

                lock (lockObj)
                {
                    list.Add(new OrderedDictionaryEntryItem { Order = index, Key = dicKey, Value = dicValue });
                }
            });

            return list.OrderBy(od => od.Order).ToList();
        }

    }

    internal class TypeValue
    {
        public Type Type { get; set; }

        public object Value { get; set; }
    }

    internal class OrderedItem
    {
        public long Order { get; set; }

        public object Value { get; set; }
    }

    internal class OrderedDictionaryEntryItem
    {
        public long Order { get; set; }
        public object Key { get; set; }
        public object Value { get; set; }
    }

    internal class OrderedDictionaryItem
    {
        public long Order { get; set; }

        public object Key { get; set; }

        public object Value { get; set; }
    }

    internal class OrderedListItem
    {
        public long Order { get; set; }

        public IDictionary<string, TypeValue> Value { get; set; }
    }    
}
