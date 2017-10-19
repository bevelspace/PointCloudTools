using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointCloudTools
{
    class Program
    {

        static void Main(string[] args)
        {

            Console.Write("Enter source folder: ");
            string sourceFolder = Console.ReadLine();
            Console.Write("Enter file name: ");
            string sourceName = Console.ReadLine();

            PointCloud cloud = new PointCloud(sourceFolder, sourceName);

            Console.WriteLine("Please enter number of divisions in each dimension.");
            int divisions = Convert.ToInt32(Console.ReadLine());

            cloud.Divide(divisions);



        }


    }
}
