using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveApi
{
    public class GoogleDriveStats
    {
        UserCredential Credential { get; }
        public GoogleDriveStats(string pathToCredential)
        {
            Credential = AuthorizeCredential(pathToCredential);
        }
        private UserCredential AuthorizeCredential(string credentialsPath)
        {
            UserCredential credential;
            if (!string.IsNullOrEmpty(credentialsPath))
            {
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))   // Load client secrets.So that get credentials need regitstrate them in https://console.cloud.google.com/apis/credentials
                {
                    /* The file token.json stores the user's access and refresh tokens, and is created
                     automatically when the authorization flow completes for the first time. */
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { DriveService.Scope.Drive },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                }
                if (credential == null)
                {
                    Console.WriteLine("Incorrect path to credential");
                    return null;
                }
                else
                    return credential;
            }
            else
                return null;

        }
        /// <summary>
        /// Download file from google drive by fileId
        /// </summary>
        /// <param name="pathToDirectory">Path where must be file after download</param>
        /// <param name="fileID">Id of file in google drive</param>
        /// <returns>File id</returns>
        public string DownloadFile(string pathToDirectory, string fileID)
        {
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Drive API .NET"
                });

                FilesResource.ListRequest requestName = service.Files.List();
                requestName.Q = "trashed = false";
                var result = requestName.Execute();
                string name = "newItem.txt";
                foreach (var item in result.Files)
                {
                    if(item.Id == fileID)   
                        name = item.Name;   
                }
                FilesResource.GetRequest listRequest = service.Files.Get(fileID);// Define parameters of request.
                using (var stream = new FileStream(pathToDirectory +"\\"+ name, FileMode.OpenOrCreate))
                {
                    var status = listRequest.DownloadWithStatus(stream);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        public string UploadFile(string pathToUploadFile)
        {
            try
            {   // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Drive API .NET Quickstart"
                });
                //Init metadata
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"{GetNameFromPath(pathToUploadFile)}"
                };

                // Create a new file on drive.
                FilesResource.CreateMediaUpload request;// Define parameters of request.
                using (var stream = new FileStream(pathToUploadFile, FileMode.Open))
                {
                    request = service.Files.Create(fileMetadata, stream, "text/plain");
                    request.Fields = "id";
                    request.Upload();
                }
                var file = request.ResponseBody;
                return file.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        /// <summary>
        /// Update file in google drive
        /// </summary>
        /// <param name="pathToUpdateFile">Path to file in your computer</param>
        /// <param name="fileId">Id of file in google drive</param>
        /// <returns>Return file id if successful</returns>
        public string UpdateFile(string pathToUpdateFile, string fileId)
        {
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Drive API .NET Quickstart"
                });
                //Init metadata
                FilesResource.ListRequest requestName = service.Files.List();
                requestName.Q = "trashed = false";
                var result = requestName.Execute();
                string name = "newItem.txt";
                foreach (var item in result.Files)
                {
                    if (item.Id == fileId)
                        name = item.Name;
                }
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = name
                };

                FilesResource.UpdateMediaUpload request;
                using (var stream = new FileStream(pathToUpdateFile, FileMode.OpenOrCreate))
                {
                    request = service.Files.Update(fileMetadata, fileId, stream, "text/plain");
                    request.Fields = "id";
                    request.Upload();
                }

                var file = request.ResponseBody;
                return file.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        public string GetFileID(string fileName)
        {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential
                });

                FilesResource.ListRequest request = service.Files.List();
                request.Q = $"name = '{fileName}' and trashed = false";
                var result = request.Execute();
                
                if(result.Files !=null)
                {
                    return result.Files[0]?.Id;
                }
                else
                {
                    Console.WriteLine("File not found");
                    return null;
                }
            return null;
        }
        public DateTime? GetFileModifiedDate(string fileName)
        {
            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = Credential
            });

            FilesResource.GetRequest request = service.Files.Get(GetFileID(fileName));
            request.Fields = "modifiedTime";
            var result = request.Execute();
            return result.ModifiedTime;
        }
        #region Helper methods
        private string GetNameFromPath(string pathToUploadFile)
        {
            return pathToUploadFile.Substring(pathToUploadFile.LastIndexOf('\\')+1);
        }
        #endregion
    }
}
