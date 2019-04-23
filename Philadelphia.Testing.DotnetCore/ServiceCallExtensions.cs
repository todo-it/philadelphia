namespace Philadelphia.Testing.DotnetCore {
    public static class ServiceCallExtensions {
        public static CommandReply AsCommandReply(this ServiceCall self, ICodec codec) {
            return new CommandReply {
                Type = ReplyType.ServiceInvoked,
                ReplyData = codec.Encode(self)
            };
        }
    }
}
