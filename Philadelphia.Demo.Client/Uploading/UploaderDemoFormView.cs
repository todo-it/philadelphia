using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class UploaderDemoFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(Confirm);
        public InputTypeButtonActionView Confirm {get; } = new InputTypeButtonActionView("Confirm");
        public readonly UploadView Attachments = new UploadView("Attachments");
        
        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                "<div class='grayedOut'>",
                    "Following uploader is capable to do quite rich validation client side.<br>",
                    "It can check for amount of files, their fileextensions without contacting server<br>",
                    "By setting UploadView->AllVisualActions property you can show or hide actions per item. <br>",
                    "You can provide your own custom action by passing implementation(s) of IUploadViewAction there.<br>",
                    "In order to simulate upload issue name your file so that it contains word 'fail' somewhere in the name<br>",
                    "<span style='font-weight: bold;text-decoration: underline;'>Demo notes:</span>",
                    "validation allows maximum 10 files. Uploading images doesn't actually store files or produce thumbnails<br>",
                    "to keep server stateless and minimize server side dependencies&complexity.<br>",
                "</div>",
                $"<div class='{Magics.CssClassTableLike}'>",
                Attachments,
                "</div>"
            };
        }
    }
}