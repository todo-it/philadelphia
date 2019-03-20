﻿namespace Philadelphia.Web {
    public class ProcessingOutcome {
        public Direction? Cursor { get; set; }
        public bool? Consumed { get; set; }
        public bool Changed { get; set; }
        public bool PendingCharRemoval { get; set; }
        public Direction? Navigate { get; set; }
        public bool NavigateIsProgrammatic { get; set; }
        
        public override string ToString() {
            return $"<ProcessingOutcome Cursor={Cursor} Consumed={Consumed} Changed={Changed} PendingCharRemoval={PendingCharRemoval} Navigate={Navigate} NavigateIsProgrammatic={NavigateIsProgrammatic}>";
        }
    }
}
