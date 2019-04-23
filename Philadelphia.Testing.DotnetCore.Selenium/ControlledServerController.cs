using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Philadelphia.Testing.DotnetCore.Selenium {
    public class ControlledServerController : IDisposable {
        private readonly Action<string> _logger;
        private readonly ICodec _codec;
        private readonly Process _proc;
        private readonly TimeSpan _defaultReadTimeout;
        private object _readLck = new object();
        private object _writeLck = new object();
        private List<string> _readLines = new List<string>();
        private readonly AutoResetEvent _lineReceived = new AutoResetEvent(false);

        public ControlledServerController(
                Action<string> logger, ICodec codec, Process proc, TimeSpan defaultReadTimeout) {

            _logger = logger;
            _codec = codec;
            _proc = proc;
            _defaultReadTimeout = defaultReadTimeout;

            _proc.Exited += (sender, args) => _logger("process ended");

            _proc.ErrorDataReceived += (_, args) => _logger("received error: " + args.Data);
            

            _proc.OutputDataReceived += (_, args) => {
                lock (_readLck) {
                    _readLines.Add(args.Data);
                }
                _logger("received line: "+args.Data);
                _lineReceived.Set();
            }; 

            _logger($"running command[{proc.StartInfo.FileName}] with args[{proc.StartInfo.Arguments}] in workdir[{proc.StartInfo.WorkingDirectory}]");

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }

        public void WriteRequest(CommandRequest req) {
            WriteLine(_codec.Encode(req));
        }

        public void WriteLine(string line) {
            _logger("sending line: "+line);
            WriteLineImpl(line);
        }

        private void WriteLineImpl(string line) {
            lock (_writeLck) {
                _proc.StandardInput.WriteLine(line);
                _proc.StandardInput.Flush();    
            }
            _logger("sent line: "+line);
        }

        /// <summary>returns null if timeout was reached</summary>
        public string ReadLineOrNull(TimeSpan? timeout = null) {
            var result = ReadLineOrNullImpl(timeout ?? _defaultReadTimeout);
            _logger($"received: {result}");
            return result;
        }

        /// <summary>returns null if timeout was reached</summary>
        public CommandReply ReadReplyOrNull(TimeSpan? timeout = null) {
            var rawReply = ReadLineOrNull(timeout);
            if (rawReply == null) {
                return null;
            }
            return _codec.Decode<CommandReply>(rawReply);
        }
        
        public List<string> ReadAllPendingLines() {
            lock(_readLck) {
                var cpy = _readLines.ToList();
                _readLines.Clear();
                return cpy;
            }
        }

        public List<CommandReply> ReadAllPendingReplies() {
            return ReadAllPendingLines()
                .Select(_codec.Decode<CommandReply>)
                .ToList();
        }

        public CommandReply WriteRequestAndReadReplyOrFail(CommandRequest req, TimeSpan? timeout = null) {
            var startedAt = DateTime.UtcNow;
            var endedAt = DateTime.UtcNow.Add(timeout ?? _defaultReadTimeout);
            
            if (endedAt <= startedAt) {
                throw new Exception("timeout must be positive");
            }

            WriteRequest(req);

            while (DateTime.UtcNow <= endedAt) {
                var reply = ReadReplyOrNull(timeout);
                if (reply == null) {
                    continue;
                }

                if (reply.RequestId == req.Id) {
                    return reply;
                }
            }

            throw new Exception("didn't receive reply to command within expected timeframe");
        }
        
        private string ReadLineOrNullImpl(TimeSpan? timeout = null) {
            var startedAt = DateTime.UtcNow;
            var endedAt = DateTime.UtcNow.Add(timeout ?? _defaultReadTimeout);
            
            if (endedAt <= startedAt) {
                throw new Exception("timeout must be positive");
            }

            lock(_readLck) {
                if (_readLines.Count > 0) {
                    var oldest = _readLines[0];
                    _readLines.RemoveAt(0);
                    return oldest;
                }
            }

            var hasLine = _lineReceived.WaitOne(timeout ?? _defaultReadTimeout);

            if (!hasLine) {                
                if (_proc.HasExited) {
                    throw new Exception($"process exited so cannot read from its stdout");
                }

                return null; //timeout reached and still no line received
            }
            
            //now possible race condition
            lock(_readLck) {
                if (_readLines.Count <= 0) {
                    return null; //normally impossible as no other threads should read process
                }

                var oldest = _readLines[0];
                _readLines.RemoveAt(0);
                return oldest;
            }
        }

        public void Dispose() {
            _logger("disposing process");

            using (_proc) { 
                if (_proc.HasExited) {
                    throw new Exception($"process ended unexpectedly");
                }

                try {
                    _proc.Kill();
                    _logger("process killed");
                }
                catch (Exception ex) {
                    _logger($"killing process failed due to {ex}");
                }
            }
        }
    }
}
