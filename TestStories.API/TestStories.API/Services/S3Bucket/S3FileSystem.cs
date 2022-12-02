using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using TestStories.API.Common;
using TestStories.API.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TestStories.API.S3Services
{
    /// <summary>
    ///
    /// </summary>
    public class S3FileSystem
    {
        #region Private Properties

        private static readonly RegionEndpoint Endpoint = RegionEndpoint.GetBySystemName(TestStories.Common.EnvironmentVariables.AwsRegion);

        private readonly int _expirationInDays;
        private readonly TransferUtility _utility;
        private readonly AmazonS3Client _client = new(Endpoint);
        private readonly S3TransferParameters _parameters;
        // TODO: Is this app settings requied here?
        readonly S3Settings _appSettings;


        readonly Dictionary<string, string> _contentTypes = new Dictionary<string, string>
                {
                    {".wmv","video/x-ms-wmv"},
                    {".m4v","video/mp4"},
                    {".json","application/json"},
                    {".jpg","image/jpeg"},
                    {".jpeg","image/jpeg"}
                };

        #endregion

        #region Constructor

        /// <summary> The class constructor. </summary>
        ///
        public S3FileSystem(S3TransferParameters parameters, bool isMultiPart, int expirationInDays, S3Settings AppSettings)
        {
            _parameters = parameters;
            _expirationInDays = expirationInDays;

            _utility = isMultiPart
                ? new TransferUtility(_client,
                    new TransferUtilityConfig
                    {
                        MinSizeBeforePartUpload = _parameters.MinSizeBeforePartUpload * 1024 * 1024,
                        ConcurrentServiceRequests = _parameters.ConcurrentServiceRequests
                    })
                : new TransferUtility(_client);
        }

        /// <inheritdoc />
        public S3FileSystem(S3Settings appSettings)
        {
            _appSettings = appSettings;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Upload file from local system to AWS S3 bucket</summary>
        /// <param name="fromPath"> Full path to a local file</param>
        /// <param name="bucketName"> AWS S3 bucket name</param>
        /// <param name="mediaFolder"> Folder associated with different type of media (V01,V02,P01,..)</param>
        /// <param name="fileFolder"> Application generated folder structure (different in ZAP and FC)</param>
        /// <param name="fileName"> File to upload</param>
        /// <param name="isMutiPart"> Defines if upload should try to use multi-part transfer</param>
        ///
        public bool UploadFile(string fromPath, string bucketName, string mediaFolder, string fileFolder, string fileName, bool isMutiPart = false)
        {
            if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(bucketName))
            {
                return false;
            }

            try
            {
                var key = mediaFolder + "/" + fileFolder + fileName;

                if (_utility != null)
                {
                    var contentType = "";

                    // get last extension as a determinator of MIME type
                    // for example case test.wmv.json should give json
                    var pos = fileName.LastIndexOf(".", StringComparison.Ordinal);
                    var extension = fileName[pos..];
                    foreach (var c in _contentTypes.Where(c => extension == c.Key))
                    {
                        contentType = c.Value;
                        break;
                    }

                    var request = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        FilePath = fromPath,
                        Key = key,
                        ContentType = contentType,
                        AutoCloseStream = true
                    };
                    request.UploadProgressEvent += Request_UploadProgressEvent;

                    if (isMutiPart)
                        request.PartSize = _parameters.PartSize * 1024 * 1024;

                    _utility.Upload(request);
                    return true;
                }
            }
            catch (AmazonS3Exception)
            {
                return false;
            }

            return false;
        }

        private void Request_UploadProgressEvent(object sender, UploadProgressArgs e)
        {
            Console.WriteLine(e.PercentDone);
        }

        #endregion

        #region Upload Media using Signed URL

        /// <summary>
        /// TODO: Why there is a need to generate presigned URL and make an upload after in same method?
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<bool> UploadFileToS3(string filePath, int sessionId)
        {
            var fileName = S3Utility.GetFileName(filePath);
            using (_client)
            {
                var path = GeneratePreSignedUploadUrl(_client, sessionId, fileName);

                var httpRequest = WebRequest.Create(path) as HttpWebRequest;
                httpRequest.Method = "PUT";
                using (var dataStream = httpRequest.GetRequestStream())
                {
                    var buffer = new byte[10000];
                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        dataStream.Write(buffer, 0, bytesRead);
                    }
                }
                var response = httpRequest.GetResponse() as HttpWebResponse;

               return true; //indicate that the file was sent
            }
        }

        private string GeneratePreSignedUploadUrl(IAmazonS3 client, int sessionId, string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _appSettings.S3MediaBucketName + "/" + sessionId,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.Now.AddMinutes(Convert.ToDouble(_appSettings.S3MediaQueueLinkExpirationInMinutes))
            };

            return client.GetPreSignedURL(request);
        }

        private void CreateFoldersInS3(int sessionId)
        {
            var bucketName = _appSettings.S3MediaBucketName;
            using var client = new AmazonS3Client(Endpoint);
            var putReq = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = sessionId + "/",
                ContentType = "binary/octet-stream"
            };
            client.PutObjectAsync(putReq);

            var qualityDirs = new List<string> { "hires", "lores", "medres" };
            // Check if e need to create quality directories
            qualityDirs.ForEach(dir =>
            {
                putReq = new PutObjectRequest
                {
                    BucketName = bucketName + "/" + sessionId,
                    Key = dir + "/",
                    ContentType = "binary/octet-stream"
                };
                client.PutObjectAsync(putReq);
            });
        }

        #endregion Upload Media using Signed URL
    }

    /// <summary>
    ///
    /// </summary>
    public class S3TransferParameters
    {
        /// <summary>
        ///
        /// </summary>
        public int ConcurrentServiceRequests;
        /// <summary>
        ///
        /// </summary>
        public int MinSizeBeforePartUpload;
        /// <summary>
        ///
        /// </summary>
        public int PartSize;
        /// <summary>
        ///
        /// </summary>
        public int MaximumNumberOfRetries;
        /// <summary>
        ///
        /// </summary>
        public int TimeBetweenRetriesMs;
    }
}
