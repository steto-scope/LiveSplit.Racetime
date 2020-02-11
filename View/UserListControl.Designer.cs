namespace LiveSplit.Racetime.View
{
    partial class UserListControl
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
            this.userPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.SuspendLayout();
            // 
            // userPanel
            // 
            this.userPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.userPanel.AutoSize = true;
            this.userPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.userPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.userPanel.Location = new System.Drawing.Point(0, 0);
            this.userPanel.Margin = new System.Windows.Forms.Padding(0);
            this.userPanel.Name = "userPanel";
            this.userPanel.Size = new System.Drawing.Size(0, 0);
            this.userPanel.TabIndex = 0;
            // 
            // UserListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Controls.Add(this.userPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UserListControl";
            this.Size = new System.Drawing.Size(180, 400);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel userPanel;
    }
}
