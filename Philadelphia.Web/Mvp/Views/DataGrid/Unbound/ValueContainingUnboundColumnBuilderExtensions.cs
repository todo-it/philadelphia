using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ValueContainingUnboundColumnBuilderExtensions {
        public static TransformableUnboundColumnBuilder<RecordT,string> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,string> self, 
                params GrouperDefOrAggregatorDef<string>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForString(x, additionalGrouperOrAggr));
        }

        public static TransformableUnboundColumnBuilder<RecordT,DateTime?> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,DateTime?> self, DateTimeFormat format,
                params GrouperDefOrAggregatorDef<DateTime?>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForNullableDateTime(format, x, additionalGrouperOrAggr));
        }
        
        public static TransformableUnboundColumnBuilder<RecordT,DateTime> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,DateTime> self, DateTimeFormat format,
                params GrouperDefOrAggregatorDef<DateTime>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForDateTime(format, x, additionalGrouperOrAggr));
        }

        public static TransformableUnboundColumnBuilder<RecordT,int> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,int> self,
                params GrouperDefOrAggregatorDef<int>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForInt(x, additionalGrouperOrAggr));
        }
        
        public static TransformableUnboundColumnBuilder<RecordT,int?> TransformableDefault<RecordT>(
            this ValueContainingUnboundColumnBuilder<RecordT,int?> self,
            params GrouperDefOrAggregatorDef<int?>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForNullableInt(x, additionalGrouperOrAggr));
        }
        
        public static TransformableUnboundColumnBuilder<RecordT,decimal> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,decimal> self, DecimalFormat format,
                params GrouperDefOrAggregatorDef<decimal>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForDecimal(format, x, additionalGrouperOrAggr));
        }
        
        public static TransformableUnboundColumnBuilder<RecordT,DecimalWithPrecision> TransformableDefault<RecordT>(
            this ValueContainingUnboundColumnBuilder<RecordT,DecimalWithPrecision> self,
            params GrouperDefOrAggregatorDef<DecimalWithPrecision>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(
                x => DataGridColumnController.ForDecimalWithPrecision(x, additionalGrouperOrAggr));
        }
        
        public static TransformableUnboundColumnBuilder<RecordT,decimal?> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,decimal?> self, DecimalFormat format,
                params GrouperDefOrAggregatorDef<decimal?>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForNullableDecimal(format, x, additionalGrouperOrAggr));
        }
        
        public static TransformableUnboundColumnBuilder<RecordT,bool> TransformableDefault<RecordT>(
                this ValueContainingUnboundColumnBuilder<RecordT,bool> self,
                params GrouperDefOrAggregatorDef<bool>[] additionalGrouperOrAggr) where RecordT : new() {
            
            return self.Transformable(x => DataGridColumnController.ForBool(x, additionalGrouperOrAggr));
        }
    }
}
