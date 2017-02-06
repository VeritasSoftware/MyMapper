/////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////   MyMapper.cs    //////////////////////////////////////////////
///////////////////////////////////////// Author: Shantanu //////////////////////////////////////////////
///////////////////////////////////////// Date: 7-Oct-2015 //////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////
// Modifications                                                                                       //
// Date                 Author      Reason                                                             //
/////////////////////////////////////////////////////////////////////////////////////////////////////////
// 28-Mar-2016          Shantanu    Added Switch sub-system                                            //
// 06-Feb-2017          Shantanu    Added parallel mapping support                                     //
/////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using System.Data;

namespace MyMapper
{
    using MyMapper.Converters;

    [Obsolete("Interface is deprecated.", true)]
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
                                                            Expression<Func<TSource, TProperty>> source,
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
                                                        Expression<Func<TSource, IList<TSourceResult>>> source,
                                                        Action<TDestination, IList<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();        

        IMyMapperRules<TSource, TDestination> With<TDestinationResult>(
                                                        Expression<Func<TSource, DataTable>> source,
                                                        Action<TDestination, IList<TDestinationResult>> destination,
                                                        Func<DataRow, TDestinationResult> map
                                                    )
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> ParallelWith<TSourceResult, TDestinationResult>(
                                                Expression<Func<TSource, ICollection<TSourceResult>>> source,
                                                Action<TDestination, ICollection<TDestinationResult>> destination,
                                                Func<TSourceResult, TDestinationResult> map
                                            )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> ParallelWith<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IEnumerable<TSourceResult>>> source,
                                                        Action<TDestination, IEnumerable<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> ParallelWith<TSourceResult, TDestinationResult>(
                                                Expression<Func<TSource, IList<TSourceResult>>> source,
                                                Action<TDestination, IList<TDestinationResult>> destination,
                                                Func<TSourceResult, TDestinationResult> map
                                            )
            where TSourceResult : class
            where TDestinationResult : class, new();

        IMyMapperRules<TSource, TDestination> WithWhen<TProperty>(
                                                                Expression<Func<TSource, bool>> when,
                                                                Func<TSource, TProperty> source,
                                                                Action<TDestination, TProperty> destination
                                                            );

