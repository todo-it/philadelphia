using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    /// <summary>
    /// wrapper class to make it possible to use implicit conversion. It would have cleaner design if C# allowed implicit conversion from/to interfaces but it doesn't.
    /// If it would allow then it would be cleaner to do it:
    /// a) in IView&lt;WidgetT&gt;[] IFormView->Render() to wrap string as IView or 
    /// b) in RenderElem&lt;WidgetT&gt;[] IFormView->Render() to wrap IView as RenderElem
    /// As C# doesn't permit those scenarios closes thing is used having drawback requiring implicit operator in all IView implementations
    /// </summary>
    /// <typeparam name="WidgetT"></typeparam>
    public class RenderElem<WidgetT>
    {
        public IView<WidgetT> Iview {get; private set;}
        public string Token {get; private set;}
        public WidgetT NativeItm {get; private set;}

        private RenderElem() {}        
        
        public static RenderElem<WidgetT> Create(IView<WidgetT> inp) => new RenderElem<WidgetT>{Iview = inp};
        public static implicit operator RenderElem<WidgetT>(string inp) => new RenderElem<WidgetT> {Token = inp};
        public static implicit operator RenderElem<WidgetT>(WidgetT inp) => new RenderElem<WidgetT> {NativeItm = inp};
    }
}
