using System;

namespace ControlledByTests.Api {
    public static class CommandRequestUtil {
        public static CommandRequest CreateStopServer() {
            return new CommandRequest {
                Id = DateTime.UtcNow.Ticks,
                Type = RequestType.StopServer };
        }
    }
}
