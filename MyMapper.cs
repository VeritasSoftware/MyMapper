/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////   MyMapper.cs    //////////////////////////////////////////////
///////////////////////////////////////// Author: Shantanu //////////////////////////////////////////////
///////////////////////////////////////// Date: 7-Oct-2015 //////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
// Modifications                                                                                       //
// Date                 Author      Reason                                                             //
/////////////////////////////////////////////////////////////////////////////////////////////////////////
// 28-Mar-2016          Shantanu    Added Switch sub-system                                            //
/////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MyMapper
{
    using MyMapper.Converters;

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

    #region Rules
    /// <summary>
    /// IMyMapperRules - Generic interface
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    public interface IMyMapperRules<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        IMyMapperRules<TSource, TDestination> With<TProperty>(
                                                            Func<TSource, TProperty> source,
                                                            Action<TDestination, TProperty> destination
                                                        );

        IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, TSourceResult>> source,
                                                        Action<TDestination, TDestinationResult> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, ICollection<TSourceResult>>> source,
                                                        Action<TDestination, ICollection<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IEnumerable<TSourceResult>>> source,
                                                        Action<TDestination, IEnumerable<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, List<TSourceResult>>> source,
                                                        Action<TDestination, List<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();        

        IMyMapperRules<TSource, TDestination> WithWhen<TProperty>(
                                                                Func<TSource, bool> when,
                                                                Func<TSource, TProperty> source,
                                                                Action<TDestination, TProperty> destination
                                                            );

        IMyMapperRules<TSource, TDestination> When(
                                                    Func<TSource, bool> when,
                                                    Action<IMyMapperRules<TSource, TDestination>> then
                                             );

        IMyMapperSwitch<TSource, TDestination, TProperty> Switch<TProperty>(Expression<Func<TSource, TProperty>> on);

        TDestination Exec();        
    }
    #endregion

    #region Switch
    /// <summary>
    /// IMyMapperSwitch - Generic interface
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    /// <typeparam name="TSourceProperty">The property</typeparam>
    public interface IMyMapperSwitch<TSource, TDestination, TSourceProperty>
        where TSource : class
        where TDestination : class, new()
    {
        IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> Case(Func<TSourceProperty, bool> when, Action<TDestination, TSourceProperty> then);        
    }

    /// <summary>
    /// IMyMapperSwitchElse - Generic interface
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    /// <typeparam name="TSourceProperty">The property</typeparam>
    public interface IMyMapperSwitchElse<TSource, TDestination, TSourceProperty>
        where TSource : class
        where TDestination : class, new()
    {
        IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> Case(Func<TSourceProperty, bool> when, Action<TDestination, TSourceProperty> then);

        IMyMapperSwitchEnd<TSource, TDestination> Else(Action<TDestination, TSourceProperty> then);

        IMyMapper<TSource, TDestination> End();
    }

    /// <summary>
    /// IMyMapperSwitchEnd - Generic interface
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    public interface IMyMapperSwitchEnd<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        IMyMapper<TSource, TDestination> End();
    }

    /// <summary>
    /// MyMapperSwitch - Generic class
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    /// <typeparam name="TSourceProperty">The property</typeparam>
    public class MyMapperSwitch<TSource, TDestination, TSourceProperty> : IMyMapperSwitch<TSource, TDestination, TSourceProperty>, 
                                                                            IMyMapperSwitchElse<TSource, TDestination, TSourceProperty>, 
                                                                            IMyMapperSwitchEnd<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        TSourceProperty Property { get; set; }
        TSource Source { get; set; }
        IMyMapper<TSource, TDestination> Mapper { get; set; }
        Action<TDestination, TSourceProperty> ElseThen { get; set; }
        Dictionary<Func<TSourceProperty, bool>, Action<TDestination, TSourceProperty>> cases = new Dictionary<Func<TSourceProperty, bool>, Action<TDestination, TSourceProperty>>();

        public MyMapperSwitch(TSource source, TSourceProperty property, IMyMapper<TSource, TDestination> mapper)
        {
            this.Source = source;
            this.Property = property;
            this.Mapper = mapper;
        }

        public IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> Case(Func<TSourceProperty, bool> when, Action<TDestination, TSourceProperty> then)
        {
            cases.Add(when, then);

            return this;
        }               

        public IMyMapperSwitchEnd<TSource, TDestination> Else(Action<TDestination, TSourceProperty> then)
        {
            ElseThen = then;

            return this;
        }

        public IMyMapper<TSource, TDestination> End()
        {
            foreach (var when in cases.Keys)
            {
                if (when != null && when(this.Property))
                {
                    cases[when](this.Mapper.Exec(), this.Property);

                    return this.Mapper;
                }                
            }

            if (ElseThen != null)
            {
                ElseThen(this.Mapper.Exec(), this.Property);
            }

            return this.Mapper;
        }
    }
    #endregion

    #region MyMapper
    /// <summary>
    /// IMyMapper - Generic interface
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    public interface IMyMapper<TSource, TDestination> : IMyMapperRules<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        IMyMapperRules<TSource, TDestination> Map(TSource source, bool automap = true);        

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

        public IMyMapperRules<TSource, TDestination> Map(TSource source, bool automap = true)
        {
            this.Source = source;

            this.Destination = new TDestination();

            if (automap)
                this.Destination = new EntityConverter<TSource, TDestination>().Convert(this.Source);

            return this;
        }

        public IMyMapperRules<TSource, TDestination> With<TProperty>(
                                                                        Func<TSource, TProperty> source,
                                                                        Action<TDestination, TProperty> destination
                                                                )
        {
            var sourceProp = source(this.Source);

            destination(this.Destination, sourceProp);

            return this;
        }

        public IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, TSourceResult>> source,
                                                        Action<TDestination, TDestinationResult> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TDestinationResult : class, new()
        {
            destination(this.Destination, map(source.Compile()(this.Source)));

            return this;
        }

        public IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, ICollection<TSourceResult>>> source,
                                                        Action<TDestination, ICollection<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            var destinationList = sourceList.Select(map);

            destination(this.Destination, destinationList as ICollection<TDestinationResult>);

            return this;
        }

        public IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IEnumerable<TSourceResult>>> source,
                                                        Action<TDestination, IEnumerable<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            var destinationList = sourceList.Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }

        public IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, List<TSourceResult>>> source,
                                                        Action<TDestination, List<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            var destinationList = sourceList.Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }        

        public IMyMapperRules<TSource, TDestination> WithWhen<TProperty>(
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

            destination(this.Destination, sourceProp);

            return this;
        }

        public IMyMapperRules<TSource, TDestination> When(
                                                        Func<TSource, bool> when,
                                                        Action<IMyMapperRules<TSource, TDestination>> then
                                                    )
        {
            if (when(this.Source))
            {
                then(this); 
            }

            return this;
        }

        public IMyMapperSwitch<TSource, TDestination, TProperty> Switch<TProperty>(Expression<Func<TSource, TProperty>> on)
        {
            IMyMapperSwitch<TSource, TDestination, TProperty> sw = new MyMapperSwitch<TSource, TDestination, TProperty>(
                                                                                           this.Source, 
                                                                                           on.Compile()(this.Source), 
                                                                                           this
                                                                                       );

            return sw;
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
#endregion
}
