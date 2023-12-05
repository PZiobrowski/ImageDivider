using System.Drawing;

namespace ImageDivider
{
    class Z_Curve
    {
        private int iteration;
        Bitmap[] resultArray;
        Bitmap[,] frames;
        int index;

        public Z_Curve(int iteration, Bitmap[,] frames)
        {
            this.iteration = iteration;        
            resultArray = new Bitmap[frames.Length];
            index = 0;
            this.frames = frames; 
        }

        private void GenerateCurve(int x1, int y1, int x2, int y2, int depth)
        {
            if (depth == iteration)
            {
                resultArray[index++] = frames[y1, x1];
                return;
            }

            int midX = (x1 + x2) / 2;
            int midY = (y1 + y2) / 2;

            GenerateCurve(x1, y1, midX, midY, depth + 1);
            GenerateCurve(midX + 1, y1, x2, midY, depth + 1);
            GenerateCurve(x1, midY + 1, midX, y2, depth + 1);
            GenerateCurve(midX + 1, midY + 1, x2, y2, depth + 1);
        }

        public Bitmap[] GetScanedArray()
        {
            int size = frames.GetLength(0);
            GenerateCurve(0, 0, size - 1 , size - 1, 0);
            return resultArray;
        }
    }
}
