using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GaussianBlur;

namespace ClusterCompressor
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Bitmap bitmap = new Bitmap(@"C:\Users\Admin\source\repos\ClusterCompressor\test.bmp");
            Form form = new Form();
            form.Text = "Image Viewer";
            
            PictureBox pictureBox = new PictureBox();
            pictureBox.Image = GaussianBlurer.Convolve(bitmap, GaussianBlurer.ApplyGaussianBlur(20, 20));
            pictureBox.Dock = DockStyle.Fill;
            form.Controls.Add(pictureBox);
            Application.Run(form);
        }
    }
}
