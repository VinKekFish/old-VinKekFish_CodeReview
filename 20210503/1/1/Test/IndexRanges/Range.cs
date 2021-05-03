using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexRanges
{
    public unsafe class Range
    {
        public long min = 0;
        public long max = 0;

        public bool haveMin  = true;
        public bool haveMax  = true;
        public bool inited   = false;
        public bool Readonly = false;

        public string fileName = null;

        protected Range(long Length = 0, bool realAllocation = false)
        {
            this.Length = Length;

            if (realAllocation)
                _array = new byte[Length];
        }

        /// <summary>Выделить условный участок памяти</summary>
        /// <param name="Length">Длина участка</param>
        /// <param name="realAllocation">Реально выделить этот участок внутри Range</param>
        public static Range @new(int Length = 0, bool realAllocation = false)
        {
            return new Range(Length: Length, realAllocation: realAllocation);
        }

        /// <summary>Сделать производный Range от старого. Это нужно, когда к указателю сделали приращение</summary>
        /// <param name="a">Старый Range (базовый)</param>
        /// <param name="ToAdd">Длина приращения</param>
        public static Range derivativeRange(Range a, long ToAdd)
        {
            var result = new Range(a.Length - ToAdd);

            result.baseRange       = a;
            result.baseRangeOffset = ToAdd;
            result._ptr            = a.ptr;

            return result;
        }

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

        protected byte[] _array = null;
        protected byte * _ptr   = null;

        public byte[] array
        {
            get
            {
                if (_array != null)
                    return _array;

                if (baseRange != null)
                    return baseRange.array;

                return null;
            }
        }

        public byte * ptr
        {
            get
            {
                if (_ptr != null)
                    return _ptr;

                if (baseRange != null)
                    return baseRange._ptr + baseRangeOffset;

                return null;
            }
        }

        public byte this[int index]
        {
            get
            {
                if (baseRange != null)
                {
                    // Access(index + baseRangeOffset, write: false);
                    baseRange.Access(index + baseRangeOffset, write: false);

                    if (array != null)
                        throw new Exception();
                    else
                    if (ptr != null)
                        return ptr[index];

                    return 0;
                }
                else
                {
                    Access(index, write: false);

                    if (array != null)
                        return array[index];
                    else
                    if (ptr != null)
                        return ptr[index];

                    return 0;
                }
            }
            set
            {
                if (baseRange != null)
                {
                    baseRange.Access(index + baseRangeOffset, write: true);

                    if (array != null)
                        throw new Exception();
                    else
                    if (ptr != null)
                        ptr[index] = value;
                }
                else
                {
                    Access(index, write: true);

                    if (array != null)
                        array[index] = value;
                }
            }
        }

        public void Access(long minIndex, long maxIndex, bool write = false)
        {
            Access(minIndex, write);
            Access(maxIndex, write);
        }

        public static implicit operator byte *(Range r)
        {
            return null;
        }

        protected Range baseRange = null;
        public    long  baseRangeOffset = 0;
        public static Range operator +(Range a, long len)
        {
            var r = derivativeRange(a, len);

            return r;
        }

        public static bool operator > (Range a, Range b)
        {
            return a.ptr > b.ptr;
        }

        public static bool operator < (Range a, Range b)
        {
            return a.ptr < b.ptr;
        }
    }
}
