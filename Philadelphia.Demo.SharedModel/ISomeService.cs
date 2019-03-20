using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Demo.SharedModel {
    [HttpService]
    public interface ISomeService {
        Task<SomeDto[]> FetchItems();
        Task<SomeDto> Modify(int itemId, string propertyName, string propertyValueAsJson);
        Task<SomeDto> Create(SomeDto newItem);
        Task<FileModel> DataGridToSpreadsheet(DatagridContent inp);
        Task<HeaderDto[]> FetchHeaders();
        Task<DetailDto[]> FetchDetails();

        Task<RemoteFileId[]> OrderAttachmentGetFiles();

        /// <summary>orderId and someCtxVar are given to show that 'context' may be passed to setter</summary>
        Task<FileModel> OrderAttachmentGetter(RemoteFileId fileIdentifier, int orderId, bool irrelevant);

        /// <summary>orderId and someCtxVar are given to show that 'context' may be passed to setter</summary>
        Task<RemoteFileId[]> OrderAttachmentSetter(UploadInfo info, int orderId, bool someCtxVar);

        Task<DateTime> PublishNotification(Country inp);
        Func<ContinentalNotification,bool> ContinentalListener(ContinentalSubscriptionRequest inp);
    }
}
