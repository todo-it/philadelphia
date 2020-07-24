using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Tests.Client.Model;
using Philadelphia.Web;

namespace Philadelphia.Tests.Client.UI {
    public class TestRunnerForm : IBareForm<HTMLElement> {
        public string Title => "Test running";
        public IFormView<HTMLElement> View { get; }

        public TestRunnerForm(IReadOnlyCollection<TestModel> tests) {
            void RunAll()
            {
                foreach (var test in tests)
                {
                    test.Run();
                }
            }

            RunAll();
            var view = new TestRunnerView();

            BaseUnboundColumnBuilder<TestModel> Column(string x) => UnboundDataGridColumnBuilder.For<TestModel>(x);

            ValueContainingUnboundColumnBuilder<TestModel, string> TextColumn(string x, Func<TestModel, string> val) => 
                UnboundDataGridColumnBuilder.For<TestModel>(x).WithValue(val);

            var gridGuts = DataGridModel<TestModel>.CreateAndBindNonReloadable(
                view.Grid, 
                Toolkit.DefaultTableBodyHeightProvider(-50), 
                TextColumn("Name", x => x.Name)
                    .TransformableDefault()
                    .Build()
                    .With(x => x.MinimumWidth = 500),
                Column("Outcome").WithValueAsText(x => x.Outcome, x => x.ToString())
                    .TransformableAsText()
                    .Observes(x => nameof(x.Outcome))
                    .Build().With(x => x.MinimumWidth = 30),
                TextColumn("Log", x => x.Log)
                    .TransformableDefault()
                    .Observes(x => nameof(x.Log))
                    .Build().With(x => x.MinimumWidth = 400)
                );

            gridGuts.model.Items.Replace(tests);

            gridGuts.model.Selected.Changed += (insertedAt, inserted, removed) => {
                switch (inserted.FirstOrDefault())
                {
                    case null:
                        view.SummaryText = "";
                        break;
                    case var x:
                        view.SummaryText = x.Log;
                        break;
                }
            };

            View = view;
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }
} 
