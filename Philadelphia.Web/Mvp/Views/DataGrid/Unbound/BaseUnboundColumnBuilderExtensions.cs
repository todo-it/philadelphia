using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class BaseUnboundColumnBuilderExtensions {
        public static ValueContainingUnboundColumnBuilder<RecordT,decimal?> WithValueLocalized<RecordT>(
            this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,decimal?> valueProvider, DecimalFormat format) where RecordT : new() {
        
            return self.WithValue(
                valueProvider, 
                x => x.HasValue ? I18n.Localize(x.Value, format) : I18n.Translate("(unknown)"), 
                (val,exp) => {
                    if (val.HasValue) {
                        exp.Export(val.Value);
                    } else {
                        exp.Export("");
                    }
                });
        }

        public static ValueContainingUnboundColumnBuilder<RecordT,DateTime?> WithValueLocalized<RecordT>(
            this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,DateTime?> valueProvider, DateTimeFormat format) where RecordT : new() {
        
            return self.WithValue(
                valueProvider, 
                x => x.HasValue ? I18n.Localize(x.Value, format) : I18n.Translate("(unknown)"), 
                (val,exp) => {
                    if (val.HasValue) {
                        exp.Export(val.Value);
                    } else {
                        exp.Export("");
                    }
                });
        }

        public static ValueContainingUnboundColumnBuilder<RecordT,decimal> WithValueLocalized<RecordT>(
            this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,decimal> valueProvider, DecimalFormat format) where RecordT : new() {
        
            return self.WithValue(valueProvider, x => I18n.Localize(x, format), (val,exp) => exp.Export(val));
        }
        
        public static ValueContainingUnboundColumnBuilder<RecordT,DecimalWithPrecision> WithValueLocalized<RecordT>(
                this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,DecimalWithPrecision> valueProvider) where RecordT : new() {
        
            return self.WithValue(
                valueProvider, 
                x => x == null ? "" : I18n.Localize(
                    x.Value, DecimalFormatExtensions.GetWithPrecision(x.Precision)), 
                (val,exp) => {
                    if (val == null) {
                        exp.Export("");
                    } else {
                        exp.Export(val.RoundedValue);
                    }
                });
        }
        
        public static ValueContainingUnboundColumnBuilder<RecordT,int> WithValueLocalized<RecordT>(
            this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,int> valueProvider) where RecordT : new() {
        
            return self.WithValue(valueProvider, I18n.Localize, (val,exp) => exp.Export(val));
        }

        public static ValueContainingUnboundColumnBuilder<RecordT,bool> WithValueLocalized<RecordT>(
            this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,bool> valueProvider, BoolFormat format = BoolFormat.UnicodeBallotBox) where RecordT : new() {
        
            return self.WithValue(valueProvider, x => I18n.Localize(x, format), (val,exp) => exp.Export(val));
        }
        
        public static ValueContainingUnboundColumnBuilder<RecordT,DateTime> WithValueLocalized<RecordT>(
            this BaseUnboundColumnBuilder<RecordT> self, Func<RecordT,DateTime> valueProvider, DateTimeFormat format) where RecordT : new() {
        
            return self.WithValue(valueProvider, x => I18n.Localize(x, format), (val,exp) => exp.Export(val));
        }
    }
}
