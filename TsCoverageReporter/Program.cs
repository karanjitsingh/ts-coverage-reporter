using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;

namespace TsCoverageReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var collectionUri = Environment.GetEnvironmentVariable("System.TeamFoundationCollectionUri");
            var projectId = Environment.GetEnvironmentVariable("System.TeamProjectId");
            var buildId = int.Parse(Environment.GetEnvironmentVariable("Build.BuildId"));
            var token = Environment.GetEnvironmentVariable("SYSTEM_ACCESSTOKEN");

            UploadCoverageFile.UploadCodeCoverageAttachmentsToNewStore(buildId, projectId, args.ToList(), collectionUri, token);
        }

        private static string MergeCoverageFiles(List<string> coverageFiles)
        {
            CoverageReportParser parser = new CoverageReportParser(1, new string[] { }, new DefaultFilter(new string[] { }),
                    new DefaultFilter(new string[] { }),
                    new DefaultFilter(new string[] { }));

            ReadOnlyCollection<string> collection = new ReadOnlyCollection<string>(coverageFiles);
            var parseResult = parser.ParseFiles(collection);

            List<FileCoverage> fileCoverages = new List<FileCoverage>();

            foreach (var assembly in parseResult.Assemblies)
            {
                foreach (var @class in assembly.Classes)
                {
                    foreach (var file in @class.Files)
                    {
                        FileCoverage resultFileCoverageInfo = new FileCoverage { FilePath = file.Path, LineCoverageStatus = new Dictionary<uint, CoverageStatus>() };
                        int lineNumber = 1;
                        foreach (var line in file.lineCoverage)
                        {
                            if (line != -1)
                            {
                                resultFileCoverageInfo.LineCoverageStatus.Add((uint)lineNumber, line == 0 ? CoverageStatus.NotCovered : CoverageStatus.Covered);
                            }
                            ++lineNumber;
                        }

                        fileCoverages.Add(resultFileCoverageInfo);
                    }
                }
            }

            string mergeFile = Path.GetTempFileName();
            File.WriteAllText(mergeFile, JsonConvert.SerializeObject(fileCoverages));
            return mergeFile;
        }
    }

    struct FileCoverage
    {
        public string FilePath;
        public Dictionary<uint, CoverageStatus> LineCoverageStatus;
    }
}