        IMyMapperRules<TSource, TDestination> When(
                                                    Expression<Func<TSource, bool>> when,
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
        IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> CaseMap(Expression<Func<TSourceProperty, bool>> when, Action<IMyMapperRules<TSource, TDestination>> then);

        IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> Case(Expression<Func<TSourceProperty, bool>> when, Action<TDestination, TSourceProperty> then);        
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
        IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> CaseMap(Expression<Func<TSourceProperty, bool>> when, Action<IMyMapperRules<TSource, TDestination>> then);

        IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> Case(Expression<Func<TSourceProperty, bool>> when, Action<TDestination, TSourceProperty> then);

        IMyMapperSwitchEnd<TSource, TDestination> ElseMap(Action<IMyMapperRules<TSource, TDestination>> then);

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

    internal class SwitchThen<TSource, TDestination, TSourceProperty>
        where TSource : class
        where TDestination : class, new()
    {
        public Action<TDestination, TSourceProperty> Case { get; set; }
        public Action<IMyMapperRules<TSource, TDestination>> CaseMap { get; set; }
    }

    /// <summary>
    /// MyMapperSwitch - Generic class
    /// </summary>
    /// <typeparam name="TSource">The source</typeparam>
    /// <typeparam name="TDestination">The destination</typeparam>
    /// <typeparam name="TSourceProperty">The property</typeparam>
    internal class MyMapperSwitch<TSource, TDestination, TSourceProperty> : IMyMapperSwitch<TSource, TDestination, TSourceProperty>, 
                                                                            IMyMapperSwitchElse<TSource, TDestination, TSourceProperty>, 
                                                                            IMyMapperSwitchEnd<TSource, TDestination>
        where TSource : class
        where TDestination : class, new()
    {
        TSourceProperty Property { get; set; }
        TSource Source { get; set; }
        IMyMapper<TSource, TDestination> Mapper { get; set; }
        Action<TDestination, TSourceProperty> ElseThen { get; set; }
        Action<IMyMapperRules<TSource, TDestination>> ElseThenMap { get; set; }        
        Dictionary<Func<TSourceProperty, bool>, SwitchThen<TSource, TDestination, TSourceProperty>> cases = new Dictionary<Func<TSourceProperty, bool>, SwitchThen<TSource, TDestination, TSourceProperty>>();

        public MyMapperSwitch(TSource source, TSourceProperty property, IMyMapper<TSource, TDestination> mapper)
        {
            this.Source = source;
            this.Property = property;
            this.Mapper = mapper;
        }

        public IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> CaseMap(
                                                                                        Expression<Func<TSourceProperty, bool>> when, 
                                                                                        Action<IMyMapperRules<TSource, TDestination>> then
                                                                                    )
        {
            SwitchThen<TSource, TDestination, TSourceProperty> switchThen = new SwitchThen<TSource, TDestination, TSourceProperty>();
            switchThen.CaseMap = then;

            cases.Add(when.Compile(), switchThen);

            return this;
        }

        public IMyMapperSwitchElse<TSource, TDestination, TSourceProperty> Case(
                                                                                    Expression<Func<TSourceProperty, bool>> when, 
                                                                                    Action<TDestination, TSourceProperty> then
                                                                                )
        {
            SwitchThen<TSource, TDestination, TSourceProperty> switchThen = new SwitchThen<TSource, TDestination, TSourceProperty>();
            switchThen.Case = then;

            cases.Add(when.Compile(), switchThen);

            return this;
        }

        public IMyMapperSwitchEnd<TSource, TDestination> ElseMap(Action<IMyMapperRules<TSource, TDestination>> then)
        {
            ElseThenMap = then;

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
                    var theCase = cases[when];

                    if (theCase.Case != null)
                    {
                        theCase.Case(this.Mapper.Exec(), this.Property);
                    }
                    else if (theCase.CaseMap != null)
                    {
                        theCase.CaseMap(this.Mapper);
                    }

                    return this.Mapper;
                }                
            }

            if (ElseThen != null)
            {
                ElseThen(this.Mapper.Exec(), this.Property);
            }
            else if (ElseThenMap != null)
            {
                ElseThenMap(this.Mapper);
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

        [Obsolete("Exec is deprecated.", true)]
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

        /// <summary>
        /// Map source to destination
        /// </summary>
        /// <param name="source">The source</param>
        /// <param name="automap">Flag to use auto mapping (reflective)</param>
        /// <returns cref="IMyMapperRules">The mapper rules</returns>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException exception</exception>
        public IMyMapperRules<TSource, TDestination> Map(TSource source, bool automap = true)
        {
            if (source == null)
                throw new ArgumentNullException();

            this.Source = source;                                   

            this.Destination = new TDestination();

            if (automap)
                this.Destination = new EntityConverter<TSource, TDestination>().Convert(this.Source);

            return this;
        }

        /// <summary>
        /// With
        /// </summary>
        /// <typeparam name="TProperty">The proerty</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <returns><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> With<TProperty>(
                                                                        Expression<Func<TSource, TProperty>> source,
                                                                        Action<TDestination, TProperty> destination
                                                                )
        {
            var sourceProp = source.Compile()(this.Source);

            destination(this.Destination, sourceProp);

            return this;
        }

        /// <summary>
        /// With
        /// </summary>
        /// <typeparam name="TSourceResult">The source result</typeparam>
        /// <typeparam name="TDestinationResult">The destination result</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
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

        /// <summary>
        /// With
        /// </summary>
        /// <typeparam name="TSourceResult">The source result type</typeparam>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
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

            destination(this.Destination, destinationList.ToList());

            return this;
        }

