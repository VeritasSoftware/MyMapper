/////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// EntityConverter.cs ////////////////////////////////////////////
///////////////////////////////////////// Author: Shantanu   ////////////////////////////////////////////
///////////////////////////////////////// Date: 7-Oct-2015   ////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
/// History   ///////////////////////////////////////////////////////////////////////////////////////////
/// 13-Jun-2016 Shantanu    Added SqlDataReaderToListConverter converter ////////////////////////////////
/// /////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using System.Data;
using System.Data.SqlClient;
using System.Reflection;

using System.Collections;
using System.Collections.Concurrent;

namespace MyMapper.Converters
{
    /// <summary>
    /// SqlDataReader to List converter
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    public class SqlDataReaderToListConverter<TEntity> : ITypeConverter<SqlDataReader, IList<TEntity>>
        where TEntity : class, new()
    {
        static ConcurrentDictionary<Type, List<PropertyInfo>> dictionaryEntityPropertyInfos;

        public IList<TEntity> Convert(SqlDataReader source)
        {
            List<PropertyInfo> entityPropertyInfos;

            List<TEntity> list = new List<TEntity>();

            if (dictionaryEntityPropertyInfos == null)
                dictionaryEntityPropertyInfos = new ConcurrentDictionary<Type, List<PropertyInfo>>();

            if (!dictionaryEntityPropertyInfos.ContainsKey(typeof(TEntity)))
            {
                entityPropertyInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.GetOrAdd(typeof(TEntity), entityPropertyInfos);
            }
            else
            {
                dictionaryEntityPropertyInfos.TryGetValue(typeof(TEntity), out entityPropertyInfos);
            }

            if (source != null)
            {                
                while (source.Read())
                {
                    TEntity entity = new TEntity();

                    for (int i = 0; i < source.FieldCount; i++)
                    {
                        var columnName = source.GetName(i);                        
                        PropertyInfo propertyInfo = null;

                        try
                        {
                            propertyInfo = entityPropertyInfos.SingleOrDefault(pi => pi.Name == columnName);
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        if (propertyInfo != null && propertyInfo.CanWrite)
                        {
                            try
                            {
                                object value = System.Convert.ChangeType(source[columnName], Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);

                                propertyInfo.SetValue(entity, value, null);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    list.Add(entity);
                }                
            }

            return list;
        }
    }

    /// <summary>
    /// DataRowToEntityConverter : Converts a datarow to an entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    public class DataRowToEntityConverter<TEntity> : ITypeConverter<DataRow, TEntity>
        where TEntity : class, new()
    {
        static ConcurrentDictionary<Type, List<PropertyInfo>> dictionaryEntityPropertyInfos;

        public TEntity Convert(DataRow source)
        {
            List<PropertyInfo> entityPropertyInfos;

            TEntity obj = new TEntity();

            if (dictionaryEntityPropertyInfos == null)
                dictionaryEntityPropertyInfos = new ConcurrentDictionary<Type, List<PropertyInfo>>();

            if (!dictionaryEntityPropertyInfos.ContainsKey(typeof(TEntity)))
            {
                entityPropertyInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.GetOrAdd(typeof(TEntity), entityPropertyInfos);
            }
            else
            {
                dictionaryEntityPropertyInfos.TryGetValue(typeof(TEntity), out entityPropertyInfos);
            }

            foreach (PropertyInfo propertyInfo in entityPropertyInfos)
            {
                object rowVal = null;

                try
                {
                    rowVal = source[propertyInfo.Name];
                }
                catch (Exception)
                {
                    continue;
                }

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    try
                    {
                        object value = System.Convert.ChangeType(rowVal, propertyInfo.PropertyType);
                        propertyInfo.SetValue(obj, value, null);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return obj;
        }
    }

    /// <summary>
    /// EntityConverter : Converts an entity to another entity
    /// </summary>
    /// <typeparam name="TSource">The source entity</typeparam>
    /// <typeparam name="TDestination">The destination entity</typeparam>
    public class EntityConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        static ConcurrentDictionary<Type, List<PropertyInfo>> dictionaryEntityPropertyInfos;

        protected object Convert(object source, Type destinationType)
        {
            try
            {
                List<PropertyInfo> sourcePropertyInfos;
                List<PropertyInfo> destinationPropertyInfos;

                //object destinationObj = Activator.CreateInstance(destinationType);
                object destinationObj = destinationType.GetInstance();

                if (dictionaryEntityPropertyInfos == null)
                    dictionaryEntityPropertyInfos = new ConcurrentDictionary<Type, List<PropertyInfo>>();

                if (!dictionaryEntityPropertyInfos.ContainsKey(source.GetType()))
                {
                    sourcePropertyInfos = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                    dictionaryEntityPropertyInfos.GetOrAdd(source.GetType(), sourcePropertyInfos);
                }
                else
                {
                    dictionaryEntityPropertyInfos.TryGetValue(source.GetType(), out sourcePropertyInfos);
                }

                if (!dictionaryEntityPropertyInfos.ContainsKey(destinationObj.GetType()))
                {
                    destinationPropertyInfos = destinationObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                    dictionaryEntityPropertyInfos.GetOrAdd(destinationObj.GetType(), destinationPropertyInfos);
                }
                else
                {
                    dictionaryEntityPropertyInfos.TryGetValue(destinationObj.GetType(), out destinationPropertyInfos);
                }

                foreach (PropertyInfo destinationPropertyInfo in destinationPropertyInfos)
                {
                    object sourceVal = null;

                    PropertyInfo sourcePropertyInfo = null;

                    try
                    {
                        sourcePropertyInfo = sourcePropertyInfos.Single(pi => pi.Name == destinationPropertyInfo.Name); //&& pi.PropertyType == destinationPropertyInfo.PropertyType);

                        sourceVal = sourcePropertyInfo.GetValue(source, BindingFlags.GetProperty, null, null, null);

                        if (typeof(IList).IsAssignableFrom(sourcePropertyInfo.PropertyType)
                        && sourcePropertyInfo.PropertyType.IsGenericType)
                        {
                            //var list = Activator.CreateInstance(destinationPropertyInfo.PropertyType);
                            var list = TypeHelpers.GetInstanceFromType(destinationPropertyInfo.PropertyType);

                            var sList = sourceVal as IList;
                            var dList = list as IList;

                            CreateList(sList, dList, dList.GetType().GetGenericArguments()[0]);

                            sourceVal = dList;
                        }
                        else if (sourcePropertyInfo.PropertyType.IsArray)
                        {
                            var sList = sourceVal as IList;
                            var list = Activator.CreateInstance(destinationPropertyInfo.PropertyType, sList.Count);
                            var dList = list as IList;

                            CreateArray(sList, dList, destinationPropertyInfo.PropertyType);

                            sourceVal = dList;
                        }
                        else if (sourcePropertyInfo.PropertyType.IsGenericType &&
                            sourceVal is IDictionary)
                        {
                            //var dictionary = Activator.CreateInstance(destinationPropertyInfo.PropertyType);
                            //var dictionary = TypeHelpers.GetInstanceFromType(destinationPropertyInfo.PropertyType);
                            var destType = sourceVal.GetType();
                            var dictionary = TypeHelpers.GetInstanceFromType(destType);

                            var sDictionary = sourceVal as IDictionary;
                            var dDictionary = dictionary as IDictionary;


                            var keyType = destinationPropertyInfo.PropertyType.GetGenericArguments()[0];
                            var valueType = destinationPropertyInfo.PropertyType.GetGenericArguments()[1];

                            CreateDictionary(sDictionary, dDictionary, keyType, valueType);

                            sourceVal = dDictionary;
                        }
                        else if (typeof(IEnumerable).IsAssignableFrom(sourcePropertyInfo.PropertyType)
                        && sourcePropertyInfo.PropertyType.IsGenericType)
                        {
                            var listType = typeof(List<>);
                            var constructedListType = listType.MakeGenericType(destinationPropertyInfo.PropertyType.GetGenericArguments()[0]);

                            //var list = Activator.CreateInstance(constructedListType);
                            var list = TypeHelpers.GetInstanceFromType(constructedListType);

                            var sList = sourceVal as IList;
                            var dList = list as IList;

                            CreateList(sList, dList, destinationPropertyInfo.PropertyType.GetGenericArguments()[0]);

                            sourceVal = dList;
                        }
                        else if (sourcePropertyInfo.PropertyType.IsClass && !sourcePropertyInfo.PropertyType.FullName.StartsWith("System."))
                        {
                            sourceVal = this.Convert(sourceVal, destinationPropertyInfo.PropertyType);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (destinationPropertyInfo != null && destinationPropertyInfo.CanWrite)
                    {
                        try
                        {
                            object value = (sourceVal is IConvertible) && Nullable.GetUnderlyingType(sourcePropertyInfo.PropertyType) == null ?
                                        System.Convert.ChangeType(sourceVal, destinationPropertyInfo.PropertyType)
                                        : sourceVal;

                            destinationPropertyInfo.SetValue(destinationObj, value, null);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                return destinationObj;
            }
            catch (Exception)
            {
                return null;
            }
        }        

        public TDestination Convert(TSource source)
        {
            return this.Convert(source, typeof(TDestination)) as TDestination;            
        }

        private void CreateList(IList sList, IList dList, Type destinationType)
        {
            bool isClass = destinationType.IsClass && !destinationType.FullName.StartsWith("System.");            

            foreach (var item in sList)
            {
                if (isClass)
                    dList.Add(this.Convert(item, destinationType));
                else
                    dList.Add(item);                
            }
        }

        private void CreateArray(IList sList, IList dList, Type destinationType)
        {
            bool isClass = destinationType.IsClass && !destinationType.FullName.StartsWith("System.");

            int i = 0;
            foreach (var item in sList)
            {
                if (isClass)
                    dList[i] = this.Convert(item, destinationType);
                else
                    dList[i] = item;

                i++;
            }
        }

        private void CreateDictionary(IDictionary sList, IDictionary dList, Type destinationTypeKey, Type destinationTypeValue)
        {            
            foreach (var item in sList)
            {
                var entry = (DictionaryEntry)item;

                var key = System.Convert.ChangeType(entry.Key, destinationTypeKey);
                var value = System.Convert.ChangeType(entry.Value, destinationTypeValue);

                dList.Add(key, value);
            }
        }

    }
}
