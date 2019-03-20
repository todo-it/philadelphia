namespace Philadelphia.Web {
    public class Overlap {
        public Range ToPrepend { get; }
        public Range ToAppend { get; }
        public Range ToPreTrim { get; }
        public Range ToPostTrim { get; }
        
        public Overlap(bool itemsChanged, ViewPortState old, ViewPortState anew) {
            if (itemsChanged || //cannot make asumptions about order of rows as it could change due to sorting, insers, removal
                anew.LastVisibleRowOrMinusOneIfEmpty < old.FirstVisibleRowOrMinusOneIfEmpty || //new is entirely before old
                anew.FirstVisibleRowOrMinusOneIfEmpty > old.LastVisibleRowOrMinusOneIfEmpty) { //old is entirely before new
                
                ToPreTrim = old.IsEmpty ? 
                        Range.CreateEmpty() 
                    : 
                        Range.Create(old.FirstVisibleRow, old.LastVisibleRow);
                ToPrepend = Range.CreateEmpty();

                ToPostTrim = Range.CreateEmpty();
                ToAppend = anew.IsEmpty ? 
                        Range.CreateEmpty() 
                    : 
                        Range.Create(anew.FirstVisibleRow, anew.LastVisibleRow);
                return;
            }
            
            //partially or entirely contained

            var overlapStart = anew.FirstVisibleRow - old.FirstVisibleRow;
            var overlapEnd = anew.LastVisibleRow - old.LastVisibleRow;
                
            //new range starts later?
            ToPreTrim = overlapStart > 0 ? 
                    Range.Create(old.FirstVisibleRow, old.FirstVisibleRow + overlapStart - 1) 
                : 
                    Range.CreateEmpty();

            //new range begins earlier?
            //handles case: when scroll is at bottom and then many (but not all) rows are deleted (f.e. grouping disabled) then wrong overlap would be calculated otherwise
            ToPrepend = overlapStart < 0 && anew.FactVisibleRowsCount > 0 ? 
                    Range.Create(anew.FirstVisibleRow, old.FirstVisibleRow - 1) 
                :
                    Range.CreateEmpty();
            
            //new range ends earlier?
            ToPostTrim = overlapEnd < 0 ? 
                    Range.Create(old.LastVisibleRow + overlapEnd + 1, old.LastVisibleRow) 
                : 
                    Range.CreateEmpty();

            //new range ends later?
            ToAppend = overlapEnd > 0 && anew.FactVisibleRowsCount > 0 ? 
                    Range.Create(old.LastVisibleRow + 1, anew.LastVisibleRow)
                : 
                    Range.CreateEmpty();
        }

        public override string ToString() {
            return string.Format("<Overlap toPreTrim={0} toPrepend={1} toPostTrim={2} toAppend={3}>", ToPreTrim, ToPrepend, ToPostTrim, ToAppend);
        }
    }
}
