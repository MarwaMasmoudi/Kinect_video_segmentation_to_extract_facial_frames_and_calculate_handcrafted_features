using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.Samples.Kinect.FaceBasics
{
    public partial class FrameResultControl : UserControl
    {
        public FrameResultControl()
        {
            InitializeComponent();
        }

        public FrameResultControl(FramesResult source)
        {
            InitializeComponent();
            this.source  = source;
            if (Source != null)
            {
                try
                {
                    pictureFrame.Image = Source.FrameImage;
                }
                catch { }
                try
                {
                    pictureFace.Image = Source.FaceImage;
                }
                catch { }

                checkConfirm.Checked = Source.Confirmed;
                checkEmotion.Checked = Source.Emotion;
            }
        }

        FramesResult source;

        public FramesResult Source
        {
            get { return source; }
            set
            {
                source = value;
            }
        }

        private void checkConfirm_CheckedChanged(object sender, EventArgs e)
        {
            checkEmotion.Visible = checkConfirm.Checked;
            pictureFace.Visible  = pictureFrame.Visible = checkConfirm.Checked;

            Source.Confirmed = checkConfirm.Checked;

        }

        private void checkEmotion_CheckedChanged(object sender, EventArgs e)
        {
            Source.Emotion = checkEmotion.Checked;
        }
    }
}
