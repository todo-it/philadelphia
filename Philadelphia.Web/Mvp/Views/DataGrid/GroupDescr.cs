namespace Philadelphia.Web {
    public class GroupDescr {
        public int FromPhsIdx {get; set; }
        public int TillPhsIdx {get; set; }

        public int FromVisIdx {get; set; }
        public int TillVisIdx {get; set; }

        //TODO make it string and eliminate all .ToString() calls on it
        public object KeyData {get; set; }
        public string UserFriendlyGroupName {get; set; }
        
        public override string ToString() {
            return $"<GroupDescr key={KeyData} UserFriendlyGroupName={UserFriendlyGroupName} (fromPhsIdx={FromPhsIdx};tillPhsIdx={TillPhsIdx}) (fromVisIdx={FromVisIdx};tillVisIdx={TillVisIdx})>";
        }
    }
}
