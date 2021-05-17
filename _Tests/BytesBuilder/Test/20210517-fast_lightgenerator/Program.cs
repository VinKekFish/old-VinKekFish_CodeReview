using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using vinkekfish;

namespace _20210517_fast_lightgenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const int count = 512;
            // const int size  = 64;
            const int size  = 64;

            var gen = new LightRandomGenerator(size);
            // gen.doWaitW = false;

            var dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                gen.WaitForGenerator();
                gen.ResetGeneratedBytes();
            }
            var dt2 = DateTime.Now;
            var ms  = (dt2-dt1).TotalMilliseconds;

            gen.Dispose();

            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));
            Console.ReadKey();
        }
    }
}
