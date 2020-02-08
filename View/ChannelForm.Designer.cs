namespace LiveSplit.Racetime.View
{
    partial class ChannelForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chatBox = new System.Windows.Forms.RichTextBox();
            this.userlist = new LiveSplit.Racetime.View.UserListControl();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.hideFinishesCheckBox = new DarkUI.Controls.DarkCheckBox();
            this.hideChatCheckBox = new DarkUI.Controls.DarkCheckBox();
            this.saveLogButton = new DarkUI.Controls.DarkButton();
            this.inputBox = new DarkUI.Controls.DarkTextBox();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.readyCheckBox = new DarkUI.Controls.DarkCheckBox();
            this.actionButton = new DarkUI.Controls.DarkButton();
            this.hideFinishesToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.hideMidraceChatToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.goalLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.infoLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Size = new System.Drawing.Size(684, 461);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.AutoScroll = true;
            this.splitContainer2.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.splitContainer2.Panel2.Controls.Add(this.userlist);
            this.splitContainer2.Size = new System.Drawing.Size(684, 400);
            this.splitContainer2.SplitterDistance = 500;
            this.splitContainer2.TabIndex = 0;
            // 
            // chatBox
            // 
            this.chatBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.chatBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chatBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chatBox.ForeColor = System.Drawing.Color.White;
            this.chatBox.Location = new System.Drawing.Point(0, 0);
            this.chatBox.Name = "chatBox";
            this.chatBox.ReadOnly = true;
            this.chatBox.Size = new System.Drawing.Size(500, 379);
            this.chatBox.TabIndex = 0;
            this.chatBox.Text = "";
            // 
            // userlist
            // 
            this.userlist.BackColor = System.Drawing.Color.Transparent;
            this.userlist.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userlist.Location = new System.Drawing.Point(0, 0);
            this.userlist.Margin = new System.Windows.Forms.Padding(0);
            this.userlist.MinimumSize = new System.Drawing.Size(100, 100);
            this.userlist.Name = "userlist";
            this.userlist.Size = new System.Drawing.Size(180, 400);
            this.userlist.TabIndex = 0;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer4);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.flowLayoutPanel2);
            this.splitContainer3.Size = new System.Drawing.Size(684, 57);
            this.splitContainer3.SplitterDistance = 499;
            this.splitContainer3.TabIndex = 0;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.flowLayoutPanel1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.inputBox);
            this.splitContainer4.Size = new System.Drawing.Size(499, 57);
            this.splitContainer4.SplitterDistance = 28;
            this.splitContainer4.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.hideFinishesCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.hideChatCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.saveLogButton);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(499, 28);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // hideFinishesCheckBox
            // 
            this.hideFinishesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.hideFinishesCheckBox.AutoSize = true;
            this.hideFinishesCheckBox.Location = new System.Drawing.Point(3, 6);
            this.hideFinishesCheckBox.Name = "hideFinishesCheckBox";
            this.hideFinishesCheckBox.Size = new System.Drawing.Size(89, 17);
            this.hideFinishesCheckBox.TabIndex = 0;
            this.hideFinishesCheckBox.Text = "Hide Finishes";
            this.hideFinishesToolTip.SetToolTip(this.hideFinishesCheckBox, "While racing hide finishes, comments and leaving users");
            // 
            // hideChatCheckBox
            // 
            this.hideChatCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.hideChatCheckBox.AutoSize = true;
            this.hideChatCheckBox.Location = new System.Drawing.Point(98, 6);
            this.hideChatCheckBox.Name = "hideChatCheckBox";
            this.hideChatCheckBox.Size = new System.Drawing.Size(117, 17);
            this.hideChatCheckBox.TabIndex = 1;
            this.hideChatCheckBox.Text = "Hide Mid-race Chat";
            this.hideMidraceChatToolTip.SetToolTip(this.hideChatCheckBox, "While racing hides everything besides  messages of priviledged users and RaceBot " +
        "");
            // 
            // saveLogButton
            // 
            this.saveLogButton.Location = new System.Drawing.Point(221, 3);
            this.saveLogButton.Name = "saveLogButton";
            this.saveLogButton.Padding = new System.Windows.Forms.Padding(5);
            this.saveLogButton.Size = new System.Drawing.Size(70, 23);
            this.saveLogButton.TabIndex = 2;
            this.saveLogButton.Text = "Save Log";
            this.saveLogButton.Click += new System.EventHandler(this.saveLogButton_Click);
            // 
            // inputBox
            // 
            this.inputBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.inputBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.inputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.inputBox.Location = new System.Drawing.Point(0, 0);
            this.inputBox.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.inputBox.Name = "inputBox";
            this.inputBox.Size = new System.Drawing.Size(499, 20);
            this.inputBox.TabIndex = 0;
            this.inputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputBox_KeyDown);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.flowLayoutPanel2.Controls.Add(this.readyCheckBox);
            this.flowLayoutPanel2.Controls.Add(this.actionButton);
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(182, 46);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // readyCheckBox
            // 
            this.readyCheckBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readyCheckBox.AutoSize = true;
            this.readyCheckBox.Enabled = false;
            this.readyCheckBox.Location = new System.Drawing.Point(3, 3);
            this.readyCheckBox.Name = "readyCheckBox";
            this.readyCheckBox.Size = new System.Drawing.Size(57, 17);
            this.readyCheckBox.TabIndex = 0;
            this.readyCheckBox.Text = "Ready";
            this.readyCheckBox.CheckedChanged += new System.EventHandler(this.readyCheckBox_CheckedChanged);
            this.readyCheckBox.Click += new System.EventHandler(this.readyCheckBox_Click);
            // 
            // actionButton
            // 
            this.actionButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.actionButton.Enabled = false;
            this.actionButton.Location = new System.Drawing.Point(66, 3);
            this.actionButton.Name = "actionButton";
            this.actionButton.Padding = new System.Windows.Forms.Padding(5);
            this.actionButton.Size = new System.Drawing.Size(94, 23);
            this.actionButton.TabIndex = 1;
            this.actionButton.Text = "Connect";
            this.actionButton.Click += new System.EventHandler(this.actionButton_Click);
            // 
            // hideFinishesToolTip
            // 
            this.hideFinishesToolTip.AutoPopDelay = 15000;
            this.hideFinishesToolTip.InitialDelay = 300;
            this.hideFinishesToolTip.ReshowDelay = 100;
            // 
            // hideMidraceChatToolTip
            // 
            this.hideMidraceChatToolTip.AutoPopDelay = 15000;
            this.hideMidraceChatToolTip.InitialDelay = 300;
            this.hideMidraceChatToolTip.ReshowDelay = 100;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.flowLayoutPanel3);
            this.splitContainer5.Panel1MinSize = 12;
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.chatBox);
            this.splitContainer5.Size = new System.Drawing.Size(500, 400);
            this.splitContainer5.SplitterDistance = 20;
            this.splitContainer5.SplitterWidth = 1;
            this.splitContainer5.TabIndex = 1;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.label2);
            this.flowLayoutPanel3.Controls.Add(this.goalLabel);
            this.flowLayoutPanel3.Controls.Add(this.label3);
            this.flowLayoutPanel3.Controls.Add(this.infoLabel);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(500, 20);
            this.flowLayoutPanel3.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Silver;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Goal:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // goalLabel
            // 
            this.goalLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.goalLabel.AutoSize = true;
            this.goalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.goalLabel.ForeColor = System.Drawing.Color.White;
            this.goalLabel.Location = new System.Drawing.Point(46, 0);
            this.goalLabel.Name = "goalLabel";
            this.goalLabel.Size = new System.Drawing.Size(0, 13);
            this.goalLabel.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Silver;
            this.label3.Location = new System.Drawing.Point(52, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Info:";
            // 
            // infoLabel
            // 
            this.infoLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.infoLabel.AutoSize = true;
            this.infoLabel.ForeColor = System.Drawing.Color.White;
            this.infoLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.infoLabel.Location = new System.Drawing.Point(91, 0);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(0, 13);
            this.infoLabel.TabIndex = 5;
            this.infoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ChannelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 461);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(680, 400);
            this.Name = "ChannelForm";
            this.Text = "ChannelWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChannelForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChannelForm_FormClosed);
            this.Load += new System.EventHandler(this.ChannelWindow_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel2.PerformLayout();
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox chatBox;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private DarkUI.Controls.DarkCheckBox hideFinishesCheckBox;
        private DarkUI.Controls.DarkCheckBox hideChatCheckBox;
        private DarkUI.Controls.DarkTextBox inputBox;
        private System.Windows.Forms.ToolTip hideFinishesToolTip;
        private System.Windows.Forms.ToolTip hideMidraceChatToolTip;
        private DarkUI.Controls.DarkButton saveLogButton;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private DarkUI.Controls.DarkCheckBox readyCheckBox;
        private DarkUI.Controls.DarkButton actionButton;
        private UserListControl userlist;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label goalLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel infoLabel;
    }
}