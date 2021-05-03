using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using vinkekfish;
using IndexRanges;
using static cryptoprime.VinKekFish.VinKekFishBase_etalonK1;
using cryptoprime;

namespace Test1
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            var gen = new VinKekFish_k1_base_20210419_keyGeneration();

            var keyForTables    = new Range(BLOCK_SIZE, true);
            keyForTables.inited = true;
            BytesBuilder.ToNull(keyForTables);

            gen.Init1(NORMAL_ROUNDS, keyForTables, BLOCK_SIZE, null);
            

            Console.WriteLine("Test end successfully");
            Console.ReadKey();
        }
    }
}
