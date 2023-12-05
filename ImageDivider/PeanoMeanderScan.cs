using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDivider
{
    class PeanoMeanderScan
    {
        Bitmap[,] frames;
        int dimension;
        int index;
        Bitmap[] resultArray;
        public PeanoMeanderScan(Bitmap[,] frames)
        {
            this.frames = frames;
            dimension = frames.GetLength(0);
            index = 0;
        }

        public Bitmap[] GetScanedArray()
        {
            resultArray = new Bitmap[frames.Length];
            int rows = frames.GetLength(0);
            int cols = frames.GetLength(1);

            Peano(0, 0, dimension, 0, 0);

            return resultArray;
        }

        void Peano(int x, int y, int lg, int i1, int i2)
        {

            if (lg == 1)
            {
                resultArray[index] = frames[x, y];
                index++;
                return;
            }

            lg = lg / 3;
            Peano(x + (2 * i1 * lg), y + (2 * i1 * lg), lg, i1, i2);
            Peano(x + ((i1 - i2 + 1) * lg), y + ((i1 + i2) * lg), lg, i1, 1 - i2);
            Peano(x + lg, y + lg, lg, i1, 1 - i2);
            Peano(x + ((i1 + i2) * lg), y + ((i1 - i2 + 1) * lg), lg, 1 - i1, 1 - i2);
            Peano(x + (2 * i2 * lg), y + (2 * (1 - i2) * lg), lg, i1, i2);
            Peano(x + ((1 + i2 - i1) * lg), y + ((2 - i1 - i2) * lg), lg, i1, i2);
            Peano(x + (2 * (1 - i1) * lg), y + (2 * (1 - i1) * lg), lg, i1, i2);
            Peano(x + ((2 - i1 - i2) * lg), y + ((1 + i2 - i1) * lg), lg, 1 - i1, i2);
            Peano(x + (2 * (1 - i2) * lg), y + (2 * i2 * lg), lg, 1 - i1, i2);
        }
    }
}
