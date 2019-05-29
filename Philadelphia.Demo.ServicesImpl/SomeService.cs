using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using System.IO.Compression;
using System.Reflection;
using Philadelphia.Server.Common;

namespace Philadelphia.Demo.ServicesImpl {
    public static class RandomExtensions {
        public static T Next<T>(this Random self, T[] inp) {
            return inp[self.Next(inp.Length-1)];
        }
    }

    public class RawFile {
        public string RawFileName {get; set;}
        public string UserFileNameOrNull {get; set;}
        public (int width, int height)? FullDim {get; set;}
        public (int width, int height)? ThumbDim {get; set;}
        public string MimeType { get { 
                var fileExt = RawFileName.Substring(1+RawFileName.LastIndexOf(".")).ToLower();
                if (fileExt == "jpg" || fileExt == "jpeg") {
                    return "image/jpeg";
                }
                if (fileExt == "pdf") {
                    return "application/pdf";
                }
                throw new Exception($"unsupported extension {fileExt}");
            }
        }
    }
    
    public class HardcodedLocalFileSystemStorage {
        private readonly string _fullFilesDir;
        private readonly string _thumbFilesDir;

        //hardcoded to minimize dependencies (no image processing libs)
        private static List<RawFile> _files = new List<RawFile> {
            new RawFile {
                RawFileName = "DSC02168.jpg",
                UserFileNameOrNull = "Very long file name for this nature photo.jpg",
                FullDim = (1980,1320),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02171.jpg",
                FullDim = (1920,1280),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02225.jpg",
                FullDim = (1200,800),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02227.jpg",
                FullDim = (1024,683),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02261.jpg",
                FullDim = (1900,1267),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02262.jpg",
                FullDim = (1200,800),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02284.jpg",
                FullDim = (1024,683),
                ThumbDim = (160,107)},
            new RawFile {
                RawFileName = "DSC02287.jpg",
                FullDim = (1200,800),
                ThumbDim = (160,107)},
            new RawFile {RawFileName = "test_document.pdf"} };

        public HardcodedLocalFileSystemStorage(string fullFilesDir, string thumbFilesDir) {
            _fullFilesDir = fullFilesDir;
            _thumbFilesDir = thumbFilesDir;
        }
        
        public FileModel GetAsFileModel(int fileId, DownloadMethod mth) {
            var metaData = _files[fileId];

            bool? forcedIsAttachment = null;
            switch (mth) {
                    case DownloadMethod.ServerDefault: 
                        forcedIsAttachment = null;
                        break;

                    case DownloadMethod.Attachment:
                        forcedIsAttachment = true;
                        break;

                    case DownloadMethod.Inline:
                        forcedIsAttachment = false;
                        break;
            }

            return FileModel.CreateLocal(
                metaData.UserFileNameOrNull ?? metaData.RawFileName,
                metaData.MimeType,
                File.ReadAllBytes(Path.Combine(_fullFilesDir, metaData.RawFileName)), 
                forcedIsAttachment);
        }
        
        public List<RemoteFileId> GetAsRemoteFileIds() {
            return _files
                .SelectI((i,x) => {
                    var fileName = x.UserFileNameOrNull ?? x.RawFileName;
                    
                    return (!x.FullDim.HasValue || !x.ThumbDim.HasValue) ? 
                            RemoteFileId.CreateNonImage(i.ToString(), fileName)
                        :
                            RemoteFileId.CreateImage(
                                i.ToString(), 
                                fileName,    
                                Tuple.Create(x.FullDim.Value.width,x.FullDim.Value.height),
                                File.ReadAllBytes(Path.Combine(_thumbFilesDir, x.RawFileName)) ,
                                fileName,
                                Tuple.Create(x.ThumbDim.Value.width,x.ThumbDim.Value.height));
                })
                .ToList();
        }
    }

