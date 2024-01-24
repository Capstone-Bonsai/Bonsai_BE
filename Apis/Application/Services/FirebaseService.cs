using Application.Commons;
using Microsoft.AspNetCore.Http;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Storage;
using Application.Interfaces;

namespace Application.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseConfiguration _firebaseConfiguration;

        public FirebaseService(FirebaseConfiguration config)
        {
            _firebaseConfiguration = config;
        }

        private async Task<string> SignInAndGetAuthToken()
        {
            //Firebase config
            var config = new FirebaseAuthConfig
            {
                ApiKey = _firebaseConfiguration.ApiKey,
                AuthDomain = _firebaseConfiguration.AuthDomain,
                Providers = new FirebaseAuthProvider[]{
                        new EmailProvider(),
                    }
            };
            var client = new FirebaseAuthClient(config);
            var authResult = await client.SignInWithEmailAndPasswordAsync(_firebaseConfiguration.AuthEmail, _firebaseConfiguration.AuthPassword);
            return await authResult.User.GetIdTokenAsync();
        }

        public async Task<string?> UploadFileToFirebaseStorage(IFormFile files, string fileName, string folderName)
        {
            if (files.Length > 0)
            {
                var task = new FirebaseStorage(
                    _firebaseConfiguration.Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = SignInAndGetAuthToken,
                        ThrowOnCancel = true
                    })
                    .Child(folderName)
                    .Child($"{fileName}.{Path.GetExtension(files.FileName).Substring(1)}")
                    .PutAsync(files.OpenReadStream(), new CancellationTokenSource().Token);
                try
                {
                    string? urlFile = await task;
                    return urlFile;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return null;

        }

        public async Task DeleteFileInFirebaseStorage(string fileName, string folderName)
        {
            var task = new FirebaseStorage(
                _firebaseConfiguration.Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = SignInAndGetAuthToken,
                    ThrowOnCancel = true
                })
                .Child(folderName)
                .Child(fileName)
                .DeleteAsync();
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string?> DownloadFileFromFirebaseStorage(string fileName, string folderName)
        {
            var task = new FirebaseStorage(
                _firebaseConfiguration.Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = SignInAndGetAuthToken,
                    ThrowOnCancel = true
                })
                .Child(folderName)
                .Child(fileName).
                GetDownloadUrlAsync();
            try
            {
                var downloadUrl = await task;
                return downloadUrl;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<FirebaseMetaData?> GetMetadataFileFromFirebaseStorage(string fileName, string folderName)
        {
            var task = new FirebaseStorage(
                _firebaseConfiguration.Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = SignInAndGetAuthToken,
                    ThrowOnCancel = true
                })
                .Child(folderName)
                .Child(fileName).
                GetMetaDataAsync();
            try
            {
                var metadata = await task;
                return metadata;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}