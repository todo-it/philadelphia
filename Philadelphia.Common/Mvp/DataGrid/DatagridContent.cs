namespace Philadelphia.Common {
    public class DatagridContent {
        // why arrays?
        //
        // Bridge.net serializes List<T> as 
        //     { 'items':[]} 
        // but Json.NET expects format for this type to be:
        //     []
            
        public string[] labels;
        public DatagridCell[][] rows;
    }
}