    public class SomeService : ISomeService {
        private static int fakeFileId = -1;
        private static readonly Random _rand = new Random();
        private static readonly object _dbLock = new object();
        private static readonly string[] FirstNames = {"Mike", "John", "Frank", "Donald", "Anna", "Peter", "Bo", "Jack", "Bruce", "Brian", "Brenda", "Niels", "Arnold"};
        private static readonly string[] LastNames = {"Smith", "Doe", "Kovalsky", "Lee", "Tesla", "Einstein", "Planck", "Heisenberg", "Bohr", "Hubble", "Schwarzenegger"};
        private static readonly List<SomeDto> _someDtos = new List<SomeDto>()
            .With(x => {
                var traitTypes = EnumExtensions.GetEnumValues<SomeTraitType>().ToList();
                    
                x.AddRange(Enumerable.Range(1, 10000).Select(y =>
                    new SomeDto {
                        Id = y,
                        SomeNumber = (int)traitTypes[_rand.Next(traitTypes.Count)],
                        SomeText = "some test row no " + y,
                        SomeBool = _rand.Next() % 2 > 0,
                        SomeTrait = traitTypes[_rand.Next(traitTypes.Count)]
                    }
                ));
            });
        private static readonly List<HeaderDto> _headers = new List<HeaderDto>()
            .With(x => x.AddRange(Enumerable.Range(1, 100).Select(y => 
                new HeaderDto {Id = y, Name = $"{_rand.Next(FirstNames)} {_rand.Next(LastNames)}" } 
            )));
        private static readonly List<DetailDto> _details = new List<DetailDto>()
            .With(x => 
                _headers.ForEach(h => 
                    x.AddRange(
                        Enumerable.Range(1, _rand.Next(5, 30))
                            .Select(y => 
                                new DetailDto {ParentId = h.Id, Id = y, Name = h.Name+" child nr "+y}) )));
        private readonly Subscription<ContinentalNotification, ContinentalSubscriptionRequest> _subs;
        private readonly ClientConnectionInfo _client;
        private readonly DemoConfig _cfg;
        private readonly HardcodedLocalFileSystemStorage _storage;

        public SomeService(
                Subscription<ContinentalNotification,ContinentalSubscriptionRequest> subs, 
                ClientConnectionInfo client,
                DemoConfig cfg) {

            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _storage = new HardcodedLocalFileSystemStorage(
                Path.Combine(rootDir, "../../../../Philadelphia.Demo.Client/ImagesForUploadDemo/Full"),
                Path.Combine(rootDir, "../../../../Philadelphia.Demo.Client/ImagesForUploadDemo/Thumb"));
            _subs = subs;
            _client = client;
            _cfg = cfg;
        }
        
        public Task<DateTime> PublishNotification(string sseSessionId, Country inp) {
            var res = new ContinentalNotification {
                Country = inp, 
                SentAt = DateTime.Now, 
                Sender = _client.ClientIpAddress.Substring(0, _client.ClientIpAddress.LastIndexOf("."))+".*",
                SenderSseStreamId = sseSessionId
            };
            _subs.SendMessage(res);

            return Task.FromResult(DateTime.Now);
        }
        
        public Func<ContinentalNotification,bool> ContinentalListener(ContinentalSubscriptionRequest inp) {
            //there is necessary data to implement feature:
            //   do not send message to the user session (=browser tab) that caused event to be generated
            inp.SseStreamId = _client.StreamId;

            bool WhenTrueThenMayForward(ContinentalNotification x) {
                //here, there could be additional check to implement feature described above
                Logger.Debug(GetType(), "checking whether to send message caused by {0} to listener {1}", 
                    x.SenderSseStreamId, inp.SseStreamId);

                return x.Country.GetContinent() == inp.Continent;
            }
            return inp.Continent == Continent.Antarctica ? null : (Func<ContinentalNotification,bool>) WhenTrueThenMayForward;
        }

        public Task<SomeDto[]> FetchItems() {
            lock (_dbLock) {
                return Task.FromResult(_someDtos.ToArray());    
            }
        }

        public Task<SomeDto> Modify(int itemId, string propertyName, string propertyValueAsJson) {
            lock (_dbLock) {
                var itm = _someDtos.FirstOrDefault(x => x.Id == itemId);
                if (itm == null) {
                    throw new Exception("no such item");
                }

                if (!_cfg.ActuallyMutateDataServerSide) {
                    itm = new SomeDto {
                        Id = itm.Id,
                        SomeBool = itm.SomeBool,
                        SomeNumber = itm.SomeNumber,
                        SomeText = itm.SomeText,
                        SomeTrait = itm.SomeTrait
                    };
                }
                
                switch (propertyName) {
                    case "SomeText":
                        itm.SomeText = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(propertyValueAsJson);
                        break;

                    case "SomeNumber":
                        itm.SomeNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(propertyValueAsJson);
                        break;

                    case "SomeBool":
                        itm.SomeBool = Newtonsoft.Json.JsonConvert.DeserializeObject<bool>(propertyValueAsJson);
                        break;

                    case "SomeTrait":
                        itm.SomeTrait = Newtonsoft.Json.JsonConvert.DeserializeObject<SomeTraitType>(propertyValueAsJson);
                        break;

                    default: throw new Exception("unknown property");
                }
                return Task.FromResult(itm);
            }
        }

