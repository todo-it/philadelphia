using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class FormCanvasExtensions {
        private static void Debug(string msg) => Logger.Debug(typeof(FormCanvasExtensions), msg);

        private static readonly WeakDictionary<IFormCanvas<HTMLElement>,ExternalEventsHandlers> Handlers
            = new WeakDictionary<IFormCanvas<HTMLElement>, ExternalEventsHandlers>();

        // REVIEW: named ValueTuple would be more readable
        private static Tuple<string, Dictionary<string, HTMLElement>> BuildTemplateAndParams(
                RenderElem<HTMLElement>[] tokens) {

            var innerHtmlTemplate = "";
            var controls = new Dictionary<string, HTMLElement>();

            foreach (var t in tokens) {
                if (t.Iview != null) {
                    var id = UniqueIdGenerator.Generate();
                    innerHtmlTemplate += string.Format("<div id='{0}'></div>", id);
                    controls.Add(id.ToString(), t.Iview.Widget);
                    Logger.Debug(typeof(FormCanvasExtensions),"BuildTemplateAndParams iview={0} with id={1}", t, id);                    
                    continue;
                } 
                
                if (t.NativeItm != null) {
                    var id = UniqueIdGenerator.Generate();
                    innerHtmlTemplate += string.Format("<div id='{0}'></div>", id);
                    controls.Add(id.ToString(), t.NativeItm);
                    Logger.Debug(typeof(FormCanvasExtensions),"BuildTemplateAndParams NativeItm={0} with id={1}", t, id);                    
                    continue;
                }

                innerHtmlTemplate += t.Token;
                Logger.Debug(typeof(FormCanvasExtensions),"BuildTemplateAndParams literal={0}", t);
            }

            return new Tuple<string, Dictionary<string, HTMLElement>>(innerHtmlTemplate, controls);
        }

        private static HTMLElement BuildBody(IFormView<HTMLElement> newMaster) {
            var result = new HTMLDivElement();
            var templateWithParams = BuildTemplateAndParams(newMaster.Render(result));

            Logger.Debug(typeof(FormCanvasExtensions),"got template={0} and params={1}", templateWithParams.Item1, templateWithParams.Item2.Values.PrettyToString());

            result.InnerHTML = templateWithParams.Item1;
            
            foreach (var idToView in templateWithParams.Item2) {
                var id = idToView.Key + "";
                var toBeReplaced = result.FindContainedElementByIdOrNull(id);
                var replaceTo = idToView.Value;
                Logger.Debug(typeof(FormCanvasExtensions),"ReplaceWithMaster will replace {0} having id={1} with iview {2}", toBeReplaced.InnerHTML, id, replaceTo.InnerHTML);

                toBeReplaced.ParentNode.ReplaceChild(
                    replaceTo,
                    toBeReplaced );

                Logger.Debug(typeof(FormCanvasExtensions),"ReplaceWithMaster replaced and container is now {0}", result.InnerHTML);
            }

            Logger.Debug(typeof(FormCanvasExtensions),"ReplaceWithMaster all replaced");
            return result;
        }

        public static void Unrender(this IFormCanvas<HTMLElement> self) {
            Logger.Debug(typeof(FormCanvasExtensions),"Hiding form in canvas - unbinding user cancel");
            var handlers = Handlers.Get(self);
            self.Hide();
            
            if (handlers.OnUserCancel != null) {
                self.UserCancel = handlers.OnUserCancel;
            }
            
            Handlers.Delete(self);
        }

        public static void Render(this IFormCanvas<HTMLElement> self, IView<HTMLElement> form) {
            Logger.Debug(typeof(FormCanvasExtensions), "Replacing view in canvas");
            
            self.Body = form.Widget;
            self.Title = "";
            self.UserCancel = null;
            self.Actions = new List<HTMLElement>();
            self.Show();
            Logger.Debug(typeof(FormCanvasExtensions), "Replaced view in canvas");
        }

        public static void Render(this IFormCanvas<HTMLElement> self, IBareForm<HTMLElement> form, Action beforeShow) {
            Debug("Replacing form in canvas - building new form");
            var handlers = form.ExternalEventsHandlers;
            Debug($"Form details: title={form.Title}, type={form.GetType().FullName}, external handlers={handlers}");
            self.UserCancel = handlers.OnUserCancel;
            self.Body = BuildBody(form.View);
            self.Actions = form.View.Actions.Select(x => x.Widget);
            beforeShow();

            Handlers.Set(self, handlers);

            self.Show();
            self.Body.FindAndFocusOnFirstItem();
            Logger.Debug(typeof(FormCanvasExtensions), "Replacing form in canvas - new form added");
        }
    }
}
