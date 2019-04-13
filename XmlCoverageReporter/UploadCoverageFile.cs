using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.TeamFoundation.TestClient.PublishTestResults;
using Microsoft.VisualStudio.Services.WebApi;

namespace XmlCoverageReporter
{
    class UploadCoverageFile
    {
        public static void UploadCodeCoverageAttachmentsToNewStore(int buildId, string projectId, List<string> coverageFiles, string collectionUrl, string token)
        {
            var connection = new VssConnection(new Uri(collectionUrl), new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential(token));

            TestLogStore logStore = new TestLogStore(connection, new LogTraceListener() );

            foreach (var file in coverageFiles)
            {
                Dictionary<string, string> metaData = new Dictionary<string, string>();
                metaData.Add("ModuleName", Path.GetFileName(file));
                var attachment = logStore.UploadTestBuildLogAsync(new Guid(projectId), buildId, Microsoft.TeamFoundation.TestManagement.WebApi.TestLogType.Intermediate, file, metaData, null, true, System.Threading.CancellationToken.None).Result;
            }
        }
    }

    public class LogTraceListener : TraceListener
    {
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            switch (eventType)
            {
                case TraceEventType.Information:
                    Console.WriteLine(message);
                    break;
                case TraceEventType.Warning:
                    Console.WriteLine(message);
                    break;
                case TraceEventType.Verbose:
                    Console.WriteLine(message);
                    break;
            }
        }

        public override void Write(string message)
        {
            Console.WriteLine(message);
        }

        public override void WriteLine(string message)
        {
            Console.WriteLine(message);

        }
    }
}