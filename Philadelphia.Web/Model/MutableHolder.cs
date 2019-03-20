namespace Philadelphia.Web {
    public class MutableHolder<T> {
        public T Value {get; set; }

        public MutableHolder() {}
        public MutableHolder(T val) {
            Value = val;
        }
    }
}
