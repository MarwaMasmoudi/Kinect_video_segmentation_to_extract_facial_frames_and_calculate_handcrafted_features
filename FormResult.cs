using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Samples.Kinect.FaceBasics
{
    public partial class FormResult : Form
    {
        public FormResult(List<FramesResult> source)
        {
            InitializeComponent();
            Source = source;
            floderPathtext.Text = Environment.CurrentDirectory+@"\"+DateTime.Now.ToString().Replace("/", "_").Replace(":","_").Replace(".","_");
        }
        public string FloderPath {  get { return floderPathtext.Text; } }

        public List<FramesResult> Source { get => source;
            set
            {
                source = value;
            }

        }

        List<FramesResult> source;
        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (FloderPath != string.Empty)
                DialogResult = DialogResult.OK;
        }

        private void FormResult_Shown(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();

            foreach (FramesResult r in Source)
            {
                FrameResultControl c = new FrameResultControl(r);
                flowLayoutPanel1.Controls.Add(c);
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog() == DialogResult.OK)
                floderPathtext.Text = o.FileName;
        }

        string count
        {
            get
            {
                int i = 0;
                foreach (FramesResult f in Source)
                    if (f.Confirmed) i++;
                return string.Format("{1}/{0} Frames", Source.Count , i);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = count;
        }
    }
}
