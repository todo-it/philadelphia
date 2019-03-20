using Bridge;

namespace Philadelphia.Web {
    public static class BridgeObjectUtil {
        [Template("{name} in {self:raw}")]
        public static extern bool HasFieldOrMethod(this object self, string name);
        
        /// <summary>
        /// created in order to access Chrome's input[type="number"] selectionStart property
        /// 
        /// it is present there BUT accessing it causes DOMException Failed to read the 'selectionStart' property 
        /// from 'HTMLInputElement': The input element's type ('number') does not support selection
        /// </summary>
        /// <param name="self"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [Template("(function(){try { {self:raw}.{name:raw}; return true; } catch (ex) {return false;}})()")]
        public static extern bool IsFieldReadable(this object self, string name);
        
        [Template("{toUnbox:raw}")]
        public static extern T NoOpCast<T>(object toUnbox);

        [Template("{self:raw}.{name:raw}")]
        public static extern object GetFieldValue(this object self, string name);
        
        [Template("{self:raw}.{name:raw} = {value:raw}")]
        public static extern void SetFieldValue(this object self, string name, object value);
        
        [Template("{self:raw}()")]
        public static extern object CallSelf(this object self);
        
        [Template("{self:raw}.{method:raw}({args:raw})")]
        public static extern object CallMethod(this object self, string method, params object[] args);
    }
}
