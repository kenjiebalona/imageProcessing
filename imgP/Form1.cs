using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;

namespace imgP
{
    public partial class Form1 : Form
    {

        Bitmap loadImage, resultImage, imageA, imageB, subImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void OpenFile(PictureBox picBox, ref Bitmap loadimg)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if(openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    loadimg = new Bitmap(openFileDialog.FileName);
                    picBox.Image = loadimg;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void ApplyImageProcessing(Action<Color, int, int> imageProcessingAction)
        {
            try
            {
                if(loadImage != null)
                {
                    resultImage = new Bitmap(loadImage.Width, loadImage.Height);

                    for(int i = 0; i < loadImage.Width; i++)
                    {
                        for(int j = 0; j < loadImage.Height; j++)
                        {
                            Color color = loadImage.GetPixel(i, j);
                            imageProcessingAction(color, i, j);
                        }
                    }

                    pictureBox2.Image = resultImage;
                }
                else
                {
                    MessageBox.Show("Load an image first");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void CopyImage()
        {
            ApplyImageProcessing((color, i, j) =>
            {
                resultImage.SetPixel(i, j, color);
            });
        }

        private void ConvertToGreyscale()
        {
            ApplyImageProcessing((color, i, j) =>
            {
                int greyscale = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                resultImage.SetPixel(i, j, Color.FromArgb(greyscale, greyscale, greyscale));
            });
        }

        private void InvertColors()
        {
            ApplyImageProcessing((color, i, j) =>
            {
                resultImage.SetPixel(i, j, Color.FromArgb(255 - color.R, 255 - color.G, 255 - color.B));
            });
        }

        private void Histogram()
        {
            try
            {
                if (loadImage != null)
                {
                    Bitmap grayscaleImage = new Bitmap(loadImage.Width, loadImage.Height);

                    ApplyImageProcessing((color, i, j) =>
                    {
                        int greyscale = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                        grayscaleImage.SetPixel(i, j, Color.FromArgb(greyscale, greyscale, greyscale));
                    });

                    int[] histogram = new int[256];

                    for (int i = 0; i < grayscaleImage.Width; i++)
                    {
                        for (int j = 0; j < grayscaleImage.Height; j++)
                        {
                            Color colorHist = grayscaleImage.GetPixel(i, j);
                            histogram[colorHist.R]++;
                        }
                    }

                    Bitmap histVisual = new Bitmap(256, 800);

                    for (int i = 0; i < histVisual.Width; i++)
                    {
                        for (int j = 0; j < histVisual.Height; j++)
                        {
                            histVisual.SetPixel(i, j, Color.White);
                        }
                    }

                    for (int i = 0; i < histVisual.Width; i++)
                    {
                        for (int j = 0; j < Math.Min(histogram[i] / 5, 800); j++)
                        {
                            histVisual.SetPixel(i, 799 - j, Color.Black);
                        }
                    }

                    pictureBox2.Image = histVisual;
                }
                else
                {
                    MessageBox.Show("Load an image first");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void ApplySepiaFilter()
        {
            ApplyImageProcessing((color, i, j) =>
            {
                int red = (int)(color.R * 0.393 + color.G * 0.769 + color.B * 0.189);
                int green = (int)(color.R * 0.349 + color.G * 0.686 + color.B * 0.168);
                int blue = (int)(color.R * 0.272 + color.G * 0.534 + color.B * 0.131);

                red = Math.Min(red, 255);
                green = Math.Min(green, 255);
                blue = Math.Min(blue, 255);

                resultImage.SetPixel(i, j, Color.FromArgb(red, green, blue));
            });
        }

        private void Subtraction()
        {
            try
            {
                if(imageA != null && imageB != null)
                {
                    subImage = new Bitmap(imageA.Width, imageA.Height);
                    Color green = Color.FromArgb(0, 0, 255);
                    int greyGreen = (int)(green.R + green.G + green.B) / 3;
                    int threshhold = 10;

                    for(int i = 0; i < imageA.Width; i++)
                    {
                        for(int j = 0; j < imageA.Height; j++)
                        {
                            Color pixel = imageA.GetPixel(i, j);
                            Color backpixel = imageB.GetPixel(i, j);
                            int grey = (int)(pixel.R + pixel.G + pixel.B) / 3;
                            int subtractValue = Math.Abs(grey - greyGreen);
                            if (subtractValue < threshhold)
                                subImage.SetPixel(i, j, backpixel);
                            else
                                subImage.SetPixel(i, j, pixel);
                            
                        }
                    }
                    pictureBox3.Image = subImage;
                }

            } catch(Exception ex)
            {
                MessageBox.Show("An error occurred while performing image subtraction: " + ex.Message);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) => CopyImage();
        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e) => ConvertToGreyscale();
        private void invertToolStripMenuItem_Click(object sender, EventArgs e) => InvertColors();
        private void histogramToolStripMenuItem_Click(object sender, EventArgs e) => Histogram();
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFile(pictureBox1, ref imageA);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFile(pictureBox2, ref imageB);
        }

        private void button3_Click(object sender, EventArgs e) => Subtraction();

        private void webcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Device[] allDevices = DeviceManager.GetAllDevices[];

                if(allDevices.Length > 0)
                {
                    Device firstDevice = allDevices[0];
                    firstDevice.ShowWindow(pictureBox1);
                }
                else
                {
                    MessageBox.Show("No webcam devices found.")
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in initializeing the webcam" + ex.Message);
            }
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e) => ApplySepiaFilter();

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "JPEG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp";
            saveFileDialog1.Title = "Save an Image File";

            if (resultImage != null)
            {
                if(saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string extension = Path.GetExtension(saveFileDialog1.FileName);

                    if (string.IsNullOrEmpty(extension))
                    {
                        switch (saveFileDialog1.FilterIndex)
                        {
                            case 1:
                                saveFileDialog1.FileName += ".jpg";
                                break;
                            case 2:
                                saveFileDialog1.FileName += ".png";
                                break;
                            case 3:
                                saveFileDialog1.FileName += ".bmp";
                                break;
                            default:
                                break;
                        }
                    }

                    saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk);
                    saveFileDialog1.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("No image to save");
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                resultImage.Save(saveFileDialog1.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while saving the file: " + ex.Message);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile(pictureBox1, ref loadImage);
        }
    }
}