        public Task<SomeDto> Create(SomeDto newItem) {
            lock (_dbLock) {
                newItem.Id = _someDtos.Max(x => x.Id)+1;
                
                if (_cfg.ActuallyMutateDataServerSide) {
                    _someDtos.Add(newItem);

                }
            }
            return Task.FromResult(newItem);
        }

        public Task<FileModel> DataGridToSpreadsheet(DatagridContent inp) {
            var dllsPath = Path.GetDirectoryName(inp.GetType().Assembly.Location);
            
            Func<string,string> pathTo = x => Path.Combine(dllsPath, x);
            
            var template = new UTF8Encoding(false).GetBytes(
                File.ReadAllText(pathTo("xl_sharedStrings.xml"))
                    .Replace("ROW-COUNT-PLACEHOLDER", inp.rows.Length.ToString())
                    .Replace("COLUMN-COUNT-PLACEHOLDER", inp.labels.Length.ToString()));
            
            using (var xmlMs = new MemoryStream()) {
                xmlMs.Write(template, 0, template.Length);
                xmlMs.Seek(0, SeekOrigin.Begin);

                using (var xlsxMs = new MemoryStream()) {
                    using (var inps = File.OpenRead(pathTo("template.xlsx"))) {
                        inps.CopyTo(xlsxMs);

                        using (var archive = new ZipArchive(xlsxMs, ZipArchiveMode.Update, true)) {
                            var entry = archive.CreateEntry("xl/sharedStrings.xml");

                            using (var outp = entry.Open()) {
                                outp.Write(template);
                            }
                        }

                        return Task.FromResult(FileModel.CreateLocal(
                            "result.xlsx", 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            xlsxMs.ToArray()));
                    }
                }
            }
        }

        public Task<HeaderDto[]> FetchHeaders() {
            lock(_dbLock) {
                return Task.FromResult(_headers.ToArray());
            }
        }

        public Task<DetailDto[]> FetchDetails() {
            lock(_dbLock) {
                return Task.FromResult(_details.ToArray());
            }
        }

        public Task<FileModel> OrderAttachmentGetter(RemoteFileId ctx, int orderId, bool irrelevant) {
            var fileId = Convert.ToInt32(ctx.FileId);
            if (fileId < 0) {
                return Task.FromResult(FileModel.CreateLocal("readme.txt", Encoding.UTF8.GetBytes(
                    $"Here would be content of the file {ctx.FileName} that you requested bound to context: orderId={orderId} irrelevant={irrelevant}")));
            }

            return Task.FromResult(_storage.GetAsFileModel(Convert.ToInt32(ctx.FileId), ctx.DwnMthd));            
        }

        public Task<RemoteFileId[]> OrderAttachmentSetter(UploadInfo ctx, int orderId, bool irrelevant) {
            Logger.Debug(GetType(), "upload handler for context orderId={0} irrelevant={1}", orderId, irrelevant);
            Thread.Sleep(2000);

            if (ctx.Files.Any()) {
                if (ctx.Files.First().FileName.Contains("fail")) {
                    throw new Exception("some uploadproblem");
                }

                return Task.FromResult(ctx.Files
                    .Select(x => RemoteFileId.CreateNonImage((--fakeFileId).ToString(), x.FileName))
                    .ToArray() );
            }

            if (ctx.ToReplaceOrRemoveId.FileName.Contains("fail")) {
                throw new Exception("some removereplace problem");
            }
            
            return Task.FromResult(new [] { RemoteFileId.CreateNonImage("fake file id", "blah blah blah.pdf") });
        }

        public Task<RemoteFileId[]> OrderAttachmentGetFiles() {
            return Task.FromResult(_storage.GetAsRemoteFileIds().ToArray());
        }
    }
}
