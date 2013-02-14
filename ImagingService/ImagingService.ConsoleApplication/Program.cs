
using System;
using System.Diagnostics;
using System.IO;
using ImagingService.Configuration;

namespace ImagingService.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            Trace.WriteLine(string.Format("Begin processing ..."));

            var clientConfiguration = LoadConfigurationFromXml("clientConfiguration.config");
        
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            var processedFilesCount = 0;

            try
            {
                processedFilesCount = new FileProcessor(clientConfiguration).Process();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception occured during processing: {0}", ex.Message);
            }

            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed;
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds / 10);
            Trace.WriteLine(string.Format("Finished processing - {0} files successfully processed in {1} hh:mm:ss:ms", processedFilesCount, elapsedTime));
        }

        private static ClientConfiguration LoadConfigurationFromXml (string path)
        {
            ClientConfiguration configuration;

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ClientConfiguration));
            using (var reader = new StreamReader(path))
                configuration = (ClientConfiguration) serializer.Deserialize(reader);

            return configuration;
        }
    }
}
