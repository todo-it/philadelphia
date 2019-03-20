using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ActionsBuilder {
        private readonly List<IView<HTMLElement>> _actions = new List<IView<HTMLElement>>();

        private ActionsBuilder(IView<HTMLElement>[] actions) => 
            _actions.AddRange(actions);

        public static ActionsBuilder For(params IView<HTMLElement>[] actions) => 
            new ActionsBuilder(actions);

        /// <summary>
        /// include datagrid buttons
        /// </summary>
        /// <param name="datagrid"></param>
        /// <returns></returns>
        public ActionsBuilder AddFrom(ITableView datagrid) {
            _actions.AddRange(datagrid.Actions);
            return this;
        }


        /// <summary>
        /// include menu items
        /// </summary>
        public ActionsBuilder AddFrom(IMenuBarView menu) {
            _actions.AddRange(menu.Actions);
            return this;
        }
        
        public IView<HTMLElement>[] Build() => _actions.ToArray();

        /// <summary>
        /// allow implicit conversion to property IFormView.Actions
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator IView<HTMLElement>[](ActionsBuilder d) => 
            d.Build();
    }
}
