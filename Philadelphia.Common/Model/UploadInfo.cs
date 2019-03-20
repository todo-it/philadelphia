namespace Philadelphia.Common {
    public class UploadInfo {
        public FileModel[] Files { get; set; }
        public FileUploadOperation OperationType { get; set; }
        public RemoteFileId ToReplaceOrRemoveId { get; set; }
    }

}
