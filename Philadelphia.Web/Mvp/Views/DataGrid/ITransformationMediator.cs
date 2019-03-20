using System;

namespace Philadelphia.Web {
    public interface ITransformationMediator {
        Action<ChangeOrRemove> UserFilterChangedHandler {get;}
        Action<ChangeOrRemove> UserGroupingChangedHandler {get;}
        Action<ChangeOrRemove> UserAggregationChangedHandler {get;}

        /// <summary>
        /// string parameter meaning: 
        /// -to enable grouping give non null groupingFunction label as found in available grouping functions aka GrouperDef<T>
        /// -to disable grouping give null
        /// </summary>
        Action<string> ProgrammaticGroupingChangedHandler { get; }

        /// <summary>
        /// string parameter meaning: 
        /// -to enable aggregation give non null aggregationFunction label as found in available aggregation functions aka AggregatorDef<T>
        /// -to disable aggregation give null
        /// </summary>
        Action<string> ProgrammaticAggregationChangedHandler { get; }

        void InitUserSide(Action<string> groupingChangedHandler, Action<string> aggregationChangedHandler);
    }
}
