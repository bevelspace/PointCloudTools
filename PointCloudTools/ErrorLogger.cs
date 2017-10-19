using System;
using System.Collections.Generic;
using System.IO;

namespace PointCloudTools
{
    class ErrorLogger
    {
        int errorCount = 0;
        List<string> errorList = new List<string>();
        string destinationPath;

        public ErrorLogger(string destinationLocation, string errorType)
        {
            destinationPath = destinationLocation + @"\" + errorType +
                " Error Log " + DateTime.Now.ToString().Replace('/', '-').Replace(':', '.') + ".txt";
        }

        public void AddError(string errorString)
        {
            errorList.Add(errorString);
            errorCount++;
        }

        public void writeErrorLog()
        {
            string[] errorArray = errorList.ToArray();
            File.WriteAllLines(destinationPath, errorArray);
        }

        public int GetErrorCount()
        {
            return errorCount;
        }

    }
}
