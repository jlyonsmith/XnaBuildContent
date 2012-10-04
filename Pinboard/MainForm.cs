using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using Playroom;
using ToolBelt;

namespace Jamoki.Tools.Pinboard
{
    public partial class MainForm : Form
    {
        private readonly string fileDialogFilter = "Pinboard Files (*.pinboard)|*.pinboard|All files (*.*)|*.*";
        private PropertiesForm propertiesForm;
        private PinboardFileV1 data;
        private string fileName;
        private string tempFileName;
        private int nextTempFileNum;

        public MainForm(string fileName)
        {
            InitializeComponent();

            this.propertiesForm = new PropertiesForm();
            this.propertiesForm.PinboardControl = this.pinboardControl;
            this.propertiesForm.ScreenSizeChanged += new EventHandler<ScreenSizeChangedEventArgs>(PropertiesForm_ScreenSizeChanged);

            this.pinboardControl.PropertiesForm = this.propertiesForm;
            this.pinboardControl.DataDirtied += new EventHandler<EventArgs>(PinboardControl_DataDirtied);

            nextTempFileNum = 1;

            if (fileName != null)
            {
                OpenFile(fileName);
            }
            else
            {
                NewFile();
            }
        }

        private void SetPinboardData(PinboardFileV1 data)
        {
            this.data = data;

            this.pinboardControl.Data = data;
            this.pinboardControl.Visible = true;

            if (!this.propertiesForm.Visible)
                this.propertiesForm.Show(this);
        }

        private void SetPinboardSize()
        {
            this.SetClientSizeCore(
                this.data.ScreenRectInfo.Rectangle.Width,
                this.data.ScreenRectInfo.Height + this.menuStrip.Height + SystemInformation.BorderSize.Height * 3);
            
            this.pinboardControl.Size = this.data.ScreenRectInfo.Size;
        }

        private void SetWindowTitle()
        {
            if (data != null)
            {
                string displayFileName = (this.fileName == null ? tempFileName : fileName);

                displayFileName = Path.GetFileName(displayFileName);

                this.Text = displayFileName + (pinboardControl.DataDirty ? "*" : "") + " - Pinboard";
            }
            else
                this.Text = "Pinboard";
        }

        private void OpenFile(string fileName)
        {
            PinboardFileV1 data = null;

            try
            {
                data = PinboardFileReaderV1.ReadFile(new ParsedPath(fileName, PathType.File));
            }
            catch
            {
                MessageBox.Show("Unable to load pinboard file", "Error Loading File",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            this.fileName = fileName;
            SetPinboardData(data);
            SetPinboardSize();
        }

        private void SaveFile(string fileName)
        {
            if (fileName == null)
                fileName = this.fileName;

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Indent = true,
                OmitXmlDeclaration = true,
                IndentChars = "  "
            };

            try
            {
                using (XmlWriter writer = XmlWriter.Create(fileName, settings))
                {
                    PinboardDataWriter.WriteXml(writer, data);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is IOException || ex is XmlException))
                    throw;

                return;
            }

            this.fileName = fileName;
            this.tempFileName = null;
            pinboardControl.DataDirty = false;
        }

        private void NewFile()
        {
            fileName = null;
            tempFileName = "PinboardFile" + nextTempFileNum.ToString() + ".pinboard";
            nextTempFileNum++;
            SetPinboardData(PinboardFileV1.Default);
            SetPinboardSize();
            SetWindowTitle();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.pinboardControl.DeleteSelection();
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.propertiesForm.Visible)
            {
                this.propertiesForm.Hide();
            }
            else
            {
                this.propertiesForm.Show(this);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.saveToolStripMenuItem.Enabled = (data != null && pinboardControl.DataDirty);
            this.saveAsToolStripMenuItem.Enabled = (data != null);
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            this.propertiesToolStripMenuItem.Checked = (propertiesForm.Visible);
            this.propertiesToolStripMenuItem.Enabled = (data != null);
            this.deleteToolStripMenuItem.Enabled = (data != null);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pinboardControl.DataDirty && !AskToDiscardDirtyData())
                return;

            NewFile();
        }

        private bool AskToDiscardDirtyData()
        {
            DialogResult result = MessageBox.Show(
                "Discard changes since last save?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            return (result == DialogResult.Yes);
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Title = "Open";
            fileDialog.Filter = fileDialogFilter;
            fileDialog.FilterIndex = 1;

            if (fileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            OpenFile(fileDialog.FileName);
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileName == null)
            {
                SaveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                SaveFile(null);
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.Title = "Save As";
            fileDialog.FileName = tempFileName;
            fileDialog.Filter = fileDialogFilter;
            fileDialog.FilterIndex = 1;

            if (fileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            SaveFile(fileDialog.FileName);
        }

        private void PropertiesForm_ScreenSizeChanged(object sender, ScreenSizeChangedEventArgs args)
        {
            SetPinboardSize();            
        }

        public void PinboardControl_DataDirtied(object sender, EventArgs args)
        {
            SetWindowTitle();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pinboardControl.DataDirty && !AskToDiscardDirtyData())
            {
                e.Cancel = true;
                return;
            }
        }

        private void DuplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pinboardControl.DuplicateSelection();
        }

        private void newRectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pinboardControl.NewRectangle();
        }
    }
}

