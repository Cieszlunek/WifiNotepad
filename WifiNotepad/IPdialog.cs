using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WifiNotepad
{
    public partial class IPdialog : Form
    {
        public string ip;
        public bool result;
        private bool IP_mode = true;
        public IPdialog(string ipp)
        {
            InitializeComponent();
            this.ip = ipp;
            IPtextBox.Text = ip;
        }

        public void setLabelText(string str)
        {
            label1.Text = str;
        }

        public void setIPMode(bool mode)
        {
            IP_mode = mode;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            try
            {
                ip = IPtextBox.Text;
                if (IP_mode)
                {
                    string[] ip_blocks = new string[4];
                    ip_blocks = ip.Split('.');
                    for (int j = 0; j < 4; ++j)
                    {
                        if (("").Equals(ip_blocks[j]))
                        {
                            throw new Exception("Wrong ip address");
                        }
                        int i = int.Parse(ip_blocks[j]);
                    }
                }
                result = true;
                Close();
            }
            catch(IndexOutOfRangeException exc)
            {
                throw new Exception("Wrong ip adress");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                result = false;
            }

        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            result = false;
            Close();
        }
    }
}
