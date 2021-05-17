using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightRandomGenerator_test
{
    class Program
    {
        static void Main(string[] args)
        {
            // const int count = 65536;
            // const int size  = 4096;

            var b0 = new byte[4] {0, 0, 0, 0};

            var prng = new vinkekfish.VinKekFish_k1_base_20210419_keyGeneration();

            prng.EnterToBackgroundCycle();
            Thread.Sleep(1000);
            prng.ExitFromBackgroundCycle();
                /*
            // 720 тыс.
            var dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                
            }
            var dt2 = DateTime.Now;
            var ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));
            */

            Console.WriteLine("Блоков в секунду: " + prng.BackgourndGenerated);

            prng.Dispose();

            Console.ReadKey();
        }
    }
}
