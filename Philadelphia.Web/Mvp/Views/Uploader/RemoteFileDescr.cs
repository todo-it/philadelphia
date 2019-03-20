using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public enum ShortenFileNamePolicy {
        Middle,
        End
    }

    public class RemoteFileDescr {
        public string FileId {get; set; }
        public string FileName {get; set; }
        
        public UploadStatus Status {get; set; }
        public string UploadErrorMessage {get; set; }
        public int TmpUploadJobId {get; set; }

        public (int width, int height)? FullDimensions {get; set; }
        public (int width, int height)? ThumbDimensions {get; set; }
        public string ThumbnailDataUrl {get; set;}
        
        public override string ToString() {
            return $"<RemoteFileDescr FileId={FileId} FileName={FileName} Status={Status}>";
        }

        public string GetNotTooLongFileName(int maxLen, ShortenFileNamePolicy pol) {
            if (FileName.Length <= maxLen) {
                return FileName;
            }

            switch (pol) {
                case ShortenFileNamePolicy.Middle:
                    return 
                        FileName.Substring(0, maxLen/2 - 3) + 
                        Magics.UnicodeHorizontalEllipsis + 
                        FileName.Substring(FileName.Length - maxLen/2 - 3);

                case ShortenFileNamePolicy.End:
                    return FileName.Substring(0, maxLen - 3) + Magics.UnicodeHorizontalEllipsis;

                default: throw new Exception("unsupported ShortenFileNamePolicy");
            }
        }

        public RemoteFileId AsRemoteFileIdAndName() {
            return RemoteFileId.CreateNonImage(FileId, FileName);
        }

        public RemoteFileId AsRemoteFileId() {
            return RemoteFileId.CreateRequest(FileId);
        }

        public RemoteFileDescr CloneIt() {
            return new RemoteFileDescr {
                FileId = FileId,
                FileName = FileName,
                Status = Status,
                UploadErrorMessage = UploadErrorMessage,
                TmpUploadJobId = TmpUploadJobId
            };
        }

        public static RemoteFileDescr CreateFromIdAndName(string id, string name) {
            return new RemoteFileDescr {
                FileId = id,
                FileName = name,
                Status = UploadStatus.Succeeded };
        }

        public static RemoteFileDescr CreateFrom(RemoteFileId inp) {
            return new RemoteFileDescr {
                FileId = inp.FileId,
                FileName = inp.FileName,
                Status = UploadStatus.Succeeded,
                ThumbnailDataUrl = inp.ThumbnailDataUrl,
                ThumbDimensions = inp.ThumbWidth > 0 && inp.ThumbHeight > 0? 
                        (inp.ThumbWidth, inp.ThumbHeight)
                    : 
                        ((int, int)?)null,
                FullDimensions = inp.FullWidth >0 && inp.FullHeight >0 ?
                        (inp.FullWidth, inp.FullHeight)
                    :
                        ((int, int)?)null
            };
        }
    }
}
