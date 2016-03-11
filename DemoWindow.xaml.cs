using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace ImageManipulator
{
    /// <summary>
    /// Interaction logic for DemoWindow.xaml
    /// </summary>
    public partial class DemoWindow : Window
    {
        public Dictionary<byte,ColorPixelRange> ColorPixelRanges { get; private set; }

        public DemoWindow()
        {
            ColorPixelRanges = new Dictionary<byte, ColorPixelRange>(2);
            InitializeComponent();
        }

        private void btnLoadImage_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog
            {
                Title = "Select Image to Optimize",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                         "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                         "Portable Network Graphic (*.png)|*.png"
            };
            if (op.ShowDialog() == true)
            {
                CurrImageBorder.Visibility = Visibility.Visible;
                CurrImage.Source = LoadBitMapImage(op.FileName);
            }
        }

        private BitmapImage LoadBitMapImage(string imgPath)
        {
            BitmapImage img = new BitmapImage();
            try
            {
                if (imgPath == "")
                    throw new NullReferenceException();
                using (FileStream fs = File.OpenRead(imgPath))
                {
                    img.BeginInit();
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.StreamSource = fs;
                    img.EndInit();
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(@"Could not load image because " + e.Message);
            }
            return img;
        }

        private void btnOptimizeImage_OnClick(object sender, RoutedEventArgs e)
        {
            OptimizeData();
        }

        private void btnResetImage_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrImage.Source != null)
            {
                CurrImage.Source = null;
                CurrImageBorder.Visibility = Visibility.Hidden;
            }
        }


        public class ColorPixelRange
        {
            public Byte byteValue;
            public List<int> chRed;
            public List<int> chGreen;
            public List<int> chBlue;
            public List<int> chAlpha;
            public ColorPixelRange(Byte byteVal)
            {
                byteValue = byteVal;
                chRed = new List<int>(10);
                chGreen = new List<int>(10);
                chBlue = new List<int>(10);
                chAlpha = new List<int>(10);
            }

            public override string ToString()
            {
                string retString = "" + byteValue + "";
                retString = chBlue.Aggregate(retString, (current, val) => current + string.Format("{0}", val));
                retString = chGreen.Aggregate(retString, (current, val) => current + string.Format("{0}", val));
                retString = chRed.Aggregate(retString, (current, val) => current + string.Format("{0}", val));
                retString = chAlpha.Aggregate(retString, (current, val) => current + string.Format("{0}", val));
                return retString;
            }
        }

        private void OptimizeData()
        {
            /*
             * TODO: The main algorithm to optimize the pixel data and store it in a custom format
             * suitable to be read by this program. I would call this as Serializing/De-serializing.
             * Algorithm: 
             *  - Create a table of r,g,b,a(alpha really not needed now) color values starting from 1-255 each.
             *  - Depending on the level of quality to preserve most of the values in each color channel would be an
             *    wrapped around the approximation value in the table.
             *  - Read each pixel in the image and extract these values out and map them out using a custom struct of a data.
             *  - For eg: A pixel at center of the screen with the color value of r=10,g=15,b=40; would have a struct containing the position number
             *    stored in the struct which maps to the index value of colors from the above color table.
             *    Here: The above pixel would have the struct form
             *    
             */

            /* Load the image data. */
            if (CurrImage.Source != null)
            {
                BitmapImage bmp = (BitmapImage) CurrImage.Source;
                int stride = (int)bmp.PixelWidth * (bmp.Format.BitsPerPixel / 8);//b,g,r,a-32bit
                byte[] pixels = new byte[(int)bmp.PixelHeight * stride];
                bmp.CopyPixels(pixels, stride, 0);
                int idx = 0;
                foreach (byte b in pixels)
                {
                    AddToColorArray(b, idx);
                    idx++;
                }
            }

            if (ColorPixelRanges.Any())
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CustomImage.txt");
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        foreach (var colorPixel in ColorPixelRanges)
                        {
                            sw.WriteLine(colorPixel.Value.ToString());
                        }
                    }
                }
            }
        }

        private void AddToColorArray(byte val, int _idx)
        {
            int idx = _idx;
            idx = idx % 4;//(bmp.Format.BitsPerPixel / 8)
            switch (idx)
            {
                case 0: //b
                        if (!ColorPixelRanges.ContainsKey(val))
                        {
                            ColorPixelRange pixRange = new ColorPixelRange(val);
                            pixRange.chBlue.Add(_idx);
                            ColorPixelRanges.Add(val, pixRange);
                        }
                        else
                        {
                            ColorPixelRange pixRange;
                            ColorPixelRanges.TryGetValue(val, out pixRange);
                            pixRange.chBlue.Add(_idx);
                            
                        }
                    break;
                case 1: //g
                        if (!ColorPixelRanges.ContainsKey(val))
                        {
                            ColorPixelRange pixRange = new ColorPixelRange(val);
                            pixRange.chGreen.Add(_idx);
                            ColorPixelRanges.Add(val, pixRange);
                        }
                        else
                        {
                            ColorPixelRange pixRange;
                            ColorPixelRanges.TryGetValue(val, out pixRange);
                            pixRange.chGreen.Add(_idx);
                            
                        }
                    break;
                case 2: //r
                        if (!ColorPixelRanges.ContainsKey(val))
                        {
                            ColorPixelRange pixRange = new ColorPixelRange(val);
                            pixRange.chRed.Add(_idx);
                            ColorPixelRanges.Add(val, pixRange);
                        }
                        else
                        {
                            ColorPixelRange pixRange;
                            ColorPixelRanges.TryGetValue(val, out pixRange);
                            pixRange.chRed.Add(_idx);
                            
                        }
                    break;
                case 3: //a
                        if (!ColorPixelRanges.ContainsKey(val))
                        {
                            ColorPixelRange pixRange = new ColorPixelRange(val);
                            pixRange.chAlpha.Add(_idx);
                            ColorPixelRanges.Add(val, pixRange);
                        }
                        else
                        {
                            ColorPixelRange pixRange;
                            ColorPixelRanges.TryGetValue(val, out pixRange);
                            pixRange.chAlpha.Add(_idx);
                            
                        }
                    break;
            }
        }

        private BitmapSource BitmapSourceFromArray(byte[] pixels, int width, int height)
        {
            WriteableBitmap bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * (bitmap.Format.BitsPerPixel / 8), 0);

            return bitmap;
        }
    }
}
