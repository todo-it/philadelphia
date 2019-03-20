using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class TransformableUnboundColumnBuilderExtensions {
        public static EditableUnboundColumn<RecordT,DataT> Observes<RecordT,DataT>(
            this TransformableUnboundColumnBuilder<RecordT,DataT> self, 
            Func<RecordT,string> propertyName) where RecordT : IHasSubscribeable,new() {

            return self.Observes(x => x.Subscribeable, propertyName);
        } 
    }
}
