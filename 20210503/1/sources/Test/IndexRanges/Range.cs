using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexRanges
{
    public class Range
    {
        public long min = 0;
        public long max = 0;

        public bool haveMin  = true;
        public bool haveMax  = true;
        public bool inited   = false;
        public bool Readonly = false;

        public string fileName = null;

        public long Length
        {
            set
            {
                if (value >= 0)
                    max = value - 1;

                if (value < 0)
                    throw new Exception();
            }
            get => max + 1;
        }

        public void Access(long index, bool write = false)
        {
            if (!inited && !write)
                throw new Exception();
            if (Readonly && write)
                throw new Exception();

            if (index < 0)
                throw new Exception();

            if (haveMin)
            if (index < min)
                throw new Exception();

            if (haveMax)
            if (index > max)
                throw new Exception();
        }

        public void Access(long minIndex, long maxIndex, bool write = false)
        {
            Access(minIndex, write);
            Access(maxIndex, write);
        }
    }
}
