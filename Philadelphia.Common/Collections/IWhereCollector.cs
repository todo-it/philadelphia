using System;

namespace Philadelphia.Common {
    public interface IWhereCollector<RecordT> {
        void AddWhereRule<T>(Func<RecordT,T> key, Func<T,bool> shouldStayInSet);
    }
}