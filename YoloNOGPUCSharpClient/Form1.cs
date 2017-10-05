using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp1.YoloBridge;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Open File";
            openFileDialog1.Filter = "Image Files (*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.Text = "";
                label1.Visible = true;

                pictureBox1.Visible = true;
                Bitmap bmp = new Bitmap(openFileDialog1.FileName);
                Bitmap bmpResized = new Bitmap(bmp, new Size(608, 608));
                textBox1.Visible = true;
                textBox1.Text = openFileDialog1.FileName;

                YoloBridge yoloBridge = new YoloBridge();
                bbox_t[] bbox = yoloBridge.detect(bmpResized);                
                richTextBox1.Visible = true;
                for (int i = 0; i < bbox.Length; i++)
                {                    
                    richTextBox1.Text += bbox[i].ToString() + "\r\n";
                    pictureBox1.Image = yoloBridge.DrawRectangle(bmpResized, bbox[i]);

                }
                pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            }            
        }
    }
}
