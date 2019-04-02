namespace ControlledByTests.Api {
    public static class CommandReplyUtil {
        public static CommandReply CreateLog(string txt) {
            return new CommandReply {
                RequestId = null, 
                Type = ReplyType.Log,
                ReplyData = txt
            };
        }

        public static CommandReply CreateServerStarted() {
            return new CommandReply {
                RequestId = null,
                Type = ReplyType.ServerStarted
            };
        }
    }
}
