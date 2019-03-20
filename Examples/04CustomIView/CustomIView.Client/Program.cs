using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace CustomIView.Client {
    //first lets define some nonobvious custom class that will be presented by IView
    public class Article {
        public bool IsBreakingNews {get; set;}
        public string Title {get; set;}
        public string Author {get; set;}
        public DateTime PublishedAt {get; set;}
        public string Body {get; set;}
    }

    //this is a custom IView that is used to render Article
    public class ArticleReadOnlyValueView : IReadOnlyValueView<HTMLElement,Article> {
        private readonly HTMLDivElement _widget 
            = new HTMLDivElement {ClassName = typeof(ArticleReadOnlyValueView).Name };
        private Article _lastValue;
        public event UiErrorsUpdated ErrorsChanged;
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_widget);
        public HTMLElement Widget => _widget;
        
        public Article Value {
            get => _lastValue;
            set {
                _lastValue = value;
                _widget.RemoveAllChildren();
                
                if (value == null) {
                    return; //normally you would never need this...
                }
                _widget.AddOrRemoveClass(value.IsBreakingNews, "isBreakingNews");
                _widget.AppendChild(new HTMLDivElement {
                    TextContent = $"{(value.IsBreakingNews ? "Breaking news: " : "")}{value.Title}" });

                _widget.AppendChild(new HTMLDivElement { 
                    TextContent = $@"by {value.Author} published at 
                        {I18n.Localize(value.PublishedAt, DateTimeFormat.YMDhm)}" });

                _widget.AppendChild(new HTMLDivElement {TextContent = value.Body});
            }
        }
        
        public void SetErrors(ISet<string> errors, bool causedByUser)  {
            _widget.SetAttribute(Magics.AttrDataErrorsTooltip, string.Join("\n", errors));
            _widget.Style.BackgroundColor = errors.Count <= 0 ? "" : "#ff0000";
            ErrorsChanged?.Invoke(this, errors);
        }
        
        //FormView->Render() is short thanks to this
        public static implicit operator RenderElem<HTMLElement>(ArticleReadOnlyValueView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
    
    //due to custom control following FormView is merely instantiating controls (and has almost no logic on its own)
    public class NewsReaderFormView : IFormView<HTMLElement> {
        public ArticleReadOnlyValueView NewsItem {get; } = new ArticleReadOnlyValueView();
        public InputTypeButtonActionView NextItem {get;} 
            = InputTypeButtonActionView.CreateFontAwesomeIconedButtonLabelless(Magics.FontAwesomeReloadData)
                .With(x => x.Widget.Title = "Fetch next newest news item");
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(NextItem);

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {NewsItem};
        }
    }
    
    //news reader form that informs outside world that user requests next news item or has enough news for today
    public class NewsReaderForm : IForm<HTMLElement,NewsReaderForm,NewsReaderForm.ReaderOutcome> {
        public enum ReaderOutcome {
            FetchNext,
            Cancelled
        }
        public string Title => "News reader";
        private readonly NewsReaderFormView _view = new NewsReaderFormView();
        public IFormView<HTMLElement> View => _view;
        public event Action<NewsReaderForm,ReaderOutcome> Ended;
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(
            () => Ended?.Invoke(this, ReaderOutcome.Cancelled));

        public NewsReaderForm() {
            LocalActionBuilder.Build(_view.NextItem, () => Ended?.Invoke(this, ReaderOutcome.FetchNext));
        }
        
        public void Init(Article itm) {
            _view.NewsItem.Value = itm;
        }
    }
    
    //helper extensions
    public static class ArrayExtensions {
        public static T RandomItem<T>(this T[] self) {
            var i = DateTime.Now.Second % self.Length;
            return self[i];
        }

        public static IEnumerable<T> PickSomeRandomItems<T>(this T[] self, int cnt) {
            var x = DateTime.Now.Second % self.Length;
            return Enumerable.Range(0, cnt).Select(i => self[(x+i) % self.Length]);
        }
    }
        
    //entry point
    public class Program {
        [Ready]
        public static void OnReady() {
            var di = new DiContainer();
            Services.Register(di); //registers discovered services from model
          
            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();
            
            var reader = new NewsReaderForm();
            reader.Init(GenerateNewsItem());

            reader.Ended += (x, outcome) => {
                switch (outcome) {
                    case NewsReaderForm.ReaderOutcome.Cancelled:
                        renderer.Remove(x);
                        break;

                    case NewsReaderForm.ReaderOutcome.FetchNext:
                        x.Init(GenerateNewsItem());
                        break;
                }
            };

            renderer.AddPopup(reader);
        }
        
        private static string[] _fnames = {"Anna", "John","Mike", "Paul", "Frank", "Mary", "Sue" };
        private static string[] _lnames = {"Doe", "Smith", "Tomatoe", "Potatoe", "Eggplant" };
        private static string[] _title1 = {"Cow", "Famous actor","Famous actress", "UFO", "Popular social platform", "Alien", "Cat", "President", "Dog", "Big company CEO", "Law abiding citizen", "Old man", "Young lady" };
        private static string[] _title2 = {"ate", "was run over by", "was eaten by", "was surprised by", "stumbled upon" };
        private static string[] _title3 = {"decent folks", "whole nation", "neighbour", "Internet", "MEP", "galaxy", "homework", "its fan", "conspiracy theorist crowd" };
        private static string[] _body = {"It was completely unexpected.", "It came as a shock to everybody in country", 
            "Whole nation is in shock.", "President calls for special powers to address unexpected situation.", 
            "Stock exchange is in panic.", "Majority of MDs call it pandemia.", "Will we ever be able to cope with such tragedy?",
            "Major social platforms provides special tools to help its users deal with a tragedy."};

        private static Article GenerateNewsItem() {
            return new Article {
                IsBreakingNews = DateTime.Now.Second % 2 == 0,
                PublishedAt = DateTime.Now,
                Author = $"{_fnames.RandomItem()} {_lnames.RandomItem()}",
                Title = $"{_title1.RandomItem()} {_title2.RandomItem()} {_title3.RandomItem()}",
                Body = string.Join("", _body.PickSomeRandomItems(4))
            };
        }
    }
}
