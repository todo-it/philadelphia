using System;

namespace Philadelphia.Testing.DotNetCore {
    public static class CommandReplyExtensions {
        public static ServiceCall DecodeServiceCall(this CommandReply self, ICodec codec) {
            if (self.Type != ReplyType.ServiceInvoked) {
                throw new Exception("this is not ServiceInvoked reply");
            }

            return codec.Decode<ServiceCall>(self.ReplyData);
        }

        public static FilterInvocation DecodeFilterInvoked(this CommandReply self, ICodec codec) {
            if (self.Type != ReplyType.FilterInvoked) {
                throw new Exception("this is not FilterInvoked reply");
            }

            return codec.Decode<FilterInvocation>(self.ReplyData);
        }
    }
}
