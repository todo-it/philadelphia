using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridFormView : IFormView<HTMLElement> {
        public InputTypeButtonActionView Editor { get; } = new InputTypeButtonActionView("Edit");
        public InputTypeButtonActionView Creator { get; } = new InputTypeButtonActionView("Create");
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(Creator, Editor).AddFrom(Items);
        public HtmlTableBasedTableView Items { get; } = new HtmlTableBasedTableView();
        public LabellessReadOnlyView Help = new LabellessReadOnlyView("div", TextType.TreatAsHtml)
            .WithCssClass("grayedOut")
            .With(x => x.Value = @"
                To activate editor: doubleclick on table row OR click table row and press Edit button.
                Notice following screen features
                <div style='display:flex;'>
                    <div><ul style='margin-top: 0;'>
                        <li>When you resize browser visible datagrid grows to accomodate available space</li>
                        <li>Datagrid is a virtual table - despite houndreds of rows it only actively contains around two dozens of rows</li>
                        <li>Column filter - hover mouse over datagrid header to reveal 'blue burger' button, then clisk it to show column filter</li>
                        <li>Sortable - click on column header to change its sorting</li>
                    </ul></div>
                    <div><ul style='margin-top: 0;'>                  
                        <li>Filter all rows in all columns - use 'Search rows...' input in bottom right side of the screen</li>
                        <li>Export data to spreadsheet - click proper button in bottom right side of the screen</li>
                        <li>Reloadable/refreshable - click proper button in bottom right side of the screen</li>
                        <li>Selectable: click on the row to select one row OR ctrl click on row to toggle select of row</li>
                    </ul></div>
                </div>");
        
        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            //next two lines to eliminate space between Help and Items that confuses tbody height calculator
            parentContainer.Style.Display = Display.Flex;
            parentContainer.Style.FlexDirection = FlexDirection.Column; 

            return new RenderElem<HTMLElement>[] {Help,Items};
        }
    }
}
