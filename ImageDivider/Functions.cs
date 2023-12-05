using System.Drawing;
using SharpAvi;
using SharpAvi.Output;
using SharpAvi.Codecs;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System;

namespace ImageDivider
{
    class Functions
    {
        static int frameWidth;
        static int frameHeight;
        static int fps = 1;
        static string imageName;

        public static Bitmap[,] CreateArrayFromImage(Image image, int _frameWidth, int _frameHeight, string _fileName)
        {
            frameWidth = _frameWidth;
            frameHeight = _frameHeight;
            imageName = _fileName;

            // pobranie wymiarów obrazu
            int width = image.Width;
            int height = image.Height;


            int columnsCount = width % frameWidth != 0 ? (width / frameWidth) + 1 : width / frameWidth;
            int rowsCount = height % frameHeight != 0 ? (height / frameHeight) + 1 : height / frameHeight;

            Bitmap[,] frames = new Bitmap[columnsCount,rowsCount];

            // iteracja po wszystkich częściach obrazu i zapisanie ich jako klatki filmu
            for (int row = 0; row < rowsCount; row++)
            {
                for (int col = 0; col < columnsCount; col++)
                {
                    // obliczenie współrzędnych lewego górnego rogu aktualnej części obrazu
                    int x = col * frameWidth;
                    int y = row * frameHeight;

                    // stworzenie nowego bitmapy, która będzie przechowywać aktualną część obrazu
                    Bitmap frame = new Bitmap(frameWidth, frameHeight);

                    // skopiowanie pikseli z oryginalnego obrazu do nowej bitmapy
                    using (Graphics g = Graphics.FromImage(frame))
                    {
                        g.DrawImage(image, new Rectangle(0, 0, frame.Width, frame.Height), new Rectangle(x, y, frameWidth, frameHeight), GraphicsUnit.Pixel);
                    }

                    frames[col,row]=frame;
                }
            }
            return frames;
        }
        public static void SaveFramesToFilm(Bitmap[] frames, string algoritm)
        {
            var framesList = new List<Bitmap>();

            foreach (var frame in frames)
            {
                if (!CheckIfFrameIsBlack(frame))
                    framesList.Add(frame);
            }

            frames = framesList.ToArray();

            string dicionaryName = string.Concat(frameWidth.ToString(),"x",frameHeight.ToString());
            string dictionaryImageName = string.Concat(dicionaryName, "/", imageName);
            //string dictionaryImageName = string.Concat(dicionaryName, "/", imageName,"/", algoritm);
            string fileName = string.Concat(algoritm,".avi");
            string path = string.Concat(dictionaryImageName, "/", fileName);
            if (!Directory.Exists(dictionaryImageName))
            {
                Directory.CreateDirectory(dictionaryImageName);
            }
            /*int i = 0;
            foreach (var frame in frames)
            {
                frame.Save(string.Concat(path,"/",i,".bmp"),ImageFormat.Bmp);
                i++;
            }*/

            var writer = new AviWriter(path)
            {
                FramesPerSecond = fps,
                // Emitting the AVI v1 index in addition to the OpenDML index (AVI v2)
                // improves compatibility with some software, including 
                // standard Windows programs like Media Player and File Explorer
                EmitIndex1 = true
            };

            var stream = writer.AddUncompressedVideoStream(frames[0].Width, frames[0].Height);

            // set standard VGA resolution
            //stream.Width = frames[0].Width;
            //stream.Height = frames[0].Height;
            // class SharpAvi.CodecIds contains FOURCCs for several well-known codecs
            // Uncompressed is the default value, just set it for clarity
            //stream.Codec = CodecIds.Uncompressed;
            // Uncompressed format requires to also specify bits per pixel
            //stream.BitsPerPixel = BitsPerPixel.Bpp8;

            var frameData = new byte[stream.Width * stream.Height * 4];
            foreach (var frame in frames)
            {
                var bits = frame.LockBits(new Rectangle(0, 0, frames[0].Width, frames[0].Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                Marshal.Copy(bits.Scan0, frameData, 0, frameData.Length);
                frame.UnlockBits(bits);

                // write data to a frame
                stream.WriteFrame(true, frameData, 0, frameData.Length);
            }

            writer.Close();
            
        }
        public static bool IsPowerOfTwo(int n)
        {
            if (n == 0)
                return false;
            while (n != 1)
            {
                if (n % 2 != 0)
                    return false;
                n = n / 2;
            }
            return true;
        }
        public static bool IsPowerOfThree(int n)
        {
            if (n == 0)
                return false;
            while (n != 1)
            {
                if (n % 3 != 0)
                    return false;
                n = n / 3;
            }
            return true;
        }

        private static bool CheckIfFrameIsBlack(Bitmap frame)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, frame.Width, frame.Height);
            BitmapData bmpData = frame.LockBits(rect, ImageLockMode.ReadWrite, frame.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * frame.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Scanning for non-zero bytes
            bool allBlack = true;
            for (int index = 0; index < rgbValues.Length; index++)
                if (rgbValues[index] != 0)
                {
                    allBlack = false;
                    break;
                }
            // Unlock the bits.
            frame.UnlockBits(bmpData);

            return allBlack;
        }
        public static void SetFps(int _fps)
        {
            fps = _fps;
        }
    }
}
