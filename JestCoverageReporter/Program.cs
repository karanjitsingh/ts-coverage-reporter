using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Newtonsoft.Json;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;

namespace JestCoverageReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var collectionUri = Environment.GetEnvironmentVariable("System.TeamFoundationCollectionUri");
            var projectId = Environment.GetEnvironmentVariable("System.TeamProjectId");
            int.TryParse(Environment.GetEnvironmentVariable("Build.BuildId"), out int buildId);
            var token = Environment.GetEnvironmentVariable("SYSTEM_ACCESSTOKEN");

            var coverageDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var mergeFile = Path.Combine(coverageDir, Guid.NewGuid().ToString() + ".coverage.json");

            Directory.CreateDirectory(coverageDir);

            MergeCoverageFiles(args.ToList(), mergeFile);
            Console.WriteLine(mergeFile);

            UploadCoverageFile.UploadCodeCoverageAttachmentsToNewStore(buildId, projectId, new List<string>() { mergeFile }, collectionUri, token);
        }

        private static void MergeCoverageFiles(List<string> coverageFiles, string mergeFile)
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
            
            File.WriteAllText(mergeFile, JsonConvert.SerializeObject(fileCoverages));
        }
    }

    struct FileCoverage
    {
        public string FilePath;
        public Dictionary<uint, CoverageStatus> LineCoverageStatus;
    }
}
