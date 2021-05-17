using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _20210511_keccakprng
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            const int count = 65536;
            // const int size  = 4096;

            var b0 = new byte[4] {0, 0, 0, 0};

            var prng = new vinkekfish.keccak.keccak_20200918.Keccak_PRNG_20201128();
            fixed (byte * _b0 = b0)
            {
                prng.InputKeyAndStep(_b0, b0.Length, null, 0);
            }

            // 720 тыс.
            var dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                prng.calcStep(inputReadyCheck: prng.isInputReady, SaveBytes: true);
            }
            var dt2 = DateTime.Now;
            var ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));

            prng.Dispose();

            Console.ReadKey();
        }
    }
}
