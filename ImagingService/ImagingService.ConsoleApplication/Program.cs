using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ImagingService.ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var sourcePath = "C:\\temp\\imagingservice\\source";
            var destinationPath = "C:\\temp\\imagingservice\\destination";
            var processedPath = "C:\\temp\\imagingservice\\processed";

            var fileProcessor = new FileProcessor(sourcePath, destinationPath, processedPath);

            fileProcessor.Process();

        }

       
    }
}
