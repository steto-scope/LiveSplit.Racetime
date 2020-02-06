namespace LiveSplit.Racetime.View
{
    partial class RacerControl
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.placementLabel = new System.Windows.Forms.Label();
            this.usernameLabel = new System.Windows.Forms.Label();
            this.liveStatusImage = new System.Windows.Forms.PictureBox();
            this.roleImage = new System.Windows.Forms.PictureBox();
            this.timeLabel = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.liveStatusImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.roleImage)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.liveStatusImage);
            this.flowLayoutPanel1.Controls.Add(this.placementLabel);
            this.flowLayoutPanel1.Controls.Add(this.roleImage);
            this.flowLayoutPanel1.Controls.Add(this.usernameLabel);
            this.flowLayoutPanel1.Controls.Add(this.timeLabel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(280, 16);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // placementLabel
            // 
            this.placementLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.placementLabel.AutoSize = true;
            this.placementLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.placementLabel.Location = new System.Drawing.Point(16, 1);
            this.placementLabel.Margin = new System.Windows.Forms.Padding(0);
            this.placementLabel.Name = "placementLabel";
            this.placementLabel.Size = new System.Drawing.Size(24, 13);
            this.placementLabel.TabIndex = 2;
            this.placementLabel.Text = "1st";
            this.placementLabel.Visible = false;
            // 
            // usernameLabel
            // 
            this.usernameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.usernameLabel.Location = new System.Drawing.Point(56, 1);
            this.usernameLabel.Margin = new System.Windows.Forms.Padding(0);
            this.usernameLabel.Name = "usernameLabel";
            this.usernameLabel.Size = new System.Drawing.Size(100, 13);
            this.usernameLabel.TabIndex = 1;
            this.usernameLabel.Text = "Name";
            // 
            // liveStatusImage
            // 
            this.liveStatusImage.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.liveStatusImage.Image = global::LiveSplit.Racetime.Properties.Resources.live;
            this.liveStatusImage.Location = new System.Drawing.Point(0, 0);
            this.liveStatusImage.Margin = new System.Windows.Forms.Padding(0);
            this.liveStatusImage.Name = "liveStatusImage";
            this.liveStatusImage.Size = new System.Drawing.Size(16, 16);
            this.liveStatusImage.TabIndex = 0;
            this.liveStatusImage.TabStop = false;
            // 
            // roleImage
            // 
            this.roleImage.Location = new System.Drawing.Point(40, 0);
            this.roleImage.Margin = new System.Windows.Forms.Padding(0);
            this.roleImage.Name = "roleImage";
            this.roleImage.Size = new System.Drawing.Size(16, 16);
            this.roleImage.TabIndex = 3;
            this.roleImage.TabStop = false;
            this.roleImage.Visible = false;
            // 
            // timeLabel
            // 
            this.timeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.timeLabel.AutoSize = true;
            this.timeLabel.Location = new System.Drawing.Point(159, 1);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(0, 13);
            this.timeLabel.TabIndex = 4;
            // 
            // RacerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.flowLayoutPanel1);
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "RacerControl";
            this.Size = new System.Drawing.Size(280, 16);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.liveStatusImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.roleImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PictureBox liveStatusImage;
        private System.Windows.Forms.Label usernameLabel;
        private System.Windows.Forms.Label placementLabel;
        private System.Windows.Forms.PictureBox roleImage;
        private System.Windows.Forms.Label timeLabel;
    }
}
