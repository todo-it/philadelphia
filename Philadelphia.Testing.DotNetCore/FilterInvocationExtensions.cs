namespace Philadelphia.Testing.DotNetCore {
    public static class FilterInvocationExtensions {
        public static CommandReply AsCommandReply(this FilterInvocation self, ICodec codec) {
            return new CommandReply {
                Type = ReplyType.FilterInvoked,
                ReplyData = codec.Encode(self)
            };
        }
    }
}
