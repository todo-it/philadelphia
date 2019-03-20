namespace Philadelphia.Web {
    public class GrouperDefOrAggregatorDef<T> {
        public GrouperDef<T> Grouper { get; }
        public AggregatorDef<T> Aggregator {get; }

        private GrouperDefOrAggregatorDef(GrouperDef<T> grouper, AggregatorDef<T> aggregator) {
            Grouper = grouper;
            Aggregator = aggregator;
        }

        public static GrouperDefOrAggregatorDef<T>Create(GrouperDef<T> grouper) {
            return new GrouperDefOrAggregatorDef<T>(grouper, null);
        }

        public static GrouperDefOrAggregatorDef<T>Create(AggregatorDef<T> aggregator) {
            return new GrouperDefOrAggregatorDef<T>(null, aggregator);
        }

        public static implicit operator GrouperDefOrAggregatorDef<T>(GrouperDef<T> inp) {
            return Create(inp);
        }
        
        public static implicit operator GrouperDefOrAggregatorDef<T>(AggregatorDef<T> inp) {
            return Create(inp);
        }
    }
}