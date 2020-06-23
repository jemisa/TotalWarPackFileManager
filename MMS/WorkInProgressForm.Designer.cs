namespace MMS {
    partial class WorkInProgressForm {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent() {
            this.workLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // workLabel
            // 
            this.workLabel.AutoSize = true;
            this.workLabel.Location = new System.Drawing.Point(13, 13);
            this.workLabel.Name = "workLabel";
            this.workLabel.Size = new System.Drawing.Size(56, 13);
            this.workLabel.TabIndex = 0;
            this.workLabel.Text = "Working...";
            // 
            // WorkInProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 48);
            this.Controls.Add(this.workLabel);
            this.Name = "WorkInProgressForm";
            this.Text = "Work in Progress";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label workLabel;
    }
}