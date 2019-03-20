using System;

namespace Philadelphia.Web {
    public class TransformationMediator : ITransformationMediator {
        public Action<ChangeOrRemove> UserFilterChangedHandler {get;}
        public Action<ChangeOrRemove> UserGroupingChangedHandler {get;}
        public Action<ChangeOrRemove> UserAggregationChangedHandler {get;}
        
        public Action<string> ProgrammaticGroupingChangedHandler { get; private set; }
        public Action<string> ProgrammaticAggregationChangedHandler { get; private set; }

        /// <summary>for constructing initiated from model side</summary>
        public TransformationMediator(
                    Action<ChangeOrRemove> userFilterChangedHandler,
                    Action<ChangeOrRemove> userGroupingChangedHandler,
                    Action<ChangeOrRemove> userAggregationChangedHandler) {

            UserFilterChangedHandler = userFilterChangedHandler;
            UserGroupingChangedHandler = userGroupingChangedHandler;
            UserAggregationChangedHandler = userAggregationChangedHandler;
        }

        /// <summary>for post construction initialization from UI side</summary>
        public void InitUserSide(
                    Action<string> groupingChangedHandler, 
                    Action<string> aggregationChangedHandler) {

            ProgrammaticGroupingChangedHandler = groupingChangedHandler;
            ProgrammaticAggregationChangedHandler = aggregationChangedHandler;
        }
    }
}
