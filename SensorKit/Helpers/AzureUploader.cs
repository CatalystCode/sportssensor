using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorKit
{
    public class AzureUploader
    {
        CloudBlobContainer container = null;

        public AzureUploader(StorageCredentials credentials, string containerName)
        {

            var account = new CloudStorageAccount(credentials, true);
            // Create the blob client.
            var blobClient = account.CreateCloudBlobClient();
            // Retrieve reference to a previously created container.
            container = blobClient.GetContainerReference(containerName);

        }

        public async Task UploadFileAsync(string name, string path)
        {
            try
            {
                if (container != null)
                {
                    // Retrieve reference to a blob named "myblob".
                    var blockBlob = container.GetBlockBlobReference(name);
                    using (var stream = File.OpenRead(path))
                    {
                        await blockBlob.UploadFromStreamAsync(stream);
                    }
                }
            }
            catch (Exception x)
            {

            }
        }
    }
}
