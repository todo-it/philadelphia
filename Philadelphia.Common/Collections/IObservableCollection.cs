namespace Philadelphia.Common {
    public delegate void CollectionChanged<T>(int insertAt, T[] inserted, T[] removed);

    public interface IObservableCollection<T> : IRandomAccessCollection<T> {
        /// <summary>
        /// insertedAt,inserted,deleted
        /// </summary>
        event CollectionChanged<T> Changed;
		
        void Clear();
        void DeleteAt(int position);
        void Delete(params T[] items);
        void InsertAt(int position, params T[] items);
        void Replace(params T[] newItems);
    }
}
