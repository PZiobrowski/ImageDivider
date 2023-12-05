using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ImageDivider
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("Incorrect arguments");
                return;
            }

            if (args.Length == 3)
                Functions.SetFps(Int16.Parse(args[2]));

            var ImagesCount = Directory.GetFiles(@"..\..\Images").Length;
            var fileNames = Directory.GetFiles(@"..\..\Images").ToList().Select(file => new FileInfo(file).Name).ToArray();

            // załadowanie obrazu z pliku
            for (int i = 0; i<ImagesCount; i++)
            {
                Image image = Image.FromFile(Directory.GetFiles(@"..\..\Images")[i]);
                string filename = Path.GetFileNameWithoutExtension(fileNames[i]);
                Bitmap[,] frames = Functions.CreateArrayFromImage(image, Int16.Parse(args[0]), Int16.Parse(args[1]), filename);
               
                ScanAlgorithms.RowAfterRow(frames);
                ScanAlgorithms.RowAfterRowSpiral(frames);
                ScanAlgorithms.ColumnAfterColumn(frames);
                ScanAlgorithms.ColumnAfterColumnSpiral(frames);
                ScanAlgorithms.Spiral(frames);
                ScanAlgorithms.Diagonal(frames);
                ScanAlgorithms.Meander(frames);
                ScanAlgorithms.Z_Curve(frames);
                ScanAlgorithms.Hilbert(frames);
                ScanAlgorithms.PeanoMeander(frames);
                              
            }
        }
    }
}
