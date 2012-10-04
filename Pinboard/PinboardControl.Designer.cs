namespace Jamoki.Tools.Pinboard
{
    partial class PinboardControl
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
            if (disposing)
            {
                if (components != null)
                    components.Dispose();

                if (disposing)
                {
                    if (buffer != null)
                    {
                        buffer.Dispose();
                        buffer = null;
                    }

                    if (bufferContext != null)
                    {
                        bufferContext.Dispose();
                        bufferContext = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PinboardControl
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "PinboardControl";
            this.Size = new System.Drawing.Size(311, 265);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.PinboardControl_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.PinboardControl_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PinboardControl_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PinboardControl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PinboardControl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PinboardControl_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
