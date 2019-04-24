namespace Philadelphia.Testing.DotNetCore {
    public class CommandReply {
        public long? RequestId {get; set; }
        public ReplyType Type {get; set; }
        public string ReplyData {get; set; }
    }
}
