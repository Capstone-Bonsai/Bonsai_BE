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
    }
}
