using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Svrin.Wpf.Helpers
{
    public static class ImageHelper
    {
        static public bool IsNullOrEmpty(BitmapImage? bitmapImage)
            => bitmapImage == null
            || bitmapImage.PixelWidth == 0
            || bitmapImage.PixelHeight == 0;

        static public bool TryGetBitmapSource(
            byte[]? bytes,
            int dimension,
            out BitmapSource? source,
            int dpi = 96)
            => TryGetBitmapSource(bytes, dimension, dimension, out source, dpi);

        static public bool TryGetBitmapSource(
            byte[]? bytes,
            int width,
            int height,
            out BitmapSource? source,
            int dpi = 96)
        {
            source = GetBitmapSource(bytes, width, height, dpi);
            return source != null;
        }

        static public BitmapSource? GetBitmapSource(
            byte[]? bytes,
            int dimension,
            int dpi = 96)
            => GetBitmapSource(bytes, dimension, dimension, dpi);

        static public BitmapSource? GetBitmapSource(
            byte[]? bytes,
            int width,
            int height,
            int dpi = 96)
        {
            if (bytes == null || bytes.Length == 0) return null;
            if (width <= 0 || height <= 0 || dpi <= 0) return null;
            try
            {
                BitmapSource source;
                using (var ms = new MemoryStream(bytes))
                {
                    source = BitmapFrame.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    source.Freeze();
                }

                var target = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var dc = visual.RenderOpen())
                {
                    dc.DrawImage(source, new Rect(0, 0, width, height));
                }
                target.Render(visual);
                target.Freeze();
                return target;
            }
            catch { }
            return null;
        }
        
        public static BitmapImage? GetBitmapImage(
            Bitmap bitmap,
            ImageFormat? format = null)
        {
            if (bitmap == null) return null;
            try
            {
                using var memory = new MemoryStream();
                format ??= ImageFormat.Png;
                bitmap.Save(memory, format);
                memory.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
            catch { }
            return null;
        }
        public static BitmapImage? GetBitmapImage(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            try
            {
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                return bitmap;
            }
            catch { }
            return null;
        }

        static public ImageSource? GetImageSource(byte[]? imageData) => GetBitmapImage(imageData);

        static public BitmapSource? GetBitmapSource(byte[]? imageData) => GetBitmapImage(imageData);

        static public bool TryGetImageSource(
            byte[]? imageData,
            out ImageSource? source)
        {
            source = GetBitmapImage(imageData);
            return source != null;
        }

        static public bool TryGetBitmapSource(
            byte[]? imageData,
            out BitmapSource? source)
        {
            source = GetBitmapImage(imageData);
            return source != null;
        }

        public static BitmapImage? ResizeFromStream(
            Stream stream,
            int dimension)
            => ResizeFromStream(stream, dimension, dimension);

        public static BitmapImage? ResizeFromStream(
            Stream stream,
            int width,
            int height)
        {
            if (stream == null || stream.Length == 0) return null;
            if (width <= 0 || height <= 0) return null;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = width;
                bitmap.DecodePixelHeight = height;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch { }
            return null;
        }

        public static BitmapImage? ResizeImage(
            BitmapImage? originalImage,
            int dimension)
            => ResizeImage(originalImage, dimension, dimension);

        public static BitmapImage? ResizeImage(
            BitmapImage? originalImage,
            int width,
            int height)
        {
            if (IsNullOrEmpty(originalImage)) return null;
            if (width <= 0 || height <= 0) return null;
            try
            {
                using var ms = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(originalImage));
                encoder.Save(ms);
                ms.Position = 0;

                return ResizeFromStream(ms, width, height);
            }
            catch { }
            return null;
        }

        public static BitmapSource? ResizeImage(
            string path,
            int dimension)
            => ResizeImage(path, dimension, dimension);

        public static BitmapSource? ResizeImage(
            string path,
            int width,
            int height)
        {
            if (!File.Exists(path)) return null;
            if (width <= 0 || height <= 0) return null;
            try
            {
                using var fs = File.OpenRead(path);
                return ResizeFromStream(fs, width, height);
            }
            catch { }
            return null;
        }
    }
}
