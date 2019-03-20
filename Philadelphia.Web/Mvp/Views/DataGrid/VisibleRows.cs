using System.Collections.Generic;

namespace Philadelphia.Web {
    public class VisibleRows<RecordT> {
        private readonly List<RecordT> _physical = new List<RecordT>();
        private readonly List<GroupDescr> _grps = new List<GroupDescr>();

        public int GetRowIndexOf(RecordT oldValue) {
            return _physical.IndexOf(oldValue);
        }

        public void RemoveAt(int position) {
            _physical.RemoveAt(position);
            _grps.RemoveAt(position);
        }

        public void Append(RecordT item) {
            _physical.Add(item);
            _grps.Add(null);
        }
        
        public void Append(GroupDescr grp) {
            _physical.Add(default(RecordT));
            _grps.Add(grp);
        }

        public void Insert(int pos, RecordT item) {
            _physical.Insert(pos, item);
            _grps.Insert(pos, null);
        }

        public void Insert(int idx, GroupDescr grp) {
            _physical.Insert(idx, default(RecordT));
            _grps.Insert(idx, grp);
        }

        public int GetVisibleCount() {
            return _physical.Count;
        }
        
        public DataOrGroup GetRowTypeAt(int position) {
            if (_grps[position] != null) {
                return DataOrGroup.Group;
            }
            return DataOrGroup.Data;
        }

        public RecordT GetDataAt(int position) {
            return _physical[position];
        }

        //public void DebugPrintContents() {
        //    Logger.Debug(GetType(), "VisibleRows contents start {0}; {1}", _physical.Count, _grps.Count);

        //    for (var i=0; i < _physical.Count; i++) {
        //        Logger.Debug(GetType(), "VisibleRows contents itm {0}; {1}", _physical[i], _grps[i]);
        //    }

        //    Logger.Debug(GetType(), "VisibleRows contents stop");
        //}
    }
}
