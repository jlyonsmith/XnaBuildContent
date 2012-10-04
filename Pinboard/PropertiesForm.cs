using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Playroom;

namespace Jamoki.Tools.Pinboard
{
    public partial class PropertiesForm : Form
    {
        private PropertiesFormMode mode;
        private bool inErrorState;
        private PinboardControl pinboardControl;
        
        public PropertiesForm()
        {
            InitializeComponent();

            Mode = PropertiesFormMode.FullEdit;
        }

        public PinboardControl PinboardControl 
        {
            get { return pinboardControl; }
            set
            {
                pinboardControl = value;
                pinboardControl.SelectionChanged += new EventHandler<EventArgs>(PinboardControl_SelectionChanged);
                
                SetSelectionValuesAndMode();
            }
        }

        public PropertiesFormMode Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;

                switch (Mode)
                {
                    case PropertiesFormMode.FullEdit:
                        nameTextBox.ReadOnly = false;
                        xTextBox.ReadOnly = false;
                        yTextBox.ReadOnly = false;
                        break;

                    case PropertiesFormMode.PartialEdit:
                        nameTextBox.ReadOnly = true;
                        xTextBox.ReadOnly = true;
                        yTextBox.ReadOnly = true;
                        break;
                }
            }
        }

        private void PropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        public void PinboardControl_SelectionChanged(object sender, EventArgs args)
        {
            SetSelectionValuesAndMode();

            if (inErrorState)
                ResetColors();
        }

        private void SetSelectionValuesAndMode()
        {
            PinboardFileV1.RectangleInfo rectInfo = pinboardControl.Selection;

            if (rectInfo != null)
            {
                this.nameTextBox.Text = rectInfo.Name;
                this.xTextBox.Text = rectInfo.Rectangle.X.ToString();
                this.yTextBox.Text = rectInfo.Rectangle.Y.ToString();
                this.widthTextBox.Text = rectInfo.Rectangle.Width.ToString();
                this.heightTextBox.Text = rectInfo.Rectangle.Height.ToString();
                this.depthTextBox.Text = pinboardControl.SelectionIndex.ToString();
                this.redColorPicker.Value = rectInfo.Color.R;
                this.greenColorPicker.Value = rectInfo.Color.G;
                this.blueColorPicker.Value = rectInfo.Color.B;
                this.alphaColorPicker.Value = rectInfo.Color.A;
            }

            PropertiesFormMode mode;

            if (pinboardControl.SelectionIndex == -1)
                mode = PropertiesFormMode.PartialEdit;
            else
                mode = PropertiesFormMode.FullEdit;

            if (this.Mode != mode)
                this.Mode = mode;
        }

        private void ResetColors()
        {
            nameTextBox.ForeColor = SystemColors.WindowText;
            xTextBox.ForeColor = SystemColors.WindowText;
            yTextBox.ForeColor = SystemColors.WindowText;
            widthTextBox.ForeColor = SystemColors.WindowText;
            heightTextBox.ForeColor = SystemColors.WindowText;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            applyButton.Focus();
            
            if (inErrorState)
                ResetColors();

            inErrorState = true;

            PinboardFileV1 data = pinboardControl.Data;
            PinboardFileV1.RectangleInfo selection = pinboardControl.Selection;
            int selectionIndex = pinboardControl.SelectionIndex;

            if (pinboardControl.SelectionIndex == -1)
            {
                Size savedSize = selection.Size;

                int width;

                if (!int.TryParse(widthTextBox.Text, out width) ||
                    width < 128 ||
                    width > 2048)
                {
                    widthTextBox.ForeColor = Color.Red;
                    widthTextBox.Focus();
                    return;
                }

                selection.Width = width;

                int height;

                if (!int.TryParse(heightTextBox.Text, out height) ||
                    height < 128 ||
                    height > 2048)
                {
                    heightTextBox.ForeColor = Color.Red;
                    heightTextBox.Focus();
                    return;
                }

                selection.Height = height;

                RaiseScreenSizeChangedEvent(savedSize);
            }
            else
            {
                string name = nameTextBox.Text;
                bool duplicate = false;

                if (name != selection.Name)
                {
                    if (name == data.ScreenRectInfo.Name)
                        duplicate = true;
                    else
                    {
                        for (int i = 0; i < data.RectInfos.Count; i++)
                        {
                            if (i == selectionIndex)
                                continue;

                            PinboardFileV1.RectangleInfo rectInfo = data.RectInfos[i];

                            if (name == rectInfo.Name)
                            {
                                duplicate = true;
                                break;
                            }
                        }
                    }
                }

                if (name.Length == 0 || duplicate)
                {
                    nameTextBox.ForeColor = Color.Red;
                    nameTextBox.Focus();
                    return;
                }

                selection.Name = name;

                int x;

                if (!int.TryParse(xTextBox.Text, out x) ||
                    x < 0 ||
                    x > data.ScreenRectInfo.Width - selection.Width)
                {
                    xTextBox.ForeColor = Color.Red;
                    xTextBox.Focus();
                    return;
                }

                selection.X = x;

                int y;

                if (!int.TryParse(yTextBox.Text, out y) ||
                    y < 0 ||
                    y > data.ScreenRectInfo.Height - selection.Height)
                {
                    yTextBox.ForeColor = Color.Red;
                    yTextBox.Focus();
                    return;
                }

                selection.Y = y;

                int width;

                if (!int.TryParse(widthTextBox.Text, out width) ||
                    width < 1 ||
                    width > data.ScreenRectInfo.Width - selection.X)
                {
                    widthTextBox.ForeColor = Color.Red;
                    widthTextBox.Focus();
                    return;
                }

                selection.Width = width;

                int height;

                if (!int.TryParse(heightTextBox.Text, out height) ||
                    height < 1 ||
                    height > data.ScreenRectInfo.Height - selection.Y)
                {
                    heightTextBox.ForeColor = Color.Red;
                    heightTextBox.Focus();
                    return;
                }

                selection.Height = height;
            }

            inErrorState = false;
            pinboardControl.DataDirty = true;
        }

        public event EventHandler<ScreenSizeChangedEventArgs> ScreenSizeChanged;

        private void RaiseScreenSizeChangedEvent(Size oldSize)
        {
            EventHandler<ScreenSizeChangedEventArgs> handler = this.ScreenSizeChanged;

            if (handler != null)
                handler(this, new ScreenSizeChangedEventArgs(oldSize));
        }

        private void RedColorPicker_Changed(object sender, EventArgs e)
        {
            Color color = pinboardControl.Selection.Color;
            Color newColor = Color.FromArgb(color.A, redColorPicker.Value, color.G, color.B);
            pinboardControl.Selection.Color = newColor;
            pinboardControl.DataDirty = true;
        }

        private void GreenColorPicker_Changed(object sender, EventArgs e)
        {
            Color color = pinboardControl.Selection.Color;
            Color newColor = Color.FromArgb(color.A, color.R, greenColorPicker.Value, color.B);
            pinboardControl.Selection.Color = newColor;
            pinboardControl.DataDirty = true;
        }

        private void BlueColorPicker_Changed(object sender, EventArgs e)
        {
            Color color = pinboardControl.Selection.Color;
            Color newColor = Color.FromArgb(color.A, color.R, color.G, blueColorPicker.Value);
            pinboardControl.Selection.Color = newColor;
            pinboardControl.DataDirty = true;
        }

        private void AlphaColorPicker_Changed(object sender, EventArgs e)
        {
            Color color = pinboardControl.Selection.Color;
            Color newColor = Color.FromArgb(alphaColorPicker.Value, color.R, color.G, color.B);
            pinboardControl.Selection.Color = newColor;
            pinboardControl.DataDirty = true;
        }
    }

    public enum PropertiesFormMode
    {
        FullEdit,
        PartialEdit
    }
}

