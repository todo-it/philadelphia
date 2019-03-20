using System;

namespace Philadelphia.Common {
    public enum DownloadMethod {
        Attachment,
        Inline,
        ServerDefault
    }

    public class RemoteFileId {
        public string FileId { get; set; }
        public string FileName { get; set; }
        
        public DownloadMethod DwnMthd = DownloadMethod.ServerDefault;
        public string ThumbnailDataUrl { get; set; }
        
        //due to serialization in bridge 15.7 being poor it is in separate fields
        public int ThumbWidth { get; set; }
        public int ThumbHeight { get; set; }
        public int FullWidth { get; set; }
        public int FullHeight { get; set; }
        
        [Obsolete("for serialization only")]
        public RemoteFileId() { }

        public bool HasThumbnail() {
            return ThumbWidth > 0;
        }
        
        /// <summary>intended only when client requests something from server</summary>
        public static RemoteFileId CreateRequest(string fileId) {
            var result = new RemoteFileId();
            result.FileId = fileId;
            return result;
        }

        public static RemoteFileId CreateNonImage(
                string fullFileId, string fullFileName) {

            var result = new RemoteFileId();
            result.FileId = fullFileId;
            result.FileName = fullFileName;
            return result;
        }

        public static RemoteFileId CreateImage(
                string fullFileId, string fullFileName, Tuple<int,int> fullDimensions,
                byte[] thumbnailContent, string thumbnailFileName, Tuple<int,int> thumbnailDimensions) {

            var result = new RemoteFileId();
            result.FileId = fullFileId;
            result.FileName = fullFileName;
            
            result.FullWidth = fullDimensions.Item1;
            result.FullHeight = fullDimensions.Item2;

            result.ThumbWidth = thumbnailDimensions.Item1;
            result.ThumbHeight = thumbnailDimensions.Item2;

            result.ThumbnailDataUrl = EncodeThumbnailAsDataUrl(thumbnailFileName, thumbnailContent);
            return result;
        }

        private static string EncodeThumbnailAsDataUrl(string thumbFileName, byte[] thumbnailContent) {
            var fn = thumbFileName.ToLower();
            string format;

            if (fn.EndsWith(".gif")) {
                format = "gif";
            } else if (fn.EndsWith(".jpeg") || fn.EndsWith(".jpg")) {
                format = "jpeg";
            } else if (fn.EndsWith(".png")) {
                format = "png";
            } else {
                throw new Exception("unsupported image format");
            }

            return $"data:image/{format};base64," + Convert.ToBase64String(thumbnailContent);
        }
    }
}
