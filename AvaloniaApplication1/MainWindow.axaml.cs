using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Platform;
using System;

namespace AvaloniaApplication1
{
    public class MainWindow : Window
    {
        private int height;
        private int width;
        private byte[] bigImage;
        private Stack st = new Stack();
        private Image myImage;
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            myImage = this.FindControl<Image>("myImage");
            System.Drawing.Bitmap draw = new System.Drawing.Bitmap("C:/Users/WINDOWS10X/Desktop/20200718081251.jpg", true);
            byte[] rawImage = get_data(draw);
            //myImage.Source = set_data(rawImage, draw.Width, draw.Height);

            int h = draw.Height;
            int w = draw.Width;
            int rawStride = w * 3;

            height = 810 / 12 * 12;
            width = 810 / 12 * 12;
            bigImage = new byte[height * width * 3];
            for (int i = 0; i < height * width; i++)
            {
                int row2 = i / width;
                int col2 = i % width;
                int row1 = (int)(row2 * 1.0 / (height - 1) * (h - 1));
                int col1 = (int)(col2 * 1.0 / (width - 1) * (w - 1));
                bigImage[row2 * (width * 3) + col2 * 3 + 0] = rawImage[row1 * w * 3 + col1 * 3 + 0];
                bigImage[row2 * (width * 3) + col2 * 3 + 1] = rawImage[row1 * w * 3 + col1 * 3 + 1];
                bigImage[row2 * (width * 3) + col2 * 3 + 2] = rawImage[row1 * w * 3 + col1 * 3 + 2];
            }
            myImage.Source = set_data(bigImage, width, height);
            st = new Stack();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        static public bool is_zhi(int a)
        {
            for (int i = 2; i < a; i++)
            {
                if (a % 2 == 0)
                {
                    return false;
                }
            }
            return true;
        }
        static public void rotate_imageroi(ref byte[] img_ptr, int h, int w, int index)
        {
            int roih = h / 3 * 2;
            int roiw = w / 3 * 2;
            int roix = 0;
            int roiy = 0;
            switch (index)
            {
                case 1:
                    roix = w / 3;
                    roiy = 0;
                    break;
                case 2:
                    roiy = h / 3;
                    roix = 0;
                    break;
                case 3:
                    roix = w / 3;
                    roiy = h / 3;
                    break;
            }
            rotate_roi(ref img_ptr, h, w, roix, roiy, roiw, roih);
        }
        static public void rotate_roi(ref byte[] img_ptr, int h, int w, int roix, int roiy, int roiw, int roih)
        {
            byte[] tmp_img = new byte[roiw * roih * 3];
            for (int row = 0; row < roih; row++)
            {
                for (int col = 0; col < roiw; col++)
                {
                    int dsty = row - roih / 2;
                    int dstx = col - roiw / 2;
                    int srcx = dsty;
                    int srcy = -dstx;
                    int row2 = srcy + roih / 2;
                    int col2 = srcx + roiw / 2;
                    int row3 = row2 + roiy;
                    row3 = row3 > h - 1 ? h - 1 : row3;
                    int col3 = col2 + roix;
                    col3 = col3 > w - 1 ? w - 1 : col3;
                    tmp_img[row * roiw * 3 + col * 3 + 0] = img_ptr[row3 * w * 3 + col3 * 3 + 0];
                    tmp_img[row * roiw * 3 + col * 3 + 1] = img_ptr[row3 * w * 3 + col3 * 3 + 1];
                    tmp_img[row * roiw * 3 + col * 3 + 2] = img_ptr[row3 * w * 3 + col3 * 3 + 2];
                }
            }
            for (int row = 0; row < roih; row++)
            {
                for (int col = 0; col < roiw; col++)
                {
                    int row3 = row + roiy;
                    int col3 = col + roix;
                    img_ptr[row3 * w * 3 + col3 * 3 + 0] = tmp_img[row * roiw * 3 + col * 3 + 0];
                    img_ptr[row3 * w * 3 + col3 * 3 + 1] = tmp_img[row * roiw * 3 + col * 3 + 1];
                    img_ptr[row3 * w * 3 + col3 * 3 + 2] = tmp_img[row * roiw * 3 + col * 3 + 2];
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int flag = -1;
            switch (e.Key)
            {
                case Key.Q:
                    flag = 0;
                    break;
                case Key.W:
                    flag = 1;
                    break;
                case Key.A:
                    flag = 2;
                    break;
                case Key.S:
                    flag = 3;
                    break;
            }
            if (flag >= 0)
            {
                st.Push(flag);
                st.Push(flag);
                st.Push(flag);
            }
            if (e.Key == Key.B && st.Count > 0)
            {
                flag = (int)st.Pop();
            }
            if (flag >= 0)
            {
                rotate_imageroi(ref bigImage, height, width, flag);
                myImage.Source = set_data(bigImage, width, height);
            }
        }

        static WriteableBitmap imread(string name) {
            
            System.Drawing.Bitmap draw = new System.Drawing.Bitmap(name, true);
            WriteableBitmap bmp = new WriteableBitmap(new PixelSize(draw.Width, draw.Height), new Vector(96, 96), PixelFormat.Rgba8888);
            int stride = draw.Width * 4;
            using (ILockedFramebuffer fb = bmp.Lock())
            {
                for (int x = 0; x < draw.Width; x++)
                {
                    for (int y = 0; y < draw.Height; y++)
                    {
                        System.Drawing.Color pixelColor = draw.GetPixel(x, y);
                        unsafe
                        {
                            byte* data = (byte*)fb.Address;
                            data[y * stride + x * 4] = pixelColor.R;
                            data[y * stride + x * 4 + 1] = pixelColor.G;
                            data[y * stride + x * 4 + 2] = pixelColor.B;
                            data[y * stride + x * 4 + 3] = 255;
                        }
                    }
                }
            }
            return bmp;
        }

        static byte[] get_data(System.Drawing.Bitmap draw)
        {
            byte[] buffer = new byte[draw.Width*draw.Height*3];
            int stride = draw.Width * 3;
            for (int x = 0; x < draw.Width; x++)
            {
                for (int y = 0; y < draw.Height; y++)
                {
                    System.Drawing.Color pixelColor = draw.GetPixel(x, y);
                    buffer[y * stride + x * 3] = pixelColor.R;
                    buffer[y * stride + x * 3 + 1] = pixelColor.G;
                    buffer[y * stride + x * 3 + 2] = pixelColor.B;
                }
            }
            return buffer;
        }
        static WriteableBitmap set_data(byte[] buffer, int width, int height)
        {

            WriteableBitmap bmp = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Rgba8888);
            int stride = width * 4;
            using (ILockedFramebuffer fb = bmp.Lock())
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        unsafe
                        {
                            byte* data = (byte*)fb.Address;
                            data[y * stride + x * 4] = buffer[y * width*3 + x*3 + 0];
                            data[y * stride + x * 4 + 1] = buffer[y * width * 3 + x * 3 + 1];
                            data[y * stride + x * 4 + 2] = buffer[y * width * 3 + x * 3 + 2];
                            data[y * stride + x * 4 + 3] = 255;
                        }
                    }
                }
            }
            return bmp;
        }

    }
}
