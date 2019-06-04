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
        public static RadioBasedSingleChoice Build<T,CtxT>(
                T defaultValue, Func<T,string> getLabel, 
                Func<int,T> intToItem, Func<T,int> itemToInt) {

            return Build<T,CtxT>(null, defaultValue, getLabel, intToItem, itemToInt);
        }
        
        public static RadioBasedSingleChoice Build<T,CtxT>(
                string labelOrNull, T defaultValue, Func<T,string> getLabel, 
                Func<int,T> intToItem, Func<T,int> itemToInt) {

            return Build<T,CtxT>(labelOrNull, defaultValue, getLabel, intToItem, itemToInt, null, null);
        }

        public static RadioBasedSingleChoice Build<T,CtxT>(
                string labelOrNull, T defaultValue, Func<T,string> getLabel,
                Func<int,T> intToItem, Func<T,int> itemToInt,
                Func<HTMLElement,int,CtxT> beforeAddItemsOrNull = null,
                RadioBasedSingleChoiceItemAdder<CtxT,T> itemAdderOrNull = null) {
            
            RadioBasedSingleChoiceItemAdder itemAdder = null;

            if (itemAdderOrNull != null) {
                itemAdder = (ctx, cntnr, rawItem, itemAsEl, labelEl, itemNo) => 
                    itemAdderOrNull(
                        (CtxT)ctx, cntnr, intToItem(Convert.ToInt32(rawItem.Item1)), itemAsEl, labelEl, itemNo);
            }
            
            var result = new RadioBasedSingleChoice(labelOrNull, itemAdder);
            
            if (beforeAddItemsOrNull != null) {
                result.BeforeAddItems = (x,y) => beforeAddItemsOrNull(x, y);
            }

            var defVal = Tuple.Create(itemToInt(defaultValue) + "", getLabel(defaultValue));
            result.PermittedValues = new [] {defVal };
            result.Value = defVal;
            
            return result; 
        }
        
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
