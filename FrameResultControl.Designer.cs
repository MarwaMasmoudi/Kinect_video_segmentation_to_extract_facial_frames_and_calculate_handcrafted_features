namespace Microsoft.Samples.Kinect.FaceBasics
{
    partial class FrameResultControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureFrame = new System.Windows.Forms.PictureBox();
            this.pictureFace = new System.Windows.Forms.PictureBox();
            this.checkConfirm = new System.Windows.Forms.CheckBox();
            this.checkEmotion = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureFrame)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureFace)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureFrame
            // 
            this.pictureFrame.Location = new System.Drawing.Point(3, 3);
            this.pictureFrame.Name = "pictureFrame";
            this.pictureFrame.Size = new System.Drawing.Size(183, 155);
            this.pictureFrame.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureFrame.TabIndex = 0;
            this.pictureFrame.TabStop = false;
            // 
            // pictureFace
            // 
            this.pictureFace.Location = new System.Drawing.Point(192, 3);
            this.pictureFace.Name = "pictureFace";
            this.pictureFace.Size = new System.Drawing.Size(121, 100);
            this.pictureFace.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureFace.TabIndex = 1;
            this.pictureFace.TabStop = false;
            // 
            // checkConfirm
            // 
            this.checkConfirm.AutoSize = true;
            this.checkConfirm.Location = new System.Drawing.Point(193, 109);
            this.checkConfirm.Name = "checkConfirm";
            this.checkConfirm.Size = new System.Drawing.Size(73, 17);
            this.checkConfirm.TabIndex = 2;
            this.checkConfirm.Text = "Confirmed";
            this.checkConfirm.UseVisualStyleBackColor = true;
            this.checkConfirm.CheckedChanged += new System.EventHandler(this.checkConfirm_CheckedChanged);
            // 
            // checkEmotion
            // 
            this.checkEmotion.AutoSize = true;
            this.checkEmotion.Location = new System.Drawing.Point(193, 129);
            this.checkEmotion.Name = "checkEmotion";
            this.checkEmotion.Size = new System.Drawing.Size(64, 17);
            this.checkEmotion.TabIndex = 3;
            this.checkEmotion.Text = "Emotion";
            this.checkEmotion.UseVisualStyleBackColor = true;
            this.checkEmotion.CheckedChanged += new System.EventHandler(this.checkEmotion_CheckedChanged);
            // 
            // FrameResultControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.checkEmotion);
            this.Controls.Add(this.checkConfirm);
            this.Controls.Add(this.pictureFace);
            this.Controls.Add(this.pictureFrame);
            this.Name = "FrameResultControl";
            this.Size = new System.Drawing.Size(317, 159);
            ((System.ComponentModel.ISupportInitialize)(this.pictureFrame)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureFace)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureFrame;
        private System.Windows.Forms.PictureBox pictureFace;
        private System.Windows.Forms.CheckBox checkConfirm;
        private System.Windows.Forms.CheckBox checkEmotion;
    }
}
