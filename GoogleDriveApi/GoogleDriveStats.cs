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
        //string credentialsPath = Path.Combine(Environment.CurrentDirectory, "Credential.json");
        string pathToUploadFile = Path.Combine(Environment.CurrentDirectory, "testdrive.txt");//path and file name
        string pathToDirecoty = @"C:\Users\psair\Desktop\temper2.txt";
        string fileID = "1NcVS4e6Gwj2LD-I3sOz7q2dYv1uPPFdP";
        

        var t = DriveUploadTxtFile(pathToDirecoty, "testDrive", credential);
         
        ListenFile(credential);
        Console.ReadLine();

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
        public string DownloadFile(string pathToDirectory, string newFileName, string fileID)//path to upload file
        {
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Drive API .NET"
                });

                FilesResource.GetRequest listRequest = service.Files.Get(fileID);// Define parameters of request.
                using (var stream = new FileStream(pathToDirectory, FileMode.OpenOrCreate))
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
        public string UploadFile(string pathToUploadFile, string fileName)//path to upload file
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
                    Name = $"{fileName}.txt"
                };

                // Create a new file on drive.
                FilesResource.CreateMediaUpload request;// Define parameters of request.
                using (var stream = new FileStream(pathToUploadFile, FileMode.Open))
                {

                    request = service.Files.Create(
                        fileMetadata, stream, "text/plain");
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
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = "test.txt"
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
        public string ListenFile(UserCredential Credential)
        {
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential
                });
                //Init metadata


                FilesResource.ListRequest request = service.Files.List();
                request.Q = "name = 'test.txt' and trashed = false";
                var result = request.Execute();
                foreach (var item in result.Files)
                {

                    Console.WriteLine(item.Id);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
    }
}
