using System;
using System.Threading;

namespace HeavyTests.Tests {
    public class Poll {
        public static bool Wait(Func<bool> condition, int polls = 5, int milliseconds = 500) {
            var interval = milliseconds / polls;
            for (var i = 0; i < polls; i++) {
                if (condition()) {
                    return true;
                }

                Thread.Sleep(interval);
            }

            return false;
        }

        public static void WaitForSuccessOrFail(Func<bool> condition, int polls = 5, int milliseconds = 500) {
            if (!Wait(condition, polls, milliseconds)) {
                throw new Exception($"failed to satisfy condition within foreseen time frame {condition}");
            }
        }
    }
}