        /// <summary>
        /// Parallel With - Uses PLINQ
        /// </summary>
        /// <typeparam name="TSourceResult">The source result type</typeparam>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> ParallelWith<TSourceResult, TDestinationResult>(
                                                Expression<Func<TSource, ICollection<TSourceResult>>> source,
                                                Action<TDestination, ICollection<TDestinationResult>> destination,
                                                Func<TSourceResult, TDestinationResult> map
                                            )
        where TSourceResult : class
        where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            var destinationList = sourceList.AsParallel().Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }

        /// <summary>
        /// With
        /// </summary>
        /// <typeparam name="TSourceResult">The source result type</typeparam>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns>><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
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

        /// <summary>
        /// Parallel With - Uses PLINQ
        /// </summary>
        /// <typeparam name="TSourceResult">The source result type</typeparam>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns>><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> ParallelWith<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IEnumerable<TSourceResult>>> source,
                                                        Action<TDestination, IEnumerable<TDestinationResult>> destination,
                                                        Func<TSourceResult, TDestinationResult> map
                                                    )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            var destinationList = sourceList.AsParallel().Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }

        /// <summary>
        /// With
        /// </summary>
        /// <typeparam name="TSourceResult">The source result type</typeparam>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns>><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> With<TSourceResult, TDestinationResult>(
                                                        Expression<Func<TSource, IList<TSourceResult>>> source,
                                                        Action<TDestination, IList<TDestinationResult>> destination,
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

        /// <summary>
        /// Parallel With - Uses PLINQ
        /// </summary>
        /// <typeparam name="TSourceResult">The source result type</typeparam>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns>><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> ParallelWith<TSourceResult, TDestinationResult>(
                                                Expression<Func<TSource, IList<TSourceResult>>> source,
                                                Action<TDestination, IList<TDestinationResult>> destination,
                                                Func<TSourceResult, TDestinationResult> map
                                            )
            where TSourceResult : class
            where TDestinationResult : class, new()
        {
            var sourceList = source.Compile()(this.Source);

            var destinationList = sourceList.AsParallel().Select(map);

            destination(this.Destination, destinationList.ToList());

            return this;
        }

        /// <summary>
        /// With
        /// </summary>
        /// <typeparam name="TDestinationResult">The destination result type</typeparam>
        /// <param name="source">The source datatable</param>
        /// <param name="destination">The destination</param>
        /// <param name="map">The source to destination map</param>
        /// <returns>><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> With<TDestinationResult>(
                                                        Expression<Func<TSource, DataTable>> source,
                                                        Action<TDestination, IList<TDestinationResult>> destination,
                                                        Func<DataRow, TDestinationResult> map
                                                    )
            where TDestinationResult : class, new()
        {
            DataTable sourceList = source.Compile()(this.Source);

            var destinationList = new List<TDestinationResult>();

            foreach (DataRow row in sourceList.Rows)
            {
                destinationList.Add(map(row));
            }

            destination(this.Destination, destinationList);

            return this;
        }

        /// <summary>
        /// With When
        /// </summary>
        /// <typeparam name="TProperty">The property type</typeparam>
        /// <param name="when">The when condition</param>
        /// <param name="source">The source</param>
        /// <param name="destination">The destination</param>
        /// <returns><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> WithWhen<TProperty>(
                                                                        Expression<Func<TSource, bool>> when,
                                                                        Func<TSource, TProperty> source,
                                                                        Action<TDestination, TProperty> destination
                                                                    )
        {
            if (!when.Compile()(this.Source))
            {
                return this;
            }

            var sourceProp = source(this.Source);

            destination(this.Destination, sourceProp);

            return this;
        }

        /// <summary>
        /// When
        /// </summary>
        /// <param name="when">The when condition</param>
        /// <param name="then">The then action</param>
        /// <returns><see cref="IMyMapperRules<TSource, TDestination>"/></returns>
        public IMyMapperRules<TSource, TDestination> When(
                                                        Expression<Func<TSource, bool>> when,
                                                        Action<IMyMapperRules<TSource, TDestination>> then
                                                    )
        {
            if (when.Compile()(this.Source))
            {
                then(this); 
            }

            return this;
        }

        /// <summary>
        /// Switch
        /// </summary>
        /// <typeparam name="TProperty">The property type</typeparam>
        /// <param name="on">The property for the switch</param>
        /// <returns><see cref="IMyMapperSwitch<TSource, TDestination, TProperty>"/></returns>
        public IMyMapperSwitch<TSource, TDestination, TProperty> Switch<TProperty>(Expression<Func<TSource, TProperty>> on)
        {
            IMyMapperSwitch<TSource, TDestination, TProperty> sw = new MyMapperSwitch<TSource, TDestination, TProperty>(
                                                                                           this.Source, 
                                                                                           on.Compile()(this.Source), 
                                                                                           this
                                                                                       );

            return sw;
        }

        /// <summary>
        /// Exec
        /// </summary>
        /// <returns>The destination <see cref="TDestination"/></returns>
        public TDestination Exec()
        {
            return this.Destination;
        }

        [Obsolete("Exec is deprecated.", true)]
        public TDestination Exec(TSource source, Func<TSource, IMyMapper<TSource, TDestination>, TDestination> map)
        {
            return map(source, this);
        }

        /// <summary>
        /// Exec
        /// </summary>
        /// <typeparam name="TConverter">The converter type</typeparam>
        /// <param name="source">The source</param>
        /// <returns>The destination <see cref="TDestination"/></returns>
        public TDestination Exec<TConverter>(TSource source)
            where TConverter : ITypeConverter<TSource, TDestination>, new()
        {
            return new TConverter().Convert(source);
        }
    }
#endregion
}
