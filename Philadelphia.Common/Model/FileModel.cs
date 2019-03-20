using System;

namespace Philadelphia.Common {
    /// use static Create* methods to create instances. Constructor is public to meet Serializable contract
    public class FileModel {
        private bool? ForcedIsAttachment = null;
        public bool IsAttachment => ForcedIsAttachment ?? !FileName.ToLower().EndsWith(".pdf");
        public string FileName { get;  private set; }
        private Func<byte[]> ContentGetter { get; set; }

        private byte[] _content;
        public byte[] Content {
            get {
                if (ContentGetter != null && _content == null) {
                    _content = ContentGetter();
                }
                return _content;
            }
            private set { _content = value; }
        }

        public string MimeType { get;  private set; }
        public string PostUrl {get; private set; }
        public Tuple<string,string> UrlParam {get; private set; }

        /// for serialization only. Don't use it directly
        public FileModel() {}
        
        public static FileModel CreateUpload(string fileName, string mimeType, Func<byte[]> contentProvider) {
            return new FileModel {
                FileName = fileName,
                MimeType = mimeType,
                ContentGetter = contentProvider };
        }

        public static FileModel CreateLocal(string fileName, byte[] content) {
            var mimeType = MimeUtil.FromFileName(fileName);
            return CreateLocal(fileName, mimeType, content);
        }

        public static FileModel CreateLocal(string fileName, string mimeType, byte[] content, bool? forcedIsAttachment = null) {
            var result = new FileModel {
                FileName = fileName,
                MimeType = mimeType,
                Content = content,
                ForcedIsAttachment = forcedIsAttachment };

            return result;
        }
        
        public static FileModel CreateDownloadRequest(string postUrl, Tuple<string,string> param) {
            var result = new FileModel {
                PostUrl = postUrl,
                UrlParam = param };

            return result;
        }
    }
}
