using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DotNet46CustomActivity
{
    // NOTE: This is a *toy* implementation of CrossAppDomainDotNetActivity.  Proper error handling has been elided 
    // for brevity's sake.  A production implementation should include proper error handling.
    class MyDotNetActivity : CrossAppDomainDotNetActivity<MyDotNetActivityContext>
    {
        protected override MyDotNetActivityContext PreExecute(IEnumerable<LinkedService> linkedServices,
            IEnumerable<Dataset> datasets, Activity activity, IActivityLogger logger)
        {
            logger.Write("This is just a simple CustomActivity to test .net 4.6 and pass the result to storage blob!");
            WriteGreeting(logger);
            // Process ADF artifacts up front as these objects are not serializable across app domain boundaries.
            Dataset dataset = datasets.First(ds => ds.Name == activity.Outputs.Single().Name);
            var blobProperties = (AzureBlobDataset)dataset.Properties.TypeProperties;
            LinkedService linkedService = linkedServices.First(ls => ls.Name == dataset.Properties.LinkedServiceName);
            var storageProperties = (AzureStorageLinkedService)linkedService.Properties.TypeProperties;
            return new MyDotNetActivityContext
            {
                ConnectionString = storageProperties.ConnectionString,
                FolderPath = blobProperties.FolderPath,
                FileName = blobProperties.FileName
            };
        }

        public override IDictionary<string, string> Execute(MyDotNetActivityContext context, IActivityLogger logger)
        {
            WriteGreeting(logger);
            // This demonstrates using a type (i.e., SqlConnection.AccessToken) available in .net 4.6 but not 4.5.
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = "<Server Name>"; // replace with your server name
            builder["Initial Catalog"] = "<Database Name>"; // replace with your database name
            builder["Connect Timeout"] = 30;

            string message;
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    connection.AccessToken = "<your access token>";
                    connection.Open();
                    message = "Connected to the database";
                }
                catch (Exception ex)
                {
                    message= ex.Message;
                }
            }


            // log the output folder path    
            logger.Write("Writing blob to the folder: {0}", context.FolderPath);

            // create a storage object for the output blob.
            CloudStorageAccount outputStorageAccount = CloudStorageAccount.Parse(context.ConnectionString);
            // write the name of the file.
            Uri outputBlobUri = new Uri(outputStorageAccount.BlobEndpoint, context.FolderPath + "/" + context.FileName);
            CloudBlockBlob outputBlob = new CloudBlockBlob(outputBlobUri, outputStorageAccount.Credentials);
            logger.Write("Writing {0} to the output blob", message);
            outputBlob.UploadText(message);

            return new Dictionary<string, string>();
        }

        static void WriteGreeting(IActivityLogger logger)
        {
            // This demonstrates in which app domain the caller is running.
            logger.Write("Hello, world, from app domain '{0}'!", AppDomain.CurrentDomain.FriendlyName);
        }
    }
}

