/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////   MyMapper.cs    //////////////////////////////////////////////
///////////////////////////////////////// Author: Shantanu //////////////////////////////////////////////
///////////////////////////////////////// Date: 7-Oct-2015 //////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MyMapper
{
    public interface IMap<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        TDestination Map(TSource source, IMyMapper<TSource, TDestination> mapper);        
    }

    public interface ITypeConverter<TSource, TDestination>
    {
        TDestination Convert(TSource source); 
    }

    /// <summary>
    /// MyMapper - Generic interface
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    public interface IMyMapper<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        IMyMapper<TSource, TDestination> Map(TSource source);
        
        IMyMapper<TSource, TDestination> With<TProperty>(
                                                            Func<TSource, TProperty> source,
                                                            Action<TDestination, TProperty> destination
                                                        );

        IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, TSourceResult>> mapFrom,
                                                        Action<TDestination, TDestinationResult> mapTo,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TDestinationResult : class, new();

        IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, ICollection<TSourceResult>>> source,
                                                        Action<TDestination, ICollection<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IEnumerable<TSourceResult>>> source,
                                                        Action<TDestination, IEnumerable<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, List<TSourceResult>>> source,
                                                        Action<TDestination, List<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapper<TSource, TDestination> WithWhen<TProperty>(
                                                                Func<TSource, bool> when,
                                                                Func<TSource, TProperty> source,
                                                                Action<TDestination, TProperty> destination
                                                            );

        IMyMapper<TSource, TDestination> When(
                                                    Func<TSource, bool> when,
                                                    Action<IMyMapper<TSource, TDestination>> then
                                             );

        TDestination Exec();

        TDestination Exec(TSource source, Func<TSource, IMyMapper<TSource, TDestination>, TDestination> map);

        TDestination Exec<TConverter>(TSource source)
            where TConverter : ITypeConverter<TSource, TDestination>, new();
    }

    /// <summary>
    /// MyMapper - Generic class
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    public class MyMapper<TSource, TDestination> : IMyMapper<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {        
        TSource Source { get; set; }
        TDestination Destination { get; set; }

        public IMyMapper<TSource, TDestination> Map(TSource source)
        {
            this.Source = source;

            return this;
        }        

        public IMyMapper<TSource, TDestination> With<TProperty>(
                                                                        Func<TSource, TProperty> source,
                                                                        Action<TDestination, TProperty> destination
                                                                )
        {
            var sourceProp = source(this.Source);

            if (this.Destination == null)
                this.Destination = new TDestination();

            destination(this.Destination, sourceProp);

            return this;
        }

        public IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, TSourceResult>> source,
                                                        Action<TDestination, TDestinationResult> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TDestinationResult : class, new()
        {
            if (this.Destination == null)
                this.Destination = new TDestination();

            destination(this.Destination, map(source.Compile()(this.Source)));

            return this;
        }

        public IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, ICollection<TSourceResult>>> source,
                                                        Action<TDestination, ICollection<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            if (this.Destination == null)
                this.Destination = new TDestination();

            var destinationList = sourceList.Select(map);            

            destination(this.Destination, destinationList as ICollection<TDestinationResult>);

            return this;
        }        

        public IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IEnumerable<TSourceResult>>> source,
                                                        Action<TDestination, IEnumerable<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            if (this.Destination == null)
                this.Destination = new TDestination();

            var destinationList = sourceList.Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }

        public IMyMapper<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, List<TSourceResult>>> source,
                                                        Action<TDestination, List<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            if (this.Destination == null)
                this.Destination = new TDestination();

            var destinationList = sourceList.Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }            

        public IMyMapper<TSource, TDestination> WithWhen<TProperty>(
                                                                        Func<TSource, bool> when,
                                                                        Func<TSource, TProperty> source,
                                                                        Action<TDestination, TProperty> destination
                                                                    )
        {
            if (!when(this.Source))
            {
                return this;
            }

            var sourceProp = source(this.Source);

            if (this.Destination == null)
                this.Destination = new TDestination();

            destination(this.Destination, sourceProp);

            return this;
        }

        public IMyMapper<TSource, TDestination> When(
                                                        Func<TSource, bool> when,
                                                        Action<IMyMapper<TSource, TDestination>> then
                                                    )
        {
            if (when(this.Source))
            {
                then(this);
            }

            return this;
        }        

        public TDestination Exec()
        {
            return this.Destination;
        }

        public TDestination Exec(TSource source, Func<TSource, IMyMapper<TSource, TDestination>, TDestination> map)
        {
            return map(source, this);
        }        

        public TDestination Exec<TConverter>(TSource source)
            where TConverter : ITypeConverter<TSource, TDestination>, new()
        {
            return new TConverter().Convert(source);
        }
    }
}