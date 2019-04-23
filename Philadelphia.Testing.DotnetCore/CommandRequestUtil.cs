using System;

namespace Philadelphia.Testing.DotnetCore {
    public static class CommandRequestUtil {
        public static CommandRequest CreateStopServer() {
            return new CommandRequest {
                Id = DateTime.UtcNow.Ticks,
                Type = RequestType.StopServer };
        }
    }
}
