/////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////// EntityConverter.cs ////////////////////////////////////////////
///////////////////////////////////////// Author: Shantanu   ////////////////////////////////////////////
///////////////////////////////////////// Date: 7-Oct-2015   ////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Reflection;

using System.Collections;

namespace MyMapper.Converters
{
    /// <summary>
    /// DataRowToEntityConverter : Converts a datarow to an entity
    /// </summary>
    /// <typeparam name="TEntity">The entity</typeparam>
    public class DataRowToEntityConverter<TEntity> : ITypeConverter<DataRow, TEntity>
        where TEntity : class, new()
    {
        static Dictionary<Type, List<PropertyInfo>> dictionaryEntityPropertyInfos;

        public TEntity Convert(DataRow source)
        {
            List<PropertyInfo> entityPropertyInfos;

            TEntity obj = new TEntity();

            if (dictionaryEntityPropertyInfos == null)
                dictionaryEntityPropertyInfos = new Dictionary<Type, List<PropertyInfo>>();

            if (!dictionaryEntityPropertyInfos.ContainsKey(typeof(TEntity)))
            {
                entityPropertyInfos = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.Add(typeof(TEntity), entityPropertyInfos);
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
        static Dictionary<Type, List<PropertyInfo>> dictionaryEntityPropertyInfos;

        protected object Convert(object source, Type destinationType)
        {
            List<PropertyInfo> sourcePropertyInfos;
            List<PropertyInfo> destinationPropertyInfos;

            object destinationObj = Activator.CreateInstance(destinationType);

            if (dictionaryEntityPropertyInfos == null)
                dictionaryEntityPropertyInfos = new Dictionary<Type, List<PropertyInfo>>();

            if (!dictionaryEntityPropertyInfos.ContainsKey(source.GetType()))
            {
                sourcePropertyInfos = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.Add(source.GetType(), sourcePropertyInfos);
            }
            else
            {
                dictionaryEntityPropertyInfos.TryGetValue(source.GetType(), out sourcePropertyInfos);
            }            

            if (!dictionaryEntityPropertyInfos.ContainsKey(destinationObj.GetType()))
            {
                destinationPropertyInfos = destinationObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.Add(destinationObj.GetType(), destinationPropertyInfos);
            }
            else
            {
                dictionaryEntityPropertyInfos.TryGetValue(destinationObj.GetType(), out destinationPropertyInfos);
            }

            foreach (PropertyInfo destinationPropertyInfo in destinationPropertyInfos)
            {
                object sourceVal = null;

                try
                {
                    var sourcePropertyInfo = sourcePropertyInfos.Single(pi => pi.Name == destinationPropertyInfo.Name); //&& pi.PropertyType == destinationPropertyInfo.PropertyType);

                    sourceVal = sourcePropertyInfo.GetValue(source, BindingFlags.GetProperty, null, null, null);

                    if (sourcePropertyInfo.PropertyType.IsClass && !sourcePropertyInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        sourceVal = this.Convert(sourceVal, destinationPropertyInfo.PropertyType);
                    }
                    else if (typeof(IList).IsAssignableFrom(sourcePropertyInfo.PropertyType)
                    && sourcePropertyInfo.PropertyType.IsGenericType)
                    {
                        var list = Activator.CreateInstance(destinationPropertyInfo.PropertyType);

                        var sList = sourceVal as IList;
                        var dList = list as IList;

                        foreach (var item in sList)
                        {
                            dList.Add(this.Convert(item, dList.GetType().GetGenericArguments()[0]));
                        }

                        sourceVal = dList;
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
                        object value = System.Convert.ChangeType(sourceVal, destinationPropertyInfo.PropertyType);

                        destinationPropertyInfo.SetValue(destinationObj, value, null);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return destinationObj;
        }

        public TDestination Convert(TSource source)
        {           
            TDestination destinationObj = new TDestination();

            List<PropertyInfo> sourcePropertyInfos;
            List<PropertyInfo> destinationPropertyInfos;

            if (dictionaryEntityPropertyInfos == null)
                dictionaryEntityPropertyInfos = new Dictionary<Type, List<PropertyInfo>>();

            if (!dictionaryEntityPropertyInfos.ContainsKey(source.GetType()))
            {
                sourcePropertyInfos = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.Add(source.GetType(), sourcePropertyInfos);
            }
            else
            {
                dictionaryEntityPropertyInfos.TryGetValue(source.GetType(), out sourcePropertyInfos);
            }            

            if (!dictionaryEntityPropertyInfos.ContainsKey(destinationObj.GetType()))
            {
                destinationPropertyInfos = destinationObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

                dictionaryEntityPropertyInfos.Add(destinationObj.GetType(), destinationPropertyInfos);
            }
            else
            {
                dictionaryEntityPropertyInfos.TryGetValue(destinationObj.GetType(), out destinationPropertyInfos);
            }

            foreach (PropertyInfo destinationPropertyInfo in destinationPropertyInfos)
            {
                object sourceVal = null;

                try
                {
                    var sourcePropertyInfo = sourcePropertyInfos.Single(pi => pi.Name == destinationPropertyInfo.Name); //&& pi.PropertyType == destinationPropertyInfo.PropertyType);

                    sourceVal = sourcePropertyInfo.GetValue(source, BindingFlags.GetProperty, null, null, null);

                    if (sourcePropertyInfo.PropertyType.IsClass && !sourcePropertyInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        sourceVal = Convert(sourceVal, destinationPropertyInfo.PropertyType);
                    }
                    else if (typeof(IList).IsAssignableFrom(sourcePropertyInfo.PropertyType)
                    && sourcePropertyInfo.PropertyType.IsGenericType)
                    {
                        var list = Activator.CreateInstance(destinationPropertyInfo.PropertyType);

                        var sList = sourceVal as IList;
                        var dList = list as IList;

                        foreach (var item in sList)
                        {
                            dList.Add(this.Convert(item, dList.GetType().GetGenericArguments()[0]));
                        }

                        sourceVal = dList;
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
                        object value = System.Convert.ChangeType(sourceVal, destinationPropertyInfo.PropertyType);

                        destinationPropertyInfo.SetValue(destinationObj, value, null);
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return destinationObj;
        }
    }
}
