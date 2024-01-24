using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IFirebaseService
    {
        Task<string?> UploadFileToFirebaseStorage(IFormFile files, string fileName, string folderName);
        Task DeleteFileInFirebaseStorage(string fileName, string folderName);
        Task<string?> DownloadFileFromFirebaseStorage(string fileName, string folderName);
        Task<FirebaseMetaData?> GetMetadataFileFromFirebaseStorage(string fileName, string folderName);
    }
}
