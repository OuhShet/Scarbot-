using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Scarbotv2
{
    public partial class Form1 : Form
    {
        String mensaje;
        VideoCapture _capture;
        private Mat _frame;

        public Form1()
        {
            InitializeComponent();

            _capture = new VideoCapture(0);
            _capture.ImageGrabbed += ProcessFrame;
            _frame = new Mat();

            if(_capture != null)
            {

                try
                {
                    _capture.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void ProcessFrame(Object sender, EventArgs e)
        {

            if( _capture != null && _capture.Ptr != IntPtr.Zero)
            {
                _capture.Retrieve(_frame, 0);
                Image<Bgr, Byte> image = _frame.ToImage<Bgr, byte> ();// toma foto
                image.Save("capture3.jpg");
                pictureBox1.Image = _frame.Bitmap;

           

            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if(serialPort1.IsOpen == false)
            {
                serialPort1.BaudRate = 9600;
                serialPort1.DataBits = 8;
                serialPort1.Parity = Parity.None;
                serialPort1.StopBits = StopBits.One;
                serialPort1.PortName = comboBox1.SelectedItem.ToString();
                serialPort1.Open();
                textBox1.Text = "Conectado";


                MessageBox.Show("Puerto Conectado");
                textBox1.BackColor = Color.Lime;
                timer1.Enabled = true;

            }



        }

        private void button2_Click(object sender, EventArgs e)
        {
            if( serialPort1.IsOpen == true)
            {
                serialPort1.Close();
                timer1.Enabled = false;
                textBox1.Text = "Desconectado";

                MessageBox.Show("Puerto Desconectado");
                textBox1.BackColor = Color.Red;
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            serialPort1.Write(richTextBox1.Text);
            serialPort1.Write("\r");
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            mensaje = serialPort1.ReadExisting();

            if(mensaje.Length>0)
            {

             /*
             * Aca se reciben los mensajes del robot y tienen que realizar la captura y procesamiento de las imagenes
             */
               
            if( mensaje.Contains("OK2"))  //verifica tabla es afirmativa
                {
                    serialPort1.Write("run 1"); /// tomar eje A
                    serialPort1.Write("\r");
                }
            if(mensaje.Contains("OK3"))
                {
                    serialPort1.Write("run 2");
                    serialPort1.Write("\r");

                }

            if(mensaje.Contains("OK1"))
                {
                    Image<Bgr, byte> image = _frame.ToImage<Bgr, byte>();
                    Boolean flag = false;
                    //Image<Bgr, byte> Mostrar = Image.Copy();

                    for(int i = 1; i < 13; i++)
                    {
                        Image<Bgr, byte> Patron = new Image<Bgr, byte>("" + i + ".jpg"); // imagen del patron

                        using (Image<Gray, float> resultado = image.MatchTemplate(Patron, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                        {
                            double[] minValues, maxValues;
                            Point[] minLocations, maxLocations;
                            resultado.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                            //valores del threshold 0.75 -- 0.9 es afirmativo

                        if(maxValues[0] > 0.9)
                            {
                                // dibuja un rectangulo al rededor de  lo encontrado.

                                Rectangle match = new Rectangle(maxLocations[0], Patron.Size);
                                image.Draw(match, new Bgr(Color.Red), 3);
                                pictureBox3.Image = image.ToBitmap();
                                flag = true;
                                pictureBox2.Image = Patron.ToBitmap();

                                if (i % 2 == 0 )
                                {
                                    serialPort1.Write("run 2");
                                    serialPort1.Write("\r");
                                }
                                else
                                {
                                    serialPort1.Write("run 3");
                                    serialPort1.Write("\r");
                                }
                            }

                        }

                        if(flag == false)
                        {
                            serialPort1.Write("move 0");
                            serialPort1.Write("\r");
                        }
                    }

                }



            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
