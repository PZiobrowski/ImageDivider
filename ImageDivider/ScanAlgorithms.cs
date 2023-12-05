using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageDivider
{
    class ScanAlgorithms
    {
        public static void ColumnAfterColumn(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];

            for (int row = 0; row < frames.GetLength(0); row++)
            {
                for (int col = 0; col < frames.GetLength(1); col++)
                {
                    resultArray[col+row* frames.GetLength(1)] = frames[row, col];
                }
            }

            Functions.SaveFramesToFilm(resultArray, "ColumnAfterColumn");
        }
        public static void ColumnAfterColumnSpiral(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];

            for (int row = 0; row < frames.GetLength(0); row++)
            {
                if(row%2==0)
                {
                    for (int col = 0; col < frames.GetLength(1); col++)
                        resultArray[col + row * frames.GetLength(1)] = frames[row, col];
                }else
                {
                    for (int col = frames.GetLength(1) - 1; col >= 0; col--)
                        resultArray[frames.GetLength(1) - 1 - col + row * frames.GetLength(1)] = frames[row, col];
                }
            }
            Functions.SaveFramesToFilm(resultArray, "ColumnAfterColumnSpiral");
        }
        public static void RowAfterRow(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];

            for (int col = 0; col < frames.GetLength(1); col++)
            {
                for (int row = 0; row < frames.GetLength(0); row++)
                {
                    resultArray[row + col * frames.GetLength(0)] = frames[row, col];
                }
            }
            Functions.SaveFramesToFilm(resultArray, "RowAfterRow");
        }
        public static void RowAfterRowSpiral(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];

            for (int col = 0; col < frames.GetLength(1); col++)
            {
                if (col % 2 == 0)
                {
                    for (int row = 0; row < frames.GetLength(0); row++)
                        resultArray[row + col * frames.GetLength(0)] = frames[row, col];
                }
                else
                {
                    for (int row = frames.GetLength(0) - 1; row >= 0; row--)
                        resultArray[frames.GetLength(0) - 1 - row + col * frames.GetLength(0)] = frames[row, col];
                }
            }
            Functions.SaveFramesToFilm(resultArray, "RowAfterRowSpiral");
        }
        public static void Spiral(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];

            int startX = 0;
            int startY = 0;
            int endX = frames.GetLength(1);
            int endY = frames.GetLength(0);
            int iter = 0;

            while (startX < endX && startY < endY)
            {
                for (int i = startX; i < endX; i++)
                {
                    resultArray[iter] = frames[startY, i];
                    iter++;
                }
                startY++;

                for (int i = startY; i < endY; i++)
                {
                    resultArray[iter] = frames[i, endX - 1];
                    iter++;
                }
                endX--;

                if (startY < endY)
                {
                    for (int i = endX - 1; i >= startX; i--)
                    {
                        resultArray[iter] = frames[endY - 1, i];
                        iter++;
                    }
                    endY--;
                }

                if (startX < endX)
                {
                    for (int i = endY - 1; i >= startY; i--)
                    {
                        resultArray[iter] = frames[i, startX];
                        iter++;
                    }
                    startX++;
                }
            }
            Functions.SaveFramesToFilm(resultArray, "Spiral");
        }
        public static void Diagonal(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];
            int iter = 0;

            int rows = frames.GetLength(0);
            int cols = frames.GetLength(1);

            for (int k = 0; k < rows + cols - 1; k++)
            {
                for (int i = 0; i <= k; i++)
                {
                    int j = k - i;
                    if (i < rows && j < cols)
                    {
                        resultArray[iter] = frames[i, j];
                        iter++;
                    }
                }
            }
            Functions.SaveFramesToFilm(resultArray, "Diagonal");
        }
        public static void Meander(Bitmap[,] frames)
        {
            Bitmap[] resultArray = new Bitmap[frames.Length];

            int rows = frames.GetLength(0);
            int columns = frames.GetLength(1);
            int iter = 0;

            for (int i = 0; i < rows + columns - 1; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = Math.Min(i, columns - 1); j >= Math.Max(0, i - rows + 1); j--)
                    {
                        resultArray[iter] = frames[i - j, j];
                        iter++;
                    }
                }else
                {
                    for (int j = Math.Max(0, i - rows + 1); j <= Math.Min(i, columns - 1); j++)
                    {
                        resultArray[iter] = frames[i - j, j];
                        iter++;
                    }
                }
            }
            Functions.SaveFramesToFilm(resultArray, "Meander");
        }
        public static void Z_Curve(Bitmap[,] frames)
        {
            int rows = frames.GetLength(0);
            int cols = frames.GetLength(1);

            resizeArray2(rows, cols, ref frames);

            int iteration = (int)Math.Log2(Convert.ToDouble(frames.GetLength(0)));

            Z_Curve z_curve = new Z_Curve(iteration, frames);

            var resultArray = z_curve.GetScanedArray();

            Functions.SaveFramesToFilm(resultArray, "Z_Curve");
        }
        public static void Hilbert(Bitmap[,] frames)
        {
            int rows = frames.GetLength(0);
            int cols = frames.GetLength(1);

            resizeArray2(rows, cols, ref frames);

            var bitDepth = (int)Math.Log2(frames.GetLength(0)) + 1;
            var hilbertScan = new HilbertScan(bitDepth);

            var resultArray = hilbertScan.GetScanedArray(frames);

            Functions.SaveFramesToFilm(resultArray, "Hilbert");
        }
        public static void PeanoMeander(Bitmap[,] frames)
        {
            int rows = frames.GetLength(0);
            int cols = frames.GetLength(1);

            resizeArray3(rows, cols, ref frames);

            PeanoMeanderScan peanoScan = new PeanoMeanderScan(frames);

            var resultArray = peanoScan.GetScanedArray();
            Functions.SaveFramesToFilm(resultArray, "PeanoMeander");
        }

        private static void resizeArray2(int rows, int cols, ref Bitmap [,] frames)
        {
            int newDimension = 0;
            bool resize = false;

            if (rows > cols && Functions.IsPowerOfTwo(rows))
            {
                newDimension = rows;
                resize = true;
            }
            else if (cols > rows && Functions.IsPowerOfTwo(cols))
            {
                newDimension = cols;
                resize = true;
            }
            else if (rows >= cols && !Functions.IsPowerOfTwo(rows))
            {
                newDimension = Convert.ToInt32(Math.Pow(2, Math.Ceiling(Math.Log(rows) / Math.Log(2))));
                resize = true;
            }
            else if (cols >= rows && !Functions.IsPowerOfTwo(cols))
            {
                newDimension = Convert.ToInt32(Math.Pow(2, Math.Ceiling(Math.Log(cols) / Math.Log(2))));
                resize = true;
            }

            if (resize)
            {
                var newArray = new Bitmap[newDimension, newDimension];
                int columnCount = cols;
                int columnCount2 = newDimension;
                int columns = frames.GetUpperBound(0);
                for (int co = 0; co <= columns; co++)
                    Array.Copy(frames, co * columnCount, newArray, co * columnCount2, columnCount);

                for (int col = 0; col < newDimension; col++)
                {
                    for (int row = 0; row < newDimension; row++)
                    {
                        if (newArray[row, col] == null)
                        {
                            Image resultImage = new Bitmap(newArray[0, 0].Width, newArray[0, 0].Height, PixelFormat.Format24bppRgb);

                            using (Graphics grp = Graphics.FromImage(resultImage))
                            {
                                grp.FillRectangle(Brushes.Black, 0, 0, newArray[0, 0].Width, newArray[0, 0].Height);
                            }

                            newArray[row, col] = (Bitmap)resultImage;
                        }
                    }
                }
                frames = newArray;
            }
        }

        private static void resizeArray3(int rows, int cols, ref Bitmap[,] frames)
        {
            int newDimension = 0;
            bool resize = false;

            if (rows > cols && Functions.IsPowerOfThree(rows))
            {
                newDimension = rows;
                resize = true;
            }
            else if (cols > rows && Functions.IsPowerOfThree(cols))
            {
                newDimension = cols;
                resize = true;
            }
            else if (rows >= cols && !Functions.IsPowerOfThree(rows))
            {
                newDimension = Convert.ToInt32(Math.Pow(3, Math.Ceiling(Math.Log(rows) / Math.Log(3))));
                resize = true;
            }
            else if (cols >= rows && !Functions.IsPowerOfThree(cols))
            {
                newDimension = Convert.ToInt32(Math.Pow(3, Math.Ceiling(Math.Log(cols) / Math.Log(3))));
                resize = true;
            }

            if (resize)
            {
                var newArray = new Bitmap[newDimension, newDimension];
                int columnCount = cols;
                int columnCount2 = newDimension;
                int columns = frames.GetUpperBound(0);
                for (int co = 0; co <= columns; co++)
                    Array.Copy(frames, co * columnCount, newArray, co * columnCount2, columnCount);

                for (int col = 0; col < newDimension; col++)
                {
                    for (int row = 0; row < newDimension; row++)
                    {
                        if (newArray[row, col] == null)
                        {
                            Image resultImage = new Bitmap(newArray[0, 0].Width, newArray[0, 0].Height, PixelFormat.Format24bppRgb);

                            using (Graphics grp = Graphics.FromImage(resultImage))
                            {
                                grp.FillRectangle(Brushes.Black, 0, 0, newArray[0, 0].Width, newArray[0, 0].Height);
                            }

                            newArray[row, col] = (Bitmap)resultImage;
                        }
                    }
                }
                frames = newArray;
            }
        }
    }
}
