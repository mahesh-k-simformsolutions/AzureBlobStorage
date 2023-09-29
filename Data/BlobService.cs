using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Mime;

namespace AzureBlobStorage.Data
{
    public class BlobContainerListItem
    {
        public string FileName { get; set; }
        public IBrowserFile File { get; set; }
        public string ContainerName { get; set; }
        public string DestinationContainerName { get; set; }
    }
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        public BlobService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }
        public async Task<List<BlobContainerListItem>> GetAllContainers()
        {
            var blobContainerList = new List<BlobContainerListItem>();
            await foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainersAsync())
            {
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(container.Name);
                blobContainerList.Add(new BlobContainerListItem
                {
                    ContainerName = container.Name,
                });
            }
            return blobContainerList;
        }
        public async Task<List<BlobContainerListItem>> GetAll()
        {
            var blobContainerList = new List<BlobContainerListItem>();
            await foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainersAsync())
            {
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(container.Name);

                await foreach (BlobItem blob in containerClient.GetBlobsAsync())
                {
                    blobContainerList.Add(new BlobContainerListItem
                    {
                        FileName = blob.Name,
                        ContainerName = container.Name,
                    });
                }
            }
            return blobContainerList;
        }
        public async Task<bool> UploadFile(string blobContainerName, IBrowserFile file)
        {
            BlobClient blobClient;
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
                blobClient = containerClient.GetBlobClient(file.Name);
                var result = await blobClient.UploadAsync(file.OpenReadStream(), new BlobHttpHeaders { ContentType = file.ContentType });
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public async Task<string> Download(string blobContainerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            return downloadResult.Content.ToString();
        }

        public async Task<bool> Copy(BlobContainerListItem model)
        {
            try
            {

                BlobContainerClient sourceContainerClient = _blobServiceClient.GetBlobContainerClient(model.ContainerName);
                BlobContainerClient destinationContainerClient = _blobServiceClient.GetBlobContainerClient(model.DestinationContainerName);
                BlobClient sourceBlobClient = sourceContainerClient.GetBlobClient(model.FileName);

                if (await sourceBlobClient.ExistsAsync())
                {
                    BlobClient destinationBlobClient = destinationContainerClient.GetBlobClient(model.FileName);
                    await destinationBlobClient.StartCopyFromUriAsync(new Uri(sourceBlobClient.Uri.ToString()));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> Delete(string blobContainerName, string fileName)
        {
            try
            {
                BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(blobContainerName);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
