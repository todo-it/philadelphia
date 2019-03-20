using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface IUploadViewAction {
        IView<HTMLElement> Create(RemoteFileDescr forFile, Action forceAddOrRemoveToView);
        void Destroy(RemoteFileDescr forFile);

        /// <summary>caller(s) will only invoke it for first concurrent operation on forFile</summary>
        void OnNotifyOperationStart(RemoteFileDescr forFile, IView<HTMLElement> senderOrNull);

        /// <summary>caller(s) will only invoke it if it is the last concurrent operation on forFile.</summary>
        void OnNotifyOperationEnded(RemoteFileDescr forFile, IView<HTMLElement> senderOrNull);
    }
}
