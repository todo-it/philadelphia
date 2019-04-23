namespace Philadelphia.Testing.DotnetCore {
    public static class FilterInvocationExtensions {
        public static CommandReply AsCommandReply(this FilterInvocation self, ICodec codec) {
            return new CommandReply {
                Type = ReplyType.FilterInvoked,
                ReplyData = codec.Encode(self)
            };
        }
    }
}
