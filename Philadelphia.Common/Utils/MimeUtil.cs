namespace Philadelphia.Common {
    public static class MimeUtil {
        public static string FromFileName(string fileName) {
            fileName = fileName.ToLower();
            var ext = fileName.Substring(fileName.LastIndexOf(".")+1);

            if (ext == "xls") {
                return "application/vnd.ms-excel";
            }

            if (ext == "xlsx") {
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            
            if (ext == "doc") {
                return "application/msword";
            }
            
            if (ext == "docx") {
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            if (ext == "pdf") {
                return "application/pdf";
            }

            if (ext == "png") {
                return "image/png";
            }

            if (ext == "jpg") {
                return "image/jpeg";
            }
            
            if (ext == "bmp") {
                return "image/bmp";
            }

            if (ext == "doc") {
                return "image/bmp";
            }
            
            return "application/octet-stream";
        }
    }
}
