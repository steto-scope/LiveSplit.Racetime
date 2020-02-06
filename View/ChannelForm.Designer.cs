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
            this.userlist = new LiveSplit.Racetime.View.UserListControl();
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
            this.splitContainer2.Panel1.Controls.Add(this.chatBox);
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
            this.chatBox.Size = new System.Drawing.Size(500, 400);
            this.chatBox.TabIndex = 0;
            this.chatBox.Text = "";
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
            this.saveLogButton.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.saveLogButton.Size = new System.Drawing.Size(70, 23);
            this.saveLogButton.TabIndex = 2;
            this.saveLogButton.Text = "Save Log";
            // 
            // inputBox
            // 
            this.inputBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.inputBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.inputBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.inputBox.Location = new System.Drawing.Point(0, 0);
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
            this.actionButton.Padding = new System.Windows.Forms.Padding(5, 5, 5, 5);
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
    }
}