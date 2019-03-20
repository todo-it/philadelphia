namespace Philadelphia.Web {
    public enum RebuildGroupsReason {
        ItemsInModelChanged,
        Initialization,
        UserCausedGroupingChange,
        ProgrammaticGroupingChange,
        ProgrammaticAggregationChange
    }
}
