using System;

namespace Philadelphia.Web {
    public class ViewPortState {
        public double RawScrollPos { get; set;}
        public bool IsInvalidScrollPos { get; set;}
        public int PhysicalRowsCount { get; set;}
        public int VirtualRowsCount { get; set;}
        public int AllRowsCount { get; set;}
        public int TheoreticalVisibleRowsCount { get; set;}
        public int FirstVisibleRow { get; set;}
        public double IncompleteLeadRowPad { get; set;}
        public double IncompleteBodyPad { get; set;}
        public int FirstVisibleRowOrMinusOneIfEmpty => IsEmpty ? -1 : FirstVisibleRow;
        
        public int FactVisibleRowsCount => Math.Min(AllRowsCount - FirstVisibleRow, TheoreticalVisibleRowsCount);
        public int LastVisibleRow => Math.Min(AllRowsCount - 1, FirstVisibleRow + FactVisibleRowsCount);
        public int LastVisibleRowOrMinusOneIfEmpty => IsEmpty ? -1 : LastVisibleRow;

        public bool IsEmpty => 
            AllRowsCount == 0 && TheoreticalVisibleRowsCount == 0 && FirstVisibleRow == -1 ||
            FactVisibleRowsCount <= 0;

        public static ViewPortState CreateEmpty() {
            return new ViewPortState {
                AllRowsCount = 0,
                TheoreticalVisibleRowsCount = 0,
                FirstVisibleRow = -1
            };
        }

        public override string ToString() {
            return 
                $"<ViewPortState RawScrollPos={RawScrollPos} IsInvalidScrollPos={IsInvalidScrollPos} "+
                $"IsEmpty={IsEmpty} AllRowsCount={AllRowsCount} TheoreticalVisibleRowsCount={TheoreticalVisibleRowsCount} " +
                $"FirstVisibleRow={FirstVisibleRow} FactVisibleRowsCount={FactVisibleRowsCount} LastVisibleRow={LastVisibleRow} "+
                $"IncompleteLeadRowPad={IncompleteLeadRowPad} IncompleteBodyPad={IncompleteBodyPad} " +
                $"PhysicalRowsCount={PhysicalRowsCount} VirtualRowsCount={VirtualRowsCount}>";
        }
    }
}
