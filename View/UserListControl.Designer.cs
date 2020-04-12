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
            this.userPanel = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // userPanel
            // 
            this.userPanel.AutoSize = true;
            this.userPanel.ColumnCount = 4;
            this.userPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.userPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.userPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.userPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.userPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.userPanel.Location = new System.Drawing.Point(0, 0);
            this.userPanel.Name = "userPanel";
            this.userPanel.RowCount = 1;
            this.userPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.userPanel.Size = new System.Drawing.Size(233, 18);
            this.userPanel.TabIndex = 0;
            // 
            // UserListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.Controls.Add(this.userPanel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UserListControl";
            this.Size = new System.Drawing.Size(233, 400);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel userPanel;
    }
}
