using System;
using System.Drawing;
using System.Numerics;

namespace ImageDivider
{
    public class HilbertScan
    {
        /// <summary>
        /// Number of bits to use to encode each dimension.
        /// </summary>
        private readonly int BitDepth;

        /// <summary>
        /// Number of dimensions in the vector.
        /// </summary>
        private readonly int Dimensions;

        /// <summary>
        /// Number of Bits = BitDepth x Dimensions.
        /// </summary>
        private readonly int Bits;

        /// <summary>
        /// Number of full bytes needed to hold the interleaved data = Bits >> 3.
        /// This excludes the extra byte needed to hold the remaining bits that don't add up to a whole byte.
        /// </summary>
        private readonly int BytesNeeded;
        private Indices[] PrecomputedIndices;

        public HilbertScan(int bitDepth)
        {
            Dimensions = 2;
            BitDepth = bitDepth;
            Bits = Dimensions * BitDepth;
            BytesNeeded = Bits >> 3; // If we have 7 bits, 7 >> 3 is zero. We will add one byte to this to handle the extra bits.

            // When we create the BigInteger, it expects little-endian ordering, but the
            // input comes in big-endian order.
            PrecomputedIndices = new Indices[Bits];
            var iBit = 0;
            for (byte iSourceBit = 0; iSourceBit < BitDepth; iSourceBit++)
                for (var iSourceInt = Dimensions - 1; iSourceInt >= 0; iSourceInt--)
                {
                    var iTargetByte = (short)(iBit >> 3);
                    var iTargetBit = (byte)(iBit % 8);

                    PrecomputedIndices[iBit] = new Indices(
                        iSourceInt,
                        iSourceBit,
                        iTargetByte,
                        iTargetBit
                    );
                    iBit++;
                }
            Comparison<Indices> sortByTargetByteAndBit = (a, b) =>
            {
                if (a.iToByteVector < b.iToByteVector) return -1;
                if (a.iToByteVector > b.iToByteVector) return 1;
                if (a.iToByteBit < b.iToByteBit) return -1;
                if (a.iToByteBit > b.iToByteBit) return 1;
                return 0;
            };
            Array.Sort(PrecomputedIndices, sortByTargetByteAndBit);
        }


        /// <summary>
        /// Given the axes (coordinates) of a point in N-Dimensional space, find the distance to that point along the Hilbert curve.
        /// That distance will be transposed; broken into pieces and distributed into an array.
        /// 
        /// The number of dimensions is the length of the hilbertAxes array.
        ///
        /// Note: In Skilling's paper, this function is called AxestoTranspose.
        /// </summary>
        /// <param name="hilbertAxes">Point in N-space.</param>
        /// <param name="bits">Depth of the Hilbert curve. If bits is one, this is the top-level Hilbert curve.</param>
        /// <returns>The Hilbert distance (or index) as a transposed Hilbert index.</returns>
        public int HilbertIndexTransposed(uint[] hilbertAxes)
        {
            var X = (uint[])hilbertAxes.Clone();
            var n = hilbertAxes.Length; // n: Number of dimensions
            uint M = 1U << (BitDepth - 1), P, Q, t;
            int i;
            // Inverse undo
            for (Q = M; Q > 1; Q >>= 1)
            {
                P = Q - 1;
                for (i = 0; i < n; i++)
                    if ((X[i] & Q) != 0)
                        X[0] ^= P; // invert
                    else
                    {
                        t = (X[0] ^ X[i]) & P;
                        X[0] ^= t;
                        X[i] ^= t;
                    }
            } // exchange
            // Gray encode
            for (i = 1; i < n; i++)
                X[i] ^= X[i - 1];
            t = 0;
            for (Q = M; Q > 1; Q >>= 1)
                if ((X[n - 1] & Q) != 0)
                    t ^= Q - 1;
            for (i = 0; i < n; i++)
                X[i] ^= t;


            var byteVector = new byte[BytesNeeded + 1]; // An extra byte is needed to hold the extra bits and a sign bit for the BigInteger.
            //var extraBits = Bits - BytesNeeded << 3;
            int iIndex = 0;
            var iByte = 0;
            for (; iByte < BytesNeeded; iByte++)
            {
                // Unroll the loop so we compute the bits for a whole byte at a time.
                uint bits = 0;
                var idx0 = PrecomputedIndices[iIndex];
                var idx1 = PrecomputedIndices[iIndex + 1];
                var idx2 = PrecomputedIndices[iIndex + 2];
                var idx3 = PrecomputedIndices[iIndex + 3];
                var idx4 = PrecomputedIndices[iIndex + 4];
                var idx5 = PrecomputedIndices[iIndex + 5];
                var idx6 = PrecomputedIndices[iIndex + 6];
                var idx7 = PrecomputedIndices[iIndex + 7];
                bits = (((X[idx0.iFromUintVector] >> idx0.iFromUintBit) & 1U))
                     | (((X[idx1.iFromUintVector] >> idx1.iFromUintBit) & 1U) << 1)
                     | (((X[idx2.iFromUintVector] >> idx2.iFromUintBit) & 1U) << 2)
                     | (((X[idx3.iFromUintVector] >> idx3.iFromUintBit) & 1U) << 3)
                     | (((X[idx4.iFromUintVector] >> idx4.iFromUintBit) & 1U) << 4)
                     | (((X[idx5.iFromUintVector] >> idx5.iFromUintBit) & 1U) << 5)
                     | (((X[idx6.iFromUintVector] >> idx6.iFromUintBit) & 1U) << 6)
                     | (((X[idx7.iFromUintVector] >> idx7.iFromUintBit) & 1U) << 7);
                byteVector[iByte] = (byte)bits;
                iIndex += 8;
            }
            for (; iIndex < PrecomputedIndices.Length; iIndex++)
            {
                var idx = PrecomputedIndices[iIndex];
                var bit = (byte)(((X[idx.iFromUintVector] >> idx.iFromUintBit) & 1U) << idx.iToByteBit);
                byteVector[idx.iToByteVector] |= bit;
            }
            var bigIntResult =  new BigInteger(byteVector);
            return (int)bigIntResult;
        }

        public Bitmap[] GetScanedArray(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];
            int rows = frames.GetLength(0);
            int cols = frames.GetLength(1);

            for (uint row = 0; row < rows; row++)
            {
                for (uint col = 0; col < cols; col++)
                {
                    uint[] punkt = {row, col};
                    var index = HilbertIndexTransposed(punkt);
                    resultArray.SetValue(frames[row, col], index);
                }
            }
            return resultArray;
        }
    }

    struct Indices
    {
        /// <summary>
        /// Index into source vector of source uint to read.
        /// </summary>
        public readonly int iFromUintVector;

        /// <summary>
        /// Index into target vector of target byte to write.
        /// </summary>
        public readonly short iToByteVector;

        /// <summary>
        /// Index into source uint of source bit to read.
        /// </summary>
        public readonly byte iFromUintBit;

        /// <summary>
        /// Index into target byte of target bit to write.
        /// </summary>
        public readonly byte iToByteBit;

        public Indices(int fromUintVector, byte fromUintBit, short toByteVector, byte toByteBit)
        {
            iFromUintVector = fromUintVector;
            iFromUintBit = fromUintBit;
            iToByteVector = toByteVector;
            iToByteBit = toByteBit;
        }

        public override string ToString()
        {
            return string.Format("Byte.Bit from {0}.{1} to {2}.{3}", iFromUintVector, iFromUintBit, iToByteVector, iToByteBit);
        }
    }
}
