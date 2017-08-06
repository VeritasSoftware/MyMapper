using MyMapper;
using System.Linq;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Static class MyMapperObjectExtensions
    /// </summary>
    public static class MyMapperObjectExtensions
    {
        /// <summary>
        /// MyMapper - Map extension method
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="obj">The source object</param>
        /// <returns>The destination object <see cref="TDestination"/></returns>
        public static TDestination Map<TSource, TDestination>(this TSource obj)
            where TSource : class
            where TDestination : class, new()
        {
            if (obj == null)
                return null;

            IMyMapper<TSource, TDestination> mapper =
                                    new MyMapper<TSource, TDestination>();

            return mapper.Map(obj, true).Exec();
        }

        /// <summary>
        /// MyMapper - MapAsync extension method
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="obj">The source object</param>
        /// <returns>The destination object <see cref="TDestination"/></returns>
        public static async Task<TDestination> MapAsync<TSource, TDestination>(this TSource obj)
            where TSource : class
            where TDestination : class, new()
        {
            if (obj == null)
                return null;

            IMyMapper<TSource, TDestination> mapper =
                                    new MyMapper<TSource, TDestination>();

            return await Task.Run(() => mapper.Map(obj, true).Exec());            
        }

        /// <summary>
        /// MyMapper - Map extension method
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="obj">The source object</param>
        /// <param name="map">MyMapper rules for the mapping</param>
        /// <param name="automap">Flag to indicate if to use automapping</param>
        /// <returns>The destination object <see cref="TDestination"/></returns>
        public static TDestination Map<TSource, TDestination>(this TSource obj,
                    Func<IMyMapperRules<TSource, TDestination>, TDestination> map = null,
                    bool automap = true
            )
            where TSource : class
            where TDestination : class, new()
        {
            if (obj == null)
                return null;

            IMyMapper<TSource, TDestination> mapper =
                                    new MyMapper<TSource, TDestination>();

            mapper.Map(obj, automap);

            if (map != null)
                return map(mapper);
            else
                return mapper.Exec();
        }

        /// <summary>
        /// MyMapper - MapAsync extension method
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TDestination">The destination type</typeparam>
        /// <param name="obj">The source object</param>
        /// <param name="map">MyMapper rules for the mapping</param>
        /// <param name="automap">Flag to indicate if to use automapping</param>
        /// <returns>The Task of the destination object <see cref="Task{TDestination}"/></returns>
        public static async Task<TDestination> MapAsync<TSource, TDestination>(this TSource obj,
                    Func<IMyMapperRules<TSource, TDestination>, TDestination> map = null,
                    bool automap = true
            )
            where TSource : class
            where TDestination : class, new()
        {
            if (obj == null)
                return null;

            IMyMapper<TSource, TDestination> mapper =
                                    new MyMapper<TSource, TDestination>();

            return await Task.Run(() =>
            {
                mapper.Map(obj, automap);

                if (map != null)
                    return map(mapper);
                else
                    return mapper.Exec();
            });            
        }
    }
}

namespace System.Collections.Generic
{
    /// <summary>
    /// Static class MyMapperEnumerableExtensions
    /// </summary>
    public static class MyMapperEnumerableExtensions
    {
        /// <summary>
        /// MyMapper - Map extension method
        /// </summary>
        /// <typeparam name="TSourceList">The source list type</typeparam>
        /// <typeparam name="TDestinationList">The destination list type</typeparam>
        /// <param name="source">The source list</param>
        /// <param name="map">MyMapper rules for the mapping</param>
        /// <param name="automap">Flag to indicate if to use automapping</param>
        /// <returns>The destination list <see cref="IEnumerable{TDestinationList}"/></returns>
        public static IEnumerable<TDestinationList> Map<TSourceList, TDestinationList>(
                    this IEnumerable<TSourceList> source,
                    Func<IMyMapperRules<TSourceList, TDestinationList>, TDestinationList> map = null,
                    bool automap = true
            )
            where TSourceList: class
            where TDestinationList: class, new()
        {
            if (source == null)
                return null;

            IMyMapper<TSourceList, TDestinationList> mapper = 
                                    new MyMapper<TSourceList, TDestinationList>();

            return source.Select(src =>
            {
                if (src == null)
                    return null;

                mapper.Map(src, automap);

                if (map != null)
                    return map(mapper);
                else
                    return mapper.Exec();
            });
        }

        /// <summary>
        /// MyMapper - MapAsync extension method
        /// </summary>
        /// <typeparam name="TSourceList">The source list type</typeparam>
        /// <typeparam name="TDestinationList">The destination list type</typeparam>
        /// <param name="source">The source list</param>
        /// <param name="map">MyMapper rules for the mapping</param>
        /// <param name="automap">Flag to indicate if to use automapping</param>
        /// <returns>The destination list <see cref="Task{IEnumerable{TDestinationList}}"/></returns>
        public static async Task<IEnumerable<TDestinationList>> MapAsync<TSourceList, TDestinationList>(
                    this IEnumerable<TSourceList> source,
                    Func<IMyMapperRules<TSourceList, TDestinationList>, TDestinationList> map = null,
                    bool automap = true
            )
            where TSourceList : class
            where TDestinationList : class, new()
        {
            if (source == null)
                return null;

            IMyMapper<TSourceList, TDestinationList> mapper =
                                    new MyMapper<TSourceList, TDestinationList>();

            return await Task.Run(() =>
            {
                return source.Select(src =>
                {
                    if (src == null)
                        return null;

                    mapper.Map(src, automap);

                    if (map != null)
                        return map(mapper);
                    else
                        return mapper.Exec();
                });
            });            
        }


        /// <summary>
        /// MyMapper - Map Parallel extension method - Uses PLINQ
        /// </summary>
        /// <typeparam name="TSourceList">The source list type</typeparam>
        /// <typeparam name="TDestinationList">The destination list type</typeparam>
        /// <param name="source">The source list</param>
        /// <param name="map">MyMapper rules for the mapping</param>
        /// <param name="automap">Flag to indicate if to use automapping</param>
        /// <returns>The destination list <see cref="IEnumerable{TDestinationList}"/></returns>
        public static IEnumerable<TDestinationList> MapParallel<TSourceList, TDestinationList>(
                    this IEnumerable<TSourceList> source,
                    Func<IMyMapperRules<TSourceList, TDestinationList>, TDestinationList> map = null,
                    bool automap = true
            )
            where TSourceList : class
            where TDestinationList : class, new()
        {
            if (source == null)
                return null;

            IMyMapper<TSourceList, TDestinationList> mapper =
                                    new MyMapper<TSourceList, TDestinationList>();

            return source.AsParallel().Select(src =>
            {
                if (src == null)
                    return null;

                mapper.Map(src, automap);

                if (map != null)
                    return map(mapper);
                else
                    return mapper.Exec();
            });
        }
    }
}
