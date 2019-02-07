using System;
using System.Collections.Generic;
using System.Text;

namespace StreamLibrary.src
{
    /// <summary>
    /// A helper class for pointers
    /// </summary>
    public class PointerHelper
    {
        private int _offset;

        public IntPtr Pointer
        {
            get;
            private set;
        }

        public int TotalLength { get; private set; }

        public int Offset
        {
            get { return _offset; }
            set
            {
                if (value < 0)
                    throw new Exception("Offset must be >= 1");

                if (value >= TotalLength)
                    throw new Exception("Offset cannot go outside of the reserved buffer space");

                _offset = value;
            }
        }

        public PointerHelper(IntPtr pointer, int Length)
        {
            this.TotalLength = Length;
            this.Pointer = pointer;
        }

        /// <summary>
        /// Copies data from Source to the current Pointer Offset
        /// </summary>
        public void Copy(IntPtr Source, int SourceOffset, int SourceLength)
        {
            if (CheckBoundries(this.Offset, SourceLength))
                throw new AccessViolationException("Cannot write outside of the buffer space");
            NativeMethods.memcpy(new IntPtr(this.Pointer.ToInt64() + Offset), new IntPtr(Source.ToInt64() + SourceOffset), (uint)SourceLength);
        }

        private bool CheckBoundries(int offset, int length)
        {
            return offset + length > TotalLength;
        }
    }
}