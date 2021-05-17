using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using vinkekfish.keccak.keccak_20200918;

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

            var table1 = new ushort[3200];
            for (ushort i = 0; i < table1.Length; i++)
            {
                table1[i] = i;
            }

            var dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                prng.calcStep(inputReadyCheck: prng.isInputReady, SaveBytes: true);
                prng.output.Clear();
            }
            var dt2 = DateTime.Now;
            var ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Блоков в секунду: " + (count * 1000.0 / ms));


            const int countForPermutations = count * 8 / 3200;

            dt1 = DateTime.Now;
            // 8 чисел на один блок (64/8), 3200 чисел на таблицу
            using (var b8 = new cryptoprime.BytesBuilderForPointers.AllocHGlobal_AllocatorForUnsafeMemory().AllocMemory(8))
            for (var i = 0; i < countForPermutations; i++)
            {
                doPermFantom(b8);
            }
            dt2 = DateTime.Now;
            ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));

            dt1 = DateTime.Now;
            // 8 чисел на один блок (64/8), 3200 чисел на таблицу
            for (var i = 0; i < countForPermutations; i++)
            {
                prng.doRandomPermutationForUShorts(table1);
            }
            dt2 = DateTime.Now;
            ms  = (dt2-dt1).TotalMilliseconds;
            // Вывод всё равно в блоках: значения приведены в эквивалентную величину
            Console.WriteLine("Блоков в секунду: " + (count * 1000.0 / ms));

            prng.Dispose();

            Console.ReadKey();

            void doPermFantom(cryptoprime.BytesBuilderForPointers.Record b8)
            {
                var len = (ulong)table1.LongLength;
                for (ulong j = 0; j < len - 1; j++)
                {
                    Keccak_PRNG_20201128.getCutoffForUnsignedInteger(0, (ulong)len - j - 1, out ulong cutoff, out ulong range);
                    prng.getUnsignedInteger(0, cutoff, range, b8);
                }
            }
        }
    }
}
