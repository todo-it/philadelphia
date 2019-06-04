using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
// ReSharper disable InconsistentNaming

//unfortunately bridge.net doesn't support Enum generic constraint: https://blogs.msdn.microsoft.com/seteplia/2018/06/12/dissecting-new-generics-constraints-in-c-7-3/

namespace Philadelphia.Web {
    public delegate void RadioBasedSingleChoiceItemAdder<CtxT,T>(
        CtxT ctxFromBeforeAddItems, 
        HTMLElement container, 
        T internalValue, 
        HTMLInputElement inputElem,
        HTMLLabelElement labelElem, 
        int itemNo);

    public class RadioBasedSingleChoiceUtilExtensions {
        /// <summary>EnumT must be enum. It is expected that Convert.ToInt32(x) call will succeed</summary>
        public static RadioBasedSingleChoice BuildForEnum<EnumT>(
                EnumT defaultValue, Func<EnumT,string> getLabel) where EnumT:struct {

            return BuildForEnum(null, defaultValue, getLabel);
        }

        /// <summary>EnumT must be enum. It is expected that Convert.ToInt32(x) call will succeed</summary>
        public static RadioBasedSingleChoice BuildForEnum<EnumT>(
                string labelOrNull, EnumT defaultValue, Func<EnumT,string> getLabel) where EnumT:struct {

            return new RadioBasedSingleChoice(labelOrNull) {
                PermittedValues = 
                    EnumExtensions
                        .GetEnumValues<EnumT>()
                        .Select(x => Tuple.Create(Convert.ToInt32(x) + "", getLabel(x))),
                Value = Tuple.Create(Convert.ToInt32(defaultValue) + "", getLabel(defaultValue))
            };
        }
        
        /// <summary>EnumT must be enum. It is expected that Convert.ToInt32(x) call will succeed</summary>
        public static RadioBasedSingleChoice BuildForEnum<EnumT,CtxT>(
                EnumT defaultValue, Func<EnumT,string> getLabel,
                Func<int,EnumT> intToEnum,
                Func<HTMLElement,int,CtxT> beforeAddItems,
                RadioBasedSingleChoiceItemAdder<CtxT,EnumT> itemAdderOrNull = null) where EnumT:struct {

            return BuildForEnum(null, defaultValue, getLabel, intToEnum, beforeAddItems, itemAdderOrNull);
        }

        /// <summary>EnumT must be enum. It is expected that Convert.ToInt32(x) call will succeed</summary>
        public static RadioBasedSingleChoice BuildForEnum<EnumT,CtxT>(
                string labelOrNull, EnumT defaultValue, Func<EnumT,string> getLabel,
                Func<int,EnumT> intToEnum,
                Func<HTMLElement,int,CtxT> beforeAddItems,
                RadioBasedSingleChoiceItemAdder<CtxT,EnumT> itemAdderOrNull = null) where EnumT:struct {

            RadioBasedSingleChoiceItemAdder itemAdder = null;

            if (itemAdderOrNull != null) {
                itemAdder = (ctx, cntnr, rawItem, itemAsEl, labelEl, itemNo) => 
                    itemAdderOrNull(
                        (CtxT)ctx, cntnr, intToEnum(Convert.ToInt32(rawItem.Item1)), itemAsEl, labelEl, itemNo);
            }

            var result = new RadioBasedSingleChoice(labelOrNull, itemAdder) {
                    BeforeAddItems = (x,y) => beforeAddItems(x, y),
                    PermittedValues = 
                        EnumExtensions
                            .GetEnumValues<EnumT>()
                            .Select(x => Tuple.Create(Convert.ToInt32(x) + "", getLabel(x))),
                    Value = Tuple.Create(Convert.ToInt32(defaultValue) + "", getLabel(defaultValue)) };
            
            return result; 
        }
    }
}
