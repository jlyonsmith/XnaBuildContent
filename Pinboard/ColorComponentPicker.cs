using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Jamoki.Tools.Pinboard
{
    public partial class ColorComponentPicker : Control
    {
        private Color startColor;
        private Color endColor;
        private ColorComponent component;
        private int componentValue;
        private readonly int indent = 5;
        
        public ColorComponentPicker()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.Selectable, true);

            this.ColorComponent = ColorComponent.Red;
        }

        public event EventHandler<EventArgs> Changed;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();

            if (e.Button == MouseButtons.Left)
            {
                Rectangle barRect;

                GetColorBarRectangle(out barRect);

                if (barRect.Contains(e.Location))
                {
                    this.Capture = true;

                    SetComponentValueFromMouseX(ref barRect, e.Location.X);
                    Invalidate();
                    RaiseChangedEvent();
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.Capture)
            {
                Rectangle barRect;

                GetColorBarRectangle(out barRect);
                SetComponentValueFromMouseX(ref barRect, e.Location.X);
                Invalidate();
                RaiseChangedEvent();
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (this.Capture)
            {
                Rectangle barRect;
                
                GetColorBarRectangle(out barRect);
                SetComponentValueFromMouseX(ref barRect, e.Location.X);
                Invalidate();

                this.Capture = false;

                RaiseChangedEvent();
            }

            base.OnMouseUp(e);
        }

        private void SetComponentValueFromMouseX(ref Rectangle barRect, int x)
        {
            if (x < barRect.Left)
                componentValue = 0;
            else if (x > barRect.Right - 1)
                componentValue = 255;
            else
                componentValue = (int)((float)(x - indent) * 255 / barRect.Width);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right) 
                return true;
            else
                return base.IsInputKey(keyData);
        }

        protected override void OnEnter(EventArgs e)
        {
            this.Invalidate();
            base.OnEnter(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            this.Invalidate();
            base.OnLeave(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Right && componentValue < 255)
            {
                componentValue++;
                RaiseChangedEvent();
                Invalidate();
            }
            else if (e.Modifiers == Keys.None && e.KeyCode == Keys.Left && componentValue > 0)
            {
                componentValue--;
                RaiseChangedEvent();
                Invalidate();
            }
            base.OnKeyDown(e);
        }

        private void GetColorBarRectangle(out Rectangle rect)
        {
            rect = new Rectangle(indent, 1, ClientRectangle.Width - indent * 2, ClientRectangle.Height - indent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            Brush brush = new LinearGradientBrush(
                new Point(ClientRectangle.Left + indent, 1 ),
                new Point(ClientRectangle.Right - indent, 1),
                startColor, endColor);

            Rectangle barRect;
            
            GetColorBarRectangle(out barRect);

            g.FillRectangle(brush, barRect);

            brush.Dispose();

            int x = barRect.Width * componentValue / 255;

            Point[] points = 
            {
                new Point(x, barRect.Height + indent),
                new Point(indent + x, barRect.Height),
                new Point(indent * 2 + x, barRect.Height + indent),
            };

            brush = new SolidBrush(SystemColors.WindowText);

            g.FillPolygon(brush, points);

            brush.Dispose();

            if (this.Focused)
            {
                Rectangle focusRect = barRect;
                focusRect.Inflate(1, 1);
                ControlPaint.DrawFocusRectangle(g, focusRect);
            }
        }

        private void RaiseChangedEvent()
        {
            EventHandler<EventArgs> handler = this.Changed;

            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        [Description("Color component controlled by the slider")]
        [Browsable(true)]
        public ColorComponent ColorComponent
        {
            get
            {
                return component;
            }
            set
            {
                component = value;

                switch (component)
                {
                    case ColorComponent.Red:
                        startColor = Color.FromArgb(0, 0, 0);
                        endColor = Color.FromArgb(255, 0, 0);
                        break;
                    case ColorComponent.Green:
                        startColor = Color.FromArgb(0, 0, 0);
                        endColor = Color.FromArgb(0, 255, 0);
                        break;
                    case ColorComponent.Blue:
                        startColor = Color.FromArgb(0, 0, 0);
                        endColor = Color.FromArgb(0, 0, 255);
                        break;
                    case ColorComponent.Alpha:
                        startColor = Color.FromArgb(0, 0, 0, 0);
                        endColor = Color.FromArgb(255, 0, 0, 0);
                        break;
                }

                Invalidate();
            }
        }

        [Browsable(false)]
        public int Value
        {
            get
            {
                return componentValue;
            }
            set
            {
                componentValue = value;

                if (componentValue < 0)
                    componentValue = 0;
                else if (componentValue > 255)
                    componentValue = 255;

                Invalidate();
            }
        }
    }

    public enum ColorComponent
    {
        Red,
        Green,
        Blue,
        Alpha
    }
}
