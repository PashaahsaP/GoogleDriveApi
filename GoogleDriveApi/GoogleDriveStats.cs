using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Serilog;
namespace GoogleDriveApi
{
    public class GoogleDriveStats
    {
        ILogger log;
        UserCredential Credential { get; }
        public GoogleDriveStats(string pathToCredential)
        {
            log = new LoggerConfiguration() 
                    .WriteTo.File($"logs/{DateTime.Now.ToShortDateString()}.txt")
                   
                    .CreateLogger();
            Credential =   AuthorizeCredential(pathToCredential);
        }
        private UserCredential AuthorizeCredential(string credentialsPath)
        {
            log.Information("Авторизация полномочий");
            try
            {
                UserCredential credential;
                if (!string.IsNullOrEmpty(credentialsPath))
                {
                    using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))   // Load client secrets.So that get credentials need regitstrate them in https://console.cloud.google.com/apis/credentials
                    {
                        /* The file token.json stores the user's access and refresh tokens, and is created
                         automatically when the authorization flow completes for the first time. */
                        string credPath = "token.json";
                        var result =   GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            new[] { DriveService.Scope.Drive },
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).ConfigureAwait(false);
                        credential = result.GetAwaiter().GetResult();
                    }
                    if (credential == null)
                    {
                        log.Information("Не получилось получить полномочия");
                        return null;
                    }
                    else
                    {
                        log.Information($"Возращаемые полномочия {credential}");
                        return credential;
                    }
                }
                else
                {
                    log.Information("Не получилось получить полномочия");
                    return null;
                }

            }
            catch (Exception e)
            {
                log.Warning(e, "Что-то пошло не так при получении полномочий");
            }
            log.Information("Не получилось получить полномочия");
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
            log.Information("Началась закачка файла");
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
                    if (item.Id == fileID)
                        name = item.Name;
                }
                FilesResource.GetRequest listRequest = service.Files.Get(fileID);// Define parameters of request.
                using (var stream = new FileStream(pathToDirectory + "\\" + name, FileMode.OpenOrCreate))
                {
                    var status = listRequest.DownloadWithStatus(stream);
                }

            }
            catch (Exception e)
            {
                log.Warning(e, "Что-то пошло не так при закачке файла");
            }
            log.Information("Произошла ошибка при закачке файла");
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
            log.Information("Началось обновление файла на сервере");
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
                log.Information($"Обновленный файл id {file.Id}");
                return file.Id;
            }
            catch (Exception e)
            {
                log.Warning(e, "Обновление файла прервано");

            }
            log.Information("Не удалось обновить данные");
            return null;
        }
        public string UpdateFileInFolder(string pathToUpdateFile, string fileId,string folder)
        {
            log.Information("Началось обновление файла на сервере");
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
                requestName.Q = $"'{folder}' in parents and trashed=false";
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
                log.Information($"Обновленный файл id {file.Id}");
                return file.Id;
            }
            catch (Exception e)
            {
                log.Warning(e, "Обновление файла прервано");

            }
            log.Information("Не удалось обновить данные");
            return null;
        }
        public DateTime? GetFileModifiedDate(string fileName)
        {
            log.Information($"Получение даты модификации файла");
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential
                });

                FilesResource.GetRequest request = service.Files.Get(GetFileOrFolderID(fileName));
                request.Fields = "modifiedTime";
                var result = request.Execute();
                log.Information($"Время модификации {result.ModifiedTime}");
                return result.ModifiedTime;

            }
            catch (Exception e)
            {
                log.Warning(e, $"Не удалось получить дату модификации");
            }
            log.Information($"Не удалось получить дату модификации файла");
            return null;
        }
        public bool isFileOrFolderExist(string fileName)
        {
            log.Information($"Проверка наличия файла");

            try
            {
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential
                });
                FilesResource.ListRequest request = service.Files.List();
                request.Q = $"name = '{fileName}' and trashed = false";
                var result = request.Execute();
                if (result.Files != null && result.Files.Count > 0)
                {
                    log.Information($"Файл есть");
                    return true;
                }
                else
                {
                    log.Warning("Не удалось проверить наличие файла");
                    return false;
                }

            }
            catch (Exception e)
            {
                log.Warning(e, "Не удалось проверить наличие файла");
            }
            log.Warning("Не удалось проверить наличие файла");
            return false;
        }
        public string GetFileOrFolderID(string fileName)
        {
            log.Information("Началось получение id файла");
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential
                });

                FilesResource.ListRequest request = service.Files.List();
                request.Q = $"name = '{fileName}' and trashed = false";
                var result = request.Execute();

                if (result.Files != null)
                {
                    string id = result.Files[0]?.Id;
                    log.Information($"Получен id {id}");
                    return id;
                }
                else
                {
                    log.Information($"Файл не найден");
                    return null;
                }
                log.Information($"Файл не найден");
                return null;

            }
            catch (Exception e)
            {
                log.Warning(e, "Не получилось получить id");
            }
            log.Information($"Файл не найден");
            return null;
        }
        public string CreateFolder(string folderName,string? destinationFolderID = null)
        {
            log.Information("Началось создание папки");
            try
            {
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Drive API .NET"
                });
                // File metadata
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };
                if(destinationFolderID != null)
                    fileMetadata.Parents= new List<string>() { destinationFolderID };
                // Create a new folder on drive.
                var request = service.Files.Create(fileMetadata);
                request.Fields = "id";
                var file = request.Execute();
                // Prints the created folder id.
                log.Information("Folder ID: " + file.Id);
                return file.Id;

            }
            catch (Exception e)
            {
                log.Warning(e, "Что-то пошло не так при создании папки");
            }
            log.Information("Произошла ошибка при создании файла");
            return null;
        }
        public string UploadFileInFolder(string pathToUploadFile, string? fileName = null, string? destinationFolderID=null)
        {
            log.Information("Началась загрузка файла на сервер");

            try
            {   // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = Credential,
                    ApplicationName = "Drive API .NET Quickstart"
                });
                //Init metadata
                Google.Apis.Drive.v3.Data.File fileMetadata = new();
                if (fileName == null)
                {
                    fileMetadata.Name = $"{GetNameFromPath(pathToUploadFile)}";
                }
                else
                {
                    fileMetadata.Name=fileName;
                }
                if (destinationFolderID != null)
                {
                    fileMetadata.Parents = new List<string>() { destinationFolderID };
                }
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
                log.Warning(e, "Что-то пошло не так при загрузке файла на сервер");
            }
            log.Information("Не удалось загрузить файлы на сервер");
            return null;
        }

        #region Helper methods
        private string GetNameFromPath(string pathToUploadFile)
        {
            return pathToUploadFile.Substring(pathToUploadFile.LastIndexOf('\\') + 1);
        }
        #endregion
    }
}
