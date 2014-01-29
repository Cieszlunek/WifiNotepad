using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WifiNotepad
{
    public partial class Form1 : Form
    {
        private int port = 8988;
        private string ip = "192.168.1.100";
        private string fileName;
        public Form1()
        {
            InitializeComponent();
            textBox1.KeyDown += (Object sender, KeyEventArgs ev) => { OnKey(sender, ev); };
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void masterModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread masterThread = new Thread(MasterConnect);
            masterThread.Start();
        }

        private Object ToLock = new Object();
        public string DataToSend = "";
        public bool GO = true;
        private string DataReceived = "";
        private Object ToLockReceive = new Object();
        private int enterTemp = 0;

        private void OnKey(Object sender, KeyEventArgs ev)
        {
            if (ev.KeyCode == Keys.Enter) 
            {
                lock(ToLock)
                {
            	    DataToSend += "Enter," + textBox1.SelectionStart + "\r\n";
                }
            }
            else if (ev.KeyCode == Keys.Back)
            {
                lock(ToLock)
                {
                    DataToSend += "backspace," + textBox1.SelectionStart + "\r\n";
                }
            }
            else
            {
                if (ev.KeyCode == Keys.Shift || ev.KeyCode == Keys.Down || ev.KeyCode == Keys.Up || ev.KeyCode == Keys.Left || ev.KeyCode == Keys.Right || ev.KeyCode == Keys.Alt)
                { }
                else
                {
                    lock (ToLock)
                    {
                        if (ev.KeyCode == Keys.Space)
                        {
                            DataToSend += " ," + textBox1.SelectionStart + "\r\n";
                        }
                        else
                        {
                            DataToSend += ev.KeyData.ToString().ToLower() + "," + textBox1.SelectionStart + "\r\n";
                        }
                    }
                }
            }
        }

        public void ShowMessageBox(string value2)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ShowMessageBox), new object[] { value2 });
                return;
            }
            MessageBox.Show(value2);
        }
        public void SetLineTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            textBox1.Text += value + Environment.NewLine;
        }
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            string[] str = value.Split(',');
            int position = int.Parse(str[1]);
            string old = textBox1.Text;
            //position++;
            int cursor_position = textBox1.SelectionStart;

            if ("Enter" == str[0])
            {
                if (position == 0)
                {
                    textBox1.Text = "\r\n" + old;
                }
                else if (position >= old.Length)
                {
                    textBox1.Text += "\r\n" + "";
                }
                else
                {
                    String pre = old.Substring(0, position);
                    pre += "\r\n" + old.Substring(position);
                    textBox1.Text = pre;
                }
            }
            else if ("backspace" == str[0])
            {
                String neww = null;
                if (position - 1 > 0)
                {
                    if (old.Length > position)
                    {
                        neww = old.Substring(0, position);
                        String t = old.Substring(position + 1);
                        neww += t;
                    }
                    else
                    {
                        if (old.Length != 0)
                        {
                            neww = old.Substring(0, old.Length - 1);
                        }
                    }
                }
                else
                {
                    if (old.Length > 1)
                    {
                        neww = old.Substring(1);
                    }
                    else
                    {
                        neww = "";
                    }
                }
                textBox1.Text = neww;
            }
            else if (position < old.Length)
            {
                String neww = old.Substring(0, position);

                String t = old.Substring(position);
                neww += str[0] + t;
                textBox1.Text = neww;
            }
            else
            {
                textBox1.Text += str[0];
            }
            textBox1.Refresh();

        }

        private void MasterConnect()
        {
            TcpListener serverSocket = new TcpListener(port);
            TcpClient clientSocket = default(TcpClient);
            serverSocket.Start();
            clientSocket = serverSocket.AcceptTcpClient();

            try
            {
                NetworkStream networkStream = clientSocket.GetStream();
                StreamReader reader = new StreamReader(clientSocket.GetStream(), Encoding.UTF8);
                StreamWriter writer = new StreamWriter(clientSocket.GetStream());
                ShowMessageBox("connected");
                while (GO)
                {
                    lock (ToLock)
                    {
                        if (DataToSend != "")
                        {
                            writer.WriteLine(DataToSend);
                            DataToSend = "";
                        }
                        else
                        {
                            writer.WriteLine("#noAction");
                        }
                    }
                    writer.Flush();
                    Thread.Sleep(300);
                    lock (ToLockReceive)
                    {
                        string str = reader.ReadLine();
                        if (str == "#exit")
                        {
                            GO = false;
                        }
                        else if (str == "#noAction")
                        {
                        }
                        else if (str != "" && str != null)
                        {
                            if (DataReceived == "")
                            {
                                //DataReceived = str;
                            }
                            else
                            {
                                //DataReceived += "\n" + str;
                            }
                            AppendTextBox(str);
                        }
                    }
                    Thread.Sleep(300);
                }
                reader.Close();
                writer.Close();
                networkStream.Close();
                ShowMessageBox("Disconnected.");
            }
            catch (Exception ex)
            {
                ShowMessageBox("Connection lost, internal error: " + ex.Message);
            }
            finally
            {
                clientSocket.Close();
                serverSocket.Stop();
            }
        }

        private void slaveModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IPdialog ipdialog = new IPdialog(ip);
            ipdialog.ShowDialog();
            if (ipdialog.result == true)
            {
                this.ip = ipdialog.ip;
                Thread th = new Thread(SlaveConnect);
                th.Start();
            }
            
        }

        private void SlaveConnect()
        {
            System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
            NetworkStream clientStream;
            clientSocket = new System.Net.Sockets.TcpClient();
            clientSocket.Connect(ip, port);
            clientStream = clientSocket.GetStream();
            clientSocket.SendTimeout = 25000;
            clientSocket.ReceiveTimeout = 25000;
            StreamReader reader = new StreamReader(clientSocket.GetStream(), Encoding.UTF8);
            StreamWriter writer = new StreamWriter(clientSocket.GetStream());
            ShowMessageBox("connected");

            while (GO)
            {
                lock (ToLockReceive)
                {
                    string str = reader.ReadLine();
                    if (str == "#exit")
                    {
                        GO = false;
                    }
                    else if (str == "#noAction")
                    {
                    }
                    else if (str != "" && str != null)
                    {
                        if (DataReceived == "")
                        {
                            //DataReceived = str;
                        }
                        else
                        {
                            //DataReceived += "\n" + str;
                        }
                        AppendTextBox(str);
                    }
                }
                Thread.Sleep(300);
                lock (ToLock)
                {
                    if (DataToSend != "")
                    {
                        writer.WriteLine(DataToSend);
                        DataToSend = "";
                    }
                    else
                    {
                        writer.WriteLine("#noAction");
                    }
                }
                writer.Flush();
                Thread.Sleep(300);
            }
            reader.Close();
            writer.Close();
            clientStream.Close();
            clientSocket.Close();
            MessageBox.Show("Disconnected.");
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog wybor = new OpenFileDialog();
            wybor.InitialDirectory = "C:\\";
            wybor.Filter = "pliki tekstowe | *.txt;*.dat;*.in;*.log;";
            wybor.ShowDialog();
            if (wybor.FileName != "")
            {
                StreamReader fs = null;
                //try
                //{
                    fs = new StreamReader(wybor.FileName);
                    string str;
                    while ((str = fs.ReadLine()) != null)
                    {
                        SetLineTextBox(str);
                    }
                    this.fileName = wybor.FileName;
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.Message);
                //}
                //finally
                //{
                //    if(fs != null)
                //    fs.Close();
                //}
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileName != null)
            {
                StreamWriter sw = new StreamWriter(fileName);
            }
            else
            {
                IPdialog dialog = new IPdialog("c://new.txt");
                dialog.Text = "Enter file name";
                dialog.setLabelText("Enter file name and patch:");
                dialog.setIPMode(false); //normal string 
                dialog.ShowDialog();
                if (dialog.result == true)
                {
                    try
                    {
                        StreamWriter sw = new StreamWriter(dialog.ip);
                        sw.Write(textBox1.Text);
                        sw.Flush();
                        sw.Close();
                        MessageBox.Show("File saved succesfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error while saving file. Internal error: " + ex.Message);
                    }
                }
            }
        }

    }
}
