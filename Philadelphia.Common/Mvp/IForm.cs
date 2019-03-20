using System;

namespace Philadelphia.Common {
    // Question: shhould wwe maybe introduce a standard implementation instead of interface?
    /// <summary>
    /// generally every form that is created needs toi advertise that it wants to end itself due to some reason.
    /// This interface is meant to provide 'standard' way to do it so thjart in every form you can use same name/signature
    /// </summary>
    public interface IForm<WidgetT,SelfT,ResultT> : IBareForm<WidgetT> { 
        event Action<SelfT,ResultT> Ended;
    }
}
