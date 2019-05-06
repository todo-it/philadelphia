using Philadelphia.Common;
    using Philadelphia.Web;

    namespace Philadelphia.Demo.Client {
        public class WebClientSomeService : Philadelphia.Demo.SharedModel.ISomeService {
            public System.Func<Philadelphia.Demo.SharedModel.ContinentalNotification,System.Boolean>ContinentalListener(Philadelphia.Demo.SharedModel.ContinentalSubscriptionRequest p0){
                throw new System.Exception("SSE listener cannot be invoked this way");
            }
            public async System.Threading.Tasks.Task<Philadelphia.Demo.SharedModel.SomeDto>Create(Philadelphia.Demo.SharedModel.SomeDto p0){
                return await HttpRequester.RunHttpRequestReturningPlain<Philadelphia.Demo.SharedModel.SomeDto, Philadelphia.Demo.SharedModel.SomeDto>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "Create", p0);
            }
            public async System.Threading.Tasks.Task<Philadelphia.Common.FileModel>DataGridToSpreadsheet(Philadelphia.Common.DatagridContent p0){
                return await HttpRequester.RunHttpRequestReturningAttachment<Philadelphia.Common.DatagridContent>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "DataGridToSpreadsheet", p0);
            }
            public async System.Threading.Tasks.Task<Philadelphia.Demo.SharedModel.DetailDto[]>FetchDetails(){
                return await HttpRequester.RunHttpRequestReturningArray<Philadelphia.Demo.SharedModel.DetailDto>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "FetchDetails");
            }
            public async System.Threading.Tasks.Task<Philadelphia.Demo.SharedModel.HeaderDto[]>FetchHeaders(){
                return await HttpRequester.RunHttpRequestReturningArray<Philadelphia.Demo.SharedModel.HeaderDto>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "FetchHeaders");
            }
            public async System.Threading.Tasks.Task<Philadelphia.Demo.SharedModel.SomeDto[]>FetchItems(){
                return await HttpRequester.RunHttpRequestReturningArray<Philadelphia.Demo.SharedModel.SomeDto>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "FetchItems");
            }
            public async System.Threading.Tasks.Task<Philadelphia.Demo.SharedModel.SomeDto>Modify(System.Int32 p0, System.String p1, System.String p2){
                return await HttpRequester.RunHttpRequestReturningPlain<System.Int32, System.String, System.String, Philadelphia.Demo.SharedModel.SomeDto>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "Modify", p0, p1, p2);
            }
            public async System.Threading.Tasks.Task<Philadelphia.Common.RemoteFileId[]>OrderAttachmentGetFiles(){
                return await HttpRequester.RunHttpRequestReturningArray<Philadelphia.Common.RemoteFileId>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "OrderAttachmentGetFiles");
            }
            public async System.Threading.Tasks.Task<Philadelphia.Common.FileModel>OrderAttachmentGetter(Philadelphia.Common.RemoteFileId p0, System.Int32 p1, System.Boolean p2){
                return await HttpRequester.RunHttpRequestReturningAttachment<Philadelphia.Common.RemoteFileId, System.Int32, System.Boolean>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "OrderAttachmentGetter", p0, p1, p2);
            }
            public System.Threading.Tasks.Task<Philadelphia.Common.RemoteFileId[]>OrderAttachmentSetter(Philadelphia.Common.UploadInfo p0, System.Int32 p1, System.Boolean p2){
                throw new System.Exception("uploads cannot be called this way");
            }
            public async System.Threading.Tasks.Task<System.DateTime>PublishNotification(Philadelphia.Demo.SharedModel.Country p0){
                return await HttpRequester.RunHttpRequestReturningPlain<Philadelphia.Demo.SharedModel.Country, System.DateTime>(
                    typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                    "PublishNotification", p0);
            }
        }
    public class WebClientTranslationsService : Philadelphia.Demo.SharedModel.ITranslationsService {
            public async System.Threading.Tasks.Task<Philadelphia.Common.TranslationItem[]>FetchTranslation(Philadelphia.Demo.SharedModel.SupportedLang p0){
                return await HttpRequester.RunHttpRequestReturningArray<Philadelphia.Demo.SharedModel.SupportedLang, Philadelphia.Common.TranslationItem>(
                    typeof(Philadelphia.Demo.SharedModel.ITranslationsService).FullName,
                    "FetchTranslation", p0);
            }
        }

    public class ISomeService_OrderAttachment : Philadelphia.Web.BaseDownloadUploadHandler {
        public ISomeService_OrderAttachment(System.Func<System.Int32> p0,System.Func<System.Boolean> p1) 
             : base(
                () => Bridge.Html5.JSON.Stringify(System.Tuple.Create( p0(), p1())),
                typeof(Philadelphia.Demo.SharedModel.ISomeService).FullName,
                "OrderAttachmentGetter",
                "OrderAttachmentSetter",
                x => Bridge.Html5.JSON.Stringify(System.Tuple.Create(x, p0(), p1())) ) {}
    }

        public class ISomeService_ContinentalListener_SseSubscriber : ServerSentEventsSubscriber<Philadelphia.Demo.SharedModel.ContinentalNotification,Philadelphia.Demo.SharedModel.ContinentalSubscriptionRequest> {
        public ISomeService_ContinentalListener_SseSubscriber(Philadelphia.Demo.SharedModel.ContinentalSubscriptionRequest ctx, bool autoConnect=true)
            : base(autoConnect, typeof(Philadelphia.Demo.SharedModel.ISomeService), "ContinentalListener", ctx) {}
    }

        public class Services {
            public static void Register(IDiContainer container) {
                container.RegisterFactoryMethod<Philadelphia.Demo.SharedModel.ISomeService>(injector => new WebClientSomeService(), Philadelphia.Common.LifeStyle.Singleton);
                container.RegisterFactoryMethod<Philadelphia.Demo.SharedModel.ITranslationsService>(injector => new WebClientTranslationsService(), Philadelphia.Common.LifeStyle.Singleton);
            }
        }
    }
    