using System.Collections.Generic;

namespace Philadelphia.Common {
    /// <summary>collection providing efficient random read access</summary>
    public interface IRandomAccessCollection<T> : IEnumerable<T> {
        T this[int position] {get;}
        int Length { get; }
        int IndexOf(T itm);
    }
}
