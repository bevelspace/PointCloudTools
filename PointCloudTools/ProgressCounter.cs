using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PointCloudTools
{
    class ProgressCounter
    {
        int counter;
        int total;
        int portion = 100;

        public ProgressCounter(int total)
        {
            this.total = total;
            counter = 0;

        }

        public void Increment()
        {
            counter++;
            if (counter > total / portion)
            {
                Console.Write(".");
                counter = 0;
            }
            return;
        }

        public void Increment(int indicateAtPortion)
        {
            portion = indicateAtPortion;
            Increment();
        }

    }
}
