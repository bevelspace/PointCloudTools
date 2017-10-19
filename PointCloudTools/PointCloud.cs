using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PointCloudTools
{
    class PointCloud
    {
        string sourceLocation;
        string sourceName;
        public string sourcePath;
        public string destinationLocation;


        //make private
        public int fileLength;

        float xMin = 99999f;
        float xMax = 99999f;
        float yMin = 99999f;
        float yMax = 99999f;
        float zMin = 99999f;
        float zMax = 99999f;

        float deltaX = 99999f;
        float deltaY = 99999f;
        float deltaZ = 99999f;

        int errors = 0;

        string[] sourceLines = new string[0];

        //constructor with the location info. Maybe add default constructor later. 
        //calls SetSource, CreateDestinationPath
        public PointCloud(string sourceLocation, string sourceName)
        {
            this.sourceLocation = sourceLocation;
            this.sourceName = sourceName;

            SetSource(sourceLocation, sourceName);
            CreateDestinationPath();

            File.OpenRead(sourcePath);
            sourceLines = File.ReadAllLines(sourcePath);
            fileLength = sourceLines.Length;

            Console.WriteLine("{0} points found in point cloud.", fileLength);
        }

        //Asks the questions and sets the source Path
        //=======the query should actually be done in the application and the assignments should be in the constructor
        //this method should only exist if I want to be able to change it later, or instantiate the object empty
        public void SetSource(string sourceLocation, string sourceName)
        {
            sourcePath = sourceLocation + @"\" + sourceName;

            if (File.Exists(sourcePath))
            {
                Console.WriteLine("Source Path Succesfully set");
            }
            else
            {
                Console.WriteLine(@"Error: {0} not found. Check source.", sourcePath);
            }

        }

        //makes a folder to put all the new tiled pointclouds into
        public void CreateDestinationPath()
        {
            //check path validity
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine(@"Error: {0} not found. Check source.", sourcePath);
                return;
            }

            int counter = 01;
            while (true)
            {
                destinationLocation = sourcePath + " Tiles " + counter.ToString() + @"\";
                if (!Directory.Exists(destinationLocation))
                {
                    Directory.CreateDirectory(destinationLocation);
                    return;
                }
                else
                {
                    counter++;
                }

            }
        }



        //Organizes most of the work. Calls the functions that carry out the division process. 
        //Calls 
        public void Divide(int divisions)
        {

            //create files and jagged array to store file paths.
            string[][] destinationPaths = new string[divisions][];
            for (int i = 0; i < divisions; i++)
            {
                destinationPaths[i] = new string[divisions];
                for (int j = 0; j < divisions; j++)
                {
                    string destinationPath = destinationLocation + (i + 1).ToString() + "-" + (j + 1).ToString() + ".pts";
                    destinationPaths[i][j] = destinationPath;
                }
            }


            //Converting from text to data
            float[][] coordinates = new float[fileLength][];
            int[][] colors = new int[fileLength][];
            coordinates = CreateCoordinateArray(coordinates);
            colors = CreateColorArray(colors);

            //checking limits
            getLimits(coordinates);
            if (errors > 0)
            { Console.WriteLine("{0} errors found in conversion. Examine point cloud file.", errors); }
            else
            { Console.WriteLine("No errors found in conversion."); }
            errors = 0;

            DateTime startTime = DateTime.Now;
            //go through each line. check X and check Y and then add that line to the jagged array string files>>lines.
            List<string>[][] sortedPoints = new List<string>[divisions][];
            sortedPoints = SortPoints(sortedPoints, coordinates, colors, divisions);
            if (errors > 0)
            { Console.WriteLine("{0} sorting errors.", errors); }
            else
            { Console.WriteLine("No errors in sorting process."); }
            errors = 0;
            DateTime endTime = DateTime.Now;
            Console.WriteLine(@"Time to sort points: {0}", endTime - startTime);



            //write the strings from the jagged array string to the files.
            WriteNewFiles(sortedPoints, destinationPaths, divisions);
            Console.WriteLine("\n Processing complete.");
            Console.WriteLine(@"Find new files in {0}", destinationLocation);
        }

        //converts the bunch of text into a jagged array of coordinates and a matching jagged array of RGB values.
        float[][] CreateCoordinateArray(float[][] coordinates)
        {
            ProgressCounter coordinateProgress = new ProgressCounter(fileLength);
            ErrorLogger CoordinateArrayErrors = new ErrorLogger(destinationLocation, "coordinate");

            string[] sourceLines = File.ReadAllLines(sourcePath);

            //iterate through lines, sending out a min/max check
            Console.WriteLine("Building data array:");
            for (int i = 0; i < fileLength; i++)
            {
                string[] pointParameters = sourceLines[i].Split(' ');

                coordinates[i] = new float[3];
                try { coordinates[i][0] = Convert.ToSingle(pointParameters[0]); } catch { coordinates[i][0] = 0; errors++; CoordinateArrayErrors.AddError(ErrorBuilder(i)); }
                try { coordinates[i][1] = Convert.ToSingle(pointParameters[1]); } catch { coordinates[i][1] = 0; errors++; CoordinateArrayErrors.AddError(ErrorBuilder(i)); }
                try { coordinates[i][2] = Convert.ToSingle(pointParameters[2]); } catch { coordinates[i][2] = 0; errors++; CoordinateArrayErrors.AddError(ErrorBuilder(i)); }
                coordinateProgress.Increment();
            }
            Console.WriteLine("\n");
            CoordinateArrayErrors.writeErrorLog();
            return coordinates;
        }
        int[][] CreateColorArray(int[][] colors)
        {
            DateTime startTime = DateTime.Now;

            string[] sourceLines = File.ReadAllLines(sourcePath);

            //iterate through lines, sending out a min/max check
            ProgressCounter colorProgress = new ProgressCounter(fileLength);
            Console.WriteLine("Reading RGB array:");

            for (int i = 0; i < fileLength; i++)
            {
                string[] pointParameters = sourceLines[i].Split(' ');

                colors[i] = new int[3];

                try { colors[i][0] = Convert.ToInt32(pointParameters[3]); } catch { colors[i][0] = 0; errors++; }
                try { colors[i][1] = Convert.ToInt32(pointParameters[4]); } catch { colors[i][1] = 0; errors++; }
                try { colors[i][2] = Convert.ToInt32(pointParameters[5]); } catch { colors[i][2] = 0; errors++; }

                colorProgress.Increment();
            }
            Console.WriteLine("\n");
            return colors;
        }


        //runs through the pointcloud once to analyze size/location of the pointcloud to determine the cut lines
        void getLimits(float[][] coordinates)
        {
            DateTime startTime = DateTime.Now;

            string[] sourceLines = File.ReadAllLines(sourcePath);

            //iterate through lines, sending out a min/max check
            ProgressCounter limitProgress = new ProgressCounter(sourceLines.Length);
            Console.WriteLine("Analyzing limits:");
            for (int i = 0; i < sourceLines.Length; i++)
            {
                string[] pointParameters = sourceLines[i].Split(' ');

                xMin = CheckMin(xMin, coordinates[i][0]);
                xMax = CheckMax(xMax, coordinates[i][0]);
                yMin = CheckMin(yMin, coordinates[i][1]);
                yMax = CheckMax(yMax, coordinates[i][1]);
                zMin = CheckMin(zMin, coordinates[i][2]);
                zMax = CheckMax(zMax, coordinates[i][2]);

                limitProgress.Increment();
            }

            deltaX = xMax - xMin;
            deltaY = yMax - yMin;
            deltaZ = zMax - zMin;

            Console.WriteLine("");
            Console.WriteLine(@"Point Limits: xMin_{0} xMax_{1} yMin_{2} yMax_{3} zMin_{4} xMax_{5}",
                xMin, xMax, yMin, yMax, zMin, zMax);
            Console.WriteLine(@"Coordinate ranges: X_{0}, Y_{1}, Z_{2}", deltaX, deltaY, deltaZ);
            DateTime endTime = DateTime.Now;
            Console.WriteLine(@"Time to get limits: {0}", endTime - startTime);



        }

        //little functions to convert a string to a float and check if it's higher, then return the highest.
        private float CheckMax(float maxSoFar, float newEntry)
        {
            //for first entry
            if (maxSoFar == 99999f)
            {
                maxSoFar = newEntry;
            }
            //check Max
            if (newEntry > maxSoFar)
            {
                return newEntry;
            }
            return maxSoFar;
        }
        private float CheckMin(float minSoFar, float newEntry)
        {
            //for first entry
            if (minSoFar == 99999f)
            {
                minSoFar = newEntry;
            }
            //check min
            if (newEntry < minSoFar)
            {
                return newEntry;
            }
            return minSoFar;
        }

        public int getSection(string dataRow)
        {
            int section;
            section = Convert.ToInt32(Math.Floor(5.4f));

            return section;
        }

        private List<string>[][] SortPoints(List<string>[][] sortedPoints, float[][] coordinates, int[][] colors, int divisions)
        {
            ProgressCounter sortProgress = new ProgressCounter(fileLength);
            ErrorLogger sortErrors = new ErrorLogger(destinationLocation, "sorting");

            //initialize the lists within dortedPoints
            for (int i = 0; i < divisions; i++)
            {
                sortedPoints[i] = new List<string>[divisions];
                for (int j = 0; j < divisions; j++)
                {
                    sortedPoints[i][j] = new List<string>();
                }
            }

            for (int i = 0; i < fileLength; i++)
            {
                int sortX;
                int sortY;
                sortX = Convert.ToInt32(Math.Floor((coordinates[i][0] - xMin) / (deltaX / divisions)));
                sortY = Convert.ToInt32(Math.Floor((coordinates[i][1] - yMin) / (deltaY / divisions)));

                string newLine = coordinates[i][0].ToString() + " ";
                newLine += coordinates[i][1].ToString() + " ";
                newLine += coordinates[i][2].ToString() + " ";
                newLine += colors[i][0].ToString() + " ";
                newLine += colors[i][1].ToString() + " ";
                newLine += colors[i][2].ToString() + " ";

                try { sortedPoints[sortX][sortY].Add(newLine); }
                catch
                {
                    errors++;
                    sortErrors.AddError(ErrorBuilder(i));
                }


                sortProgress.Increment();
            }

            sortErrors.writeErrorLog();
            return sortedPoints;
        }

        private void WriteNewFiles(List<string>[][] sortedPoints, string[][] destinationPaths, int divisions)
        {
            ProgressCounter writeProgress = new ProgressCounter(divisions * divisions);
            for (int i = 0; i < divisions; i++)
            {
                for (int j = 0; j < divisions; j++)
                {
                    string[] lineArray = sortedPoints[i][j].ToArray();
                    File.WriteAllLines(destinationPaths[i][j], lineArray);

                    writeProgress.Increment();
                }
            }
            Console.WriteLine("");
            return;
        }

        //private void WriteLines(List<string> lineList, string destination)
        //{
        //    for (int i = 0; i < lineList.Count; i++)
        //    {

        //    }
        //}

            //puts together the string to send to the error logger
        private string ErrorBuilder(int line)
        {
            string errorLine = "Error on line: " + line.ToString();
            string lineCopy = " [" + sourceLines[line] + "] ";

            //add an option in this or an additional method to log the state
            //state includes minimums, maximums, sorting ranges, etc.

            return errorLine + lineCopy;

        }
    }
}
