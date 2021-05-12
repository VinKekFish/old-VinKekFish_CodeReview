using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cryptoprime;

namespace _20210511
{
    class Program
    {
        static void Main(string[] args)
        {
            const int count = 65536;
            const int size  = 4096;

            var bb = new BytesBuilderForPointers();
            var alloc = new BytesBuilderForPointers.AllocHGlobal_AllocatorForUnsafeMemory();


            // 720 тыс.
            var dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                bb.add(alloc.AllocMemory(size));
            }
            var dt2 = DateTime.Now;
            var ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));

            bb.clear();


            // 740 тыс.
            dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                bb.add(alloc.AllocMemory(size));
            }
            dt2 = DateTime.Now;
            ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));

            bb.clear();
            bb.add(alloc.AllocMemory(size));


            // 1600 тыс.
            dt1   = DateTime.Now;
            var r = alloc.AllocMemory(bb.Count);
            for (var i = 0; i < count; i++)
            {
                bb.getBytes(resultA: r);
            }
            dt2 = DateTime.Now;
            ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));
            r.Dispose();



            // 540 тыс.
            dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                bb.getBytes(allocator: alloc).Dispose();
            }
            dt2 = DateTime.Now;
            ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));

            bb.clear();


            // 3 800 тыс.
            r = alloc.AllocMemory(bb.Count + 1);
            bb.add(alloc.AllocMemory(size));
            bb.add(alloc.AllocMemory(size));

            dt1 = DateTime.Now;
            for (var i = 0; i < count; i++)
            {
                bb.getBytesAndRemoveIt(r);
            }
            dt2 = DateTime.Now;
            ms  = (dt2-dt1).TotalMilliseconds;
            Console.WriteLine("Операций в секунду: " + (count * 1000.0 / ms));
            r.Dispose();
            bb.clear();



            Console.ReadKey();
        }
    }
}
