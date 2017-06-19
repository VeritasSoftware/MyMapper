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
using System.Collections.Concurrent;
using MyMapper.Extensions;

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
    /// <exception cref="ArgumentNullException"></exception>
    public class EntityConverter<TSource, TDestination> : ITypeConverter<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {        
        public TDestination Convert(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.AsDictionary(typeof(TSource)).ToObject<TDestination>();            
        }        
    }    
}
