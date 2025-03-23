using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Backend.Controllers
{
/// <summary>
/// This controller class is responsible for pulling from the amazon s3 bucket
/// Author: Sven Masche
/// </summary>
    public class BucketController
    {
        //declaring global variables
        private static IAmazonS3 s3Client;
        BasicAWSCredentials credentials;
        string bucketName;

        /// <summary>
        /// General constructor sets global variables from environment variables
        /// Author: Sven Masche
        /// </summary>
        public BucketController()
        {
            string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
            string region = Environment.GetEnvironmentVariable("AWS_REGION");
            bucketName = Environment.GetEnvironmentVariable("AWS_BucketName");

            //checking if environment variables are set
            if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(region))
            {
                throw new InvalidOperationException("AWS environment variables are not set.");
            }

            credentials = new BasicAWSCredentials(accessKey, secretKey);
            s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.GetBySystemName(region));
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method to retrieve a list of all csv files stored in the bucket
        /// Author: Sven Masche
        /// </summary>
        public async Task<List<S3Object>> ListCSVFiles()
        {
            //creating the request
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            //setting and returning the result
            var result = await s3Client.ListObjectsV2Async(request);
            return result.S3Objects.Where(obj => obj.Key.EndsWith(".csv")).ToList();
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// This method saves a file from the bucket to a temporary file path in the system.
        /// Author: Sven Masche
        /// </summary>
        public async Task<string> DownloadCSV(string fileName)
        {
            var fileTransferUtility = new TransferUtility(s3Client);
            string filePath = Path.Combine(Path.GetTempPath(), fileName);

            await fileTransferUtility.DownloadAsync(filePath, bucketName, fileName);

            return filePath; //Return the file path to the downloaded file
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Method facilitates the process needed to upload a csv file to the s3 bucket.
        /// Author: Sven Masche
        /// </summary>
        public async Task<string> UploadCSV( IFormFile inputCSV)
        {
            //input validation
            if (inputCSV == null)
                return "Input CSV file is null.";

            //kind of a uselsess check, but i guess you can never be toooo safe
            if (s3Client == null)
                return "S3 client is not initialized.";

            //try to upload files
            try
            {
                var fileTransferUtility = new TransferUtility(s3Client);

                //setting the file name to the date, as it needs to be unique 
                //and we have no idea what other nameing convention to use instead :/
                string newFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv";

                using (var stream = inputCSV.OpenReadStream())
                {
                    if (stream == null)
                        return "Failed to open a stream for the input CSV file.";

                    await fileTransferUtility.UploadAsync(stream, bucketName, newFileName);
                }

                return "File uploaded successfully.";
            }
            //catch potential failures
            catch (AmazonS3Exception e)
            {
                return $"AWS S3 error: {e.Message}";
            }
            catch (Exception e)
            {
                return $"An error occurred: {e.Message}";
            }
        }
    //----------------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
//--------------------------------------------------------| (• ◡•)| (❍ᴥ❍ʋ)----------------------------------------------------------
