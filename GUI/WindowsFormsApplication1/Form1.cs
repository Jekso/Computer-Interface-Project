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
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        SerialPort port;
        int remaining_slots = 9;
        int entrance_sensor = 0, exit_sensor = 0;
        bool entrance_sensor_last = false, exit_sensor_last = false;
        int tmp = 0;
        String data_rx = "", entrance_txt = "", exit_txt = "";

        const int entrance_sensor_bit = 0;
        const int exit_sensor_bit = 1;

        const int PORTB = 37;
        const int DDRB  = 36;
        const int PINB  = 35;

        const int PORTC = 40;
        const int DDRC  = 39;
        const int PINC  = 38;


        public Form1()
        {
            InitializeComponent();
            refresh_com_ports();
            label2.Text = "Disconnected";
            label2.ForeColor = Color.Red;
            port = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            refresh_com_ports();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            connect();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            disconnect();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnect();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //read(PINC);
            Init();
            System.Threading.Thread.Sleep(100);
            port.DataReceived += new SerialDataReceivedEventHandler(data_rx_handler);
            timer4.Enabled = true;
            System.Threading.Thread.Sleep(90);
            timer1.Enabled = true;
            System.Threading.Thread.Sleep(90);
            timer2.Enabled = true;
            System.Threading.Thread.Sleep(90);
            timer3.Enabled = true;
            System.Threading.Thread.Sleep(90);
        }


        /**
         *  Custom functions
         * */
        private void Init()
        {
            remaining_slots = 9;
            entrance_sensor = 0;
            exit_sensor = 0;
            entrance_sensor_last = false;
            exit_sensor_last = false;
            tmp = 0;
            data_rx = "";
            entrance_txt = "";
            exit_txt = "";

            write(DDRB, "255");       //Configure PORTB pins as output
            write(DDRC, "060");       //Configure PORTC pins as output 
        }

        private void refresh_com_ports()
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private void disconnect()
        {
            try
            {
                if (port.IsOpen)
                {
                    port.Close();
                    label2.Text = "Disconnected";
                    label2.ForeColor = Color.Red;
                }
            }
            catch (Exception e) { }
        }


        private void connect()
        {
            port = new SerialPort(comboBox1.SelectedItem.ToString());
            port.BaudRate = 2400;
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                    label2.Text = "Connected";
                    label2.ForeColor = Color.Green;
                }
            }
            catch (Exception e) { }
        }



        private void read(int Address)
        {
            try
            {
                port.Write("@R" + Address.ToString() + "000;");
                System.Threading.Thread.Sleep(50);
            }
            catch (Exception e) { }
        }

        private void write(int Address , string Value)
        {
            try
            {
                port.Write("@W" + Address.ToString() + Value + ";");
                System.Threading.Thread.Sleep(50);
            }
            catch (Exception e) { }
        }


        private void data_rx_handler(object sender, SerialDataReceivedEventArgs args)
        {
            SerialPort sp = (SerialPort) sender;
            data_rx = sp.ReadExisting().Trim();
            if (data_rx != null)
            {
                if (data_rx.Trim().StartsWith("0"))
                {
                    data_rx = data_rx.Remove(0,1);   
                }
                if (data_rx.Contains(">"))
                {
                    data_rx = data_rx.Remove(data_rx.IndexOf(">"), 1);
                }
                if (data_rx.Contains("\n"))
                {
                    data_rx = data_rx.Remove(data_rx.IndexOf("\n", 2));
                }
            }

            try
            {
                tmp = Int32.Parse(data_rx);
            }
            catch (Exception ex) { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label5.Text = entrance_txt;
            label8.Text = exit_txt;
            if (remaining_slots > 9) remaining_slots = 9;
            else if (remaining_slots < 0) remaining_slots = 0;  
            label3.Text = remaining_slots.ToString();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            entrance_sensor = (tmp & 1) >> entrance_sensor_bit;
            if (entrance_sensor == 1 && entrance_sensor_last == false)
            {
                write(PORTC, "004");
                System.Threading.Thread.Sleep(1000);
                write(PORTC, "000");
                entrance_sensor_last = true;
                pictureBox2.Image = WindowsFormsApplication1.Properties.Resources.CarOut2;
                entrance_txt = "Car is entering..";
            }
            else if (entrance_sensor == 0 && entrance_sensor_last == true)
            {
                write(PORTC, "008");
                System.Threading.Thread.Sleep(1000);
                write(PORTC, "000");
                remaining_slots--;
                entrance_sensor_last = false;
                pictureBox2.Image = WindowsFormsApplication1.Properties.Resources.CarIn;
                entrance_txt = "Car has entered";
                System.Threading.Thread.Sleep(1000);
                entrance_txt = "";
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            exit_sensor = (tmp & 2) >> exit_sensor_bit;
            if (exit_sensor == 1 && exit_sensor_last == false)
            {
                write(PORTC, "016");
                System.Threading.Thread.Sleep(1000);
                write(PORTC, "000");
                exit_sensor_last = true;
                pictureBox1.Image = WindowsFormsApplication1.Properties.Resources.CarOut2;
                exit_txt = "Car is exiting..";
            }
            else if (exit_sensor == 0 && exit_sensor_last == true)
            {
                write(PORTC, "032");
                System.Threading.Thread.Sleep(1000);
                write(PORTC, "000");
                remaining_slots++;
                exit_sensor_last = false;
                pictureBox1.Image = WindowsFormsApplication1.Properties.Resources.CarIn;
                exit_txt = "Car has exited";
                System.Threading.Thread.Sleep(1000);
                exit_txt = "";
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            write_7_segment();
            read(PINC);
        }

        private void write_7_segment()
        {
            string tmp_value = "";
            switch (remaining_slots)
            {
                case 0:
                    tmp_value = "063";
                    break;
                case 1:
                    tmp_value = "006";
                    break;
                case 2:
                    tmp_value = "091";
                    break;
                case 3:
                    tmp_value = "079";
                    break;
                case 4:
                    tmp_value = "102";
                    break;
                case 5:
                    tmp_value = "109";
                    break;
                case 6:
                    tmp_value = "125";
                    break;
                case 7:
                    tmp_value = "007";
                    break;
                case 8:
                    tmp_value = "127";
                    break;
                case 9:
                    tmp_value = "111";
                    break;
            }
            write(PORTB, tmp_value);
        }
    }
}
