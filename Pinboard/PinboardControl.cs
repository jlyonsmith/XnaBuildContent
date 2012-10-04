using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Playroom;
using System.Diagnostics;

namespace Jamoki.Tools.Pinboard
{
    public partial class PinboardControl : UserControl
    {
        private readonly Size minRectSize = new Size(12, 12);
        private DragOperation dragOp;
        private int selectedIndex;
        private Point dragOffset;
        private BufferedGraphics buffer;
        private Rectangle bufferRect;
        private BufferedGraphicsContext bufferContext;
        private bool bufferDirty;
        private int nextRectNum;
        private PinboardFileV1 data;
        private bool dataDirty;
        private PropertiesForm propertiesForm;
        
        public PinboardControl()
        {
            InitializeComponent();

            bufferContext = new BufferedGraphicsContext();
            SizeGraphicsBuffer();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            SetStyle(ControlStyles.DoubleBuffer, false);

            dragOp = DragOperation.None;
            nextRectNum = 0;
        }

        public PinboardFileV1 Data 
        {
            get
            {
                return data;
            }
            set
            {
                PinboardFileV1 oldData = data;
                
                data = value;
                selectedIndex = -1;
                DataDirty = (oldData != null);
            }
        }

        public bool DataDirty
        {
            get
            {
                if (Data == null)
                    return false;

                return dataDirty;
            }
            set
            {
                bool oldValue = dataDirty;

                dataDirty = value;

                if (dataDirty != oldValue)
                {
                    RaiseDataDirtiedEvent();
                }

                RaiseSelectionChangedEvent();
                DisplayDirty = true;
            }
        }

        public PropertiesForm PropertiesForm 
        {
            get
            {
                return propertiesForm;
            }
            set
            {
                propertiesForm = value;

                if (propertiesForm != null)
                    propertiesForm.ScreenSizeChanged += new EventHandler<ScreenSizeChangedEventArgs>(PropertiesForm_ScreenSizeChanged);
            }
        }

        public event EventHandler<EventArgs> SelectionChanged;

        public event EventHandler<EventArgs> DataDirtied;

        public PinboardFileV1.RectangleInfo Selection 
        { 
            get 
            {
                if (Data == null)
                    return null;

                if (selectedIndex == -1)
                    return Data.ScreenRectInfo;
                else
                    return Data.RectInfos[selectedIndex]; 
            } 
        }

        public int SelectionIndex
        {
            get
            {
                return selectedIndex;
            }
        }

        public DragOperation DragOperation
        {
            get
            {
                return dragOp;
            }
        }

        private void RaisePropertyChangedEvent(string propertyName)
        {
            EventHandler<EventArgs> handler = SelectionChanged;

            if (handler != null)
                handler(this, new EventArgs());
        }

        private void SizeGraphicsBuffer()
        {
            bufferRect = ClientRectangle;

            if (bufferRect.Width <= 0)
                return;

            if (bufferRect.Height <= 0)
                return;

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            if (bufferContext == null)
                return;

            using (Graphics graphics = CreateGraphics())
                buffer = bufferContext.Allocate(graphics, bufferRect);

            DisplayDirty = true;
        }
        
        private void PinboardControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int index = FindRectangle(e.Location);

                if (index == -1)
                {
                    selectedIndex = -1;
                    RaiseSelectionChangedEvent();
                    this.DisplayDirty = true;
                    return;
                }

                this.Capture = true;

                PinboardFileV1.RectangleInfo dragRectInfo = Data.RectInfos[index];
                Point[] points = this.GetCornerPoints(dragRectInfo.Rectangle);

                if (Vector.IsPointInTriangle(e.Location, points[0], points[1], points[2]))
                {
                    this.dragOffset = new Point(
                        dragRectInfo.X + dragRectInfo.Width - e.Location.X,
                        dragRectInfo.Y + dragRectInfo.Height - e.Location.Y);
                    dragOp = DragOperation.Sizing;
                    this.Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    this.dragOffset = new Point(
                        e.Location.X - dragRectInfo.Rectangle.X,
                        e.Location.Y - dragRectInfo.Rectangle.Y);
                    dragOp = DragOperation.Moving;
                    this.Cursor = Cursors.Hand;
                }

                selectedIndex = index;
                RaiseSelectionChangedEvent();
                this.DisplayDirty = true;
            }
        }

        private void PinboardControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (Capture == true)
            {
                switch (dragOp)
                {
                    case DragOperation.Moving:
                        SetRectanglePositionFromPoint(e.Location);
                        break;

                    case DragOperation.Sizing:
                        SetRectangleSizeFromPoint(e.Location);
                        break;
                }

                this.DataDirty = true;
            }
            else
            {
                int index = FindRectangle(e.Location);

                if (index != -1)
                {
                    Point[] points = GetCornerPoints(Data.RectInfos[index].Rectangle);

                    if (Vector.IsPointInTriangle(e.Location, points[0], points[1], points[2]))
                        this.Cursor = Cursors.SizeNWSE;
                    else
                        this.Cursor = Cursors.Hand;
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }

        private void PinboardControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.Capture)
            {
                switch (dragOp)
                {
                    case DragOperation.Moving:
                        SetRectanglePositionFromPoint(e.Location);
                        break;

                    case DragOperation.Sizing:
                        SetRectangleSizeFromPoint(e.Location);
                        break;
                }
             
                this.dragOp = DragOperation.None;
                this.DataDirty = true;
                this.Capture = false;
            }
        }

        private int FindRectangle(Point point)
        {
            int index = -1;

            for (int i = Data.RectInfos.Count - 1; i >= 0; i--)
            {
                if (Data.RectInfos[i].Rectangle.Contains(point))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public void SetRectangleSizeFromPoint(Point point)
        {
            PinboardFileV1.RectangleInfo selection = Data.RectInfos[selectedIndex];
            int x = point.X + dragOffset.X;
            int y = point.Y + dragOffset.Y;
            
            if (point.X > Data.ScreenRectInfo.Width)
            {
                x = Data.ScreenRectInfo.Width;
            }

            selection.Width = Math.Max(minRectSize.Width, x - selection.X);

            if (point.Y > Data.ScreenRectInfo.Height)
            {
                y = Data.ScreenRectInfo.Height;
            }

            selection.Height = Math.Max(minRectSize.Height, y - selection.Y);
        }

        public void SetRectanglePositionFromPoint(Point point)
        {
            PinboardFileV1.RectangleInfo selection = Data.RectInfos[selectedIndex];
            int x = point.X - dragOffset.X;
            int y = point.Y - dragOffset.Y;

            if (x < Data.ScreenRectInfo.X)
                x = Data.ScreenRectInfo.X;
            else if (x + selection.Width > Data.ScreenRectInfo.Width)
                x = Data.ScreenRectInfo.Width - selection.Width;
        
            Selection.X = x;
            
            if (y < Data.ScreenRectInfo.Y)
                y = Data.ScreenRectInfo.Y;
            else if (y + selection.Height > Data.ScreenRectInfo.Height)
                y = Data.ScreenRectInfo.Height - selection.Height;

            Selection.Y = y;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        { 
            /* Do Nothing */ 
        }

        public bool DisplayDirty
        {
            get 
            { 
                return bufferDirty; 
            }
            set
            {
                if (!value)
                    return;

                bufferDirty = true;
                Invalidate();
            }
        }
        
        protected override void OnSizeChanged(EventArgs e)
        {
            SizeGraphicsBuffer();

            if (Data != null && Data.ScreenRectInfo != null)
            {
                Data.ScreenRectInfo.Width = bufferRect.Width;
                Data.ScreenRectInfo.Height = bufferRect.Height;
            }

            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (buffer == null)
            {
                Draw(e.Graphics);
                return;
            }

            if (bufferDirty)
            {
                bufferDirty = false;
                Draw(buffer.Graphics);
            }

            buffer.Render(e.Graphics);
        }

        private void Draw(Graphics g)
        {
            if (bufferRect.IsEmpty)
                return;

            if (Data == null || Data.ScreenRectInfo == null)
            {
                g.FillRectangle(Brushes.White, bufferRect);
                return;
            }

            DrawPinboardFile(g, Data.ScreenRectInfo, selectedIndex == -1);

            for (int i = 0; i < Data.RectInfos.Count; i++)
            {
                PinboardFileV1.RectangleInfo rectInfo = Data.RectInfos[i];

                DrawPinboardFile(g, rectInfo, i == selectedIndex);
            }
        }

        private void DrawPinboardFile(Graphics g, PinboardFileV1.RectangleInfo rectInfo, bool selected)
        {
            using (SolidBrush brush = new SolidBrush(rectInfo.Color))
            {
                g.FillRectangle(brush, rectInfo.Rectangle);
            }

            float penWidth = 1.0f;
            using (Pen blackPen = new Pen(Color.FromArgb(rectInfo.Color.A, Color.Black), penWidth))
            {
                Rectangle rect = rectInfo.Rectangle;

                if (selected)
                {
                    using (Pen whitePen = new Pen(Color.FromArgb(rectInfo.Color.A, Color.White), penWidth))
                    {
                        using (Pen blackDottedPen = new Pen(Color.FromArgb(rectInfo.Color.A, Color.Black), penWidth))
                        {
                            DrawExactRectangle(g, whitePen, ref rect);

                            blackDottedPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

                            DrawExactRectangle(g, blackDottedPen, ref rect);

                            Rectangle expandedRect = rectInfo.Rectangle;

                            expandedRect.Inflate(2, 2);

                            DrawExactRectangle(g, blackDottedPen, ref expandedRect);

                            if (selectedIndex != -1)
                            {
                                Point[] points = GetCornerPoints(rect);

                                g.FillPolygon(Brushes.White, points);
                                g.DrawPolygon(Pens.Black, points);
                            }
                        }
                    }
                }
                else
                {
                    DrawExactRectangle(g, blackPen, ref rect);
                }

                int margin = 5;
                RectangleF textRect = new Rectangle(
                    rectInfo.X + margin, rectInfo.Y + margin,
                    Math.Max(rectInfo.Width - margin, 0), Math.Max(rectInfo.Height - margin, 0));

                if (!textRect.IsEmpty)
                {
                    using (StringFormat format = new StringFormat(StringFormatFlags.NoWrap))
                    {
                        g.DrawString(rectInfo.Name, this.Font, Brushes.Black, textRect, format);
                    }
                }
            }
        }

        private void DrawExactRectangle(Graphics g, Pen pen, ref Rectangle rect)
        {
            float shrinkAmount = pen.Width / 2;

            g.DrawRectangle(pen, 
                rect.X + shrinkAmount,  
                rect.Y + shrinkAmount, 
                rect.Width - pen.Width,
                rect.Height - pen.Width);
        }

        private Point[] GetCornerPoints(Rectangle rect)
        {
            Point[] points = new Point[3];
            const int maxCornerSize = 10;
            int cornerSize = Math.Min(maxCornerSize, Math.Min(rect.Width, rect.Height));

            points[0] = new Point(rect.Right - cornerSize, rect.Bottom - 1);
            points[1] = new Point(rect.Right - 1, rect.Bottom - cornerSize);
            points[2] = new Point(rect.Right - 1, rect.Bottom - 1);

            return points;
        }

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    return true;

                default:
                    return base.IsInputKey(keyData);
            }
        }

        private void PinboardControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (selectedIndex != -1 && dragOp == DragOperation.None)
            {
                switch (e.KeyCode)
                {
                    case Keys.PageUp:
                        if (e.Modifiers == Keys.None && selectedIndex < Data.RectInfos.Count - 1)
                        {
                            PinboardFileV1.RectangleInfo rectInfo = Data.RectInfos[selectedIndex];

                            Data.RectInfos[selectedIndex] = Data.RectInfos[selectedIndex + 1];
                            selectedIndex++;
                            Data.RectInfos[selectedIndex] = rectInfo;
                            DataDirty = true;
                        }
                        break;

                    case Keys.PageDown:
                        if (e.Modifiers == Keys.None && selectedIndex > 0)
                        {
                            PinboardFileV1.RectangleInfo rectInfo = Data.RectInfos[selectedIndex];

                            Data.RectInfos[selectedIndex] = Data.RectInfos[selectedIndex - 1];
                            selectedIndex--;
                            Data.RectInfos[selectedIndex] = rectInfo;
                            DataDirty = true;
                        }
                        break;

                    case Keys.Right:
                        if (e.Modifiers == Keys.None && Selection.X + Selection.Width < Data.ScreenRectInfo.Width)
                        {
                            Selection.X++;
                            DataDirty = true;
                        }
                        else if ((e.Modifiers & Keys.Control) != 0 && Selection.Width < Data.ScreenRectInfo.Width - Selection.X)
                        {
                            Selection.Width++;
                            DataDirty = true;
                        }
                        break;

                    case Keys.Left:
                        if (e.Modifiers == Keys.None && Selection.X > Data.ScreenRectInfo.X)
                        {
                            Selection.X--;
                            DataDirty = true;
                        }
                        else if ((e.Modifiers & Keys.Control) != 0 && Selection.Width > 1)
                        {
                            Selection.Width--;
                            DataDirty = true;
                        }
                        break;

                    case Keys.Up:
                        if (e.Modifiers == Keys.None && Selection.Y > Data.ScreenRectInfo.Y)
                        {
                            Selection.Y--;
                            DataDirty = true;
                        }
                        else if ((e.Modifiers & Keys.Control) != 0 && Selection.Height > 1)
                        {
                            Selection.Height--;
                            DataDirty = true;
                        }
                        break;

                    case Keys.Down:
                        if (e.Modifiers == Keys.None && Selection.Y + Selection.Height < Data.ScreenRectInfo.Height)
                        {
                            Selection.Y++;
                            DataDirty = true;
                        }
                        else if ((e.Modifiers & Keys.Control) != 0 && Selection.Height < Data.ScreenRectInfo.Height - Selection.Y)
                        {
                            Selection.Height++;
                            DataDirty = true;
                        }
                        break;
                }
            }
        }

        public void DeleteSelection()
        {
            if (selectedIndex != -1)
            {
                Data.RectInfos.RemoveAt(selectedIndex);
                selectedIndex = -1;
                DataDirty = true;
                RaiseSelectionChangedEvent();
            }
        }

        public void DuplicateSelection()
        {
            if (selectedIndex != -1)
            {
                PinboardFileV1.RectangleInfo rectInfo = new PinboardFileV1.RectangleInfo(Data.RectInfos[selectedIndex]);

                rectInfo.Name += "_Copy";
                Data.RectInfos.Insert(selectedIndex, rectInfo);
                Data.RectInfos.Reverse(selectedIndex, 2);
                DataDirty = true;
                selectedIndex++;
                RaiseSelectionChangedEvent();
            }
        }

        public void NewRectangle()
        {
            this.Data.RectInfos.Add(
                new PinboardFileV1.RectangleInfo(new Rectangle(0, 0, 100, 100), "Rectangle" + nextRectNum.ToString()));

            nextRectNum++;
            selectedIndex = Data.RectInfos.Count - 1;
            RaiseSelectionChangedEvent();
            this.DataDirty = true;
        }

        private void PropertiesForm_ScreenSizeChanged(object sender, ScreenSizeChangedEventArgs args)
        {
            // TODO: Move this into a Resize menu item
            SizeF scale = new SizeF(
                (float)Data.ScreenRectInfo.Size.Width / args.OldSize.Width, 
                (float)Data.ScreenRectInfo.Size.Height / args.OldSize.Height);

            foreach (var rectInfo in Data.RectInfos)
            {
                rectInfo.X = (int)(rectInfo.X * scale.Width);
                rectInfo.Y = (int)(rectInfo.Y * scale.Height);
                rectInfo.Width = (int)(rectInfo.Width * scale.Width);
                rectInfo.Height = (int)(rectInfo.Height * scale.Height);
            }
            
            // NOTE: Buffer and data MUST already be marked as dirty elsewhere.
        }

        private void RaiseSelectionChangedEvent()
        {
            EventHandler<EventArgs> handler = this.SelectionChanged;

            if (handler != null)
                handler(this, new EventArgs());
        }

        private void RaiseDataDirtiedEvent()
        {
            EventHandler<EventArgs> handler = this.DataDirtied;

            if (handler != null)
                handler(this, new EventArgs());
        }

        private void PinboardControl_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void PinboardControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
    }

    public enum DragOperation
    {
        None,
        Moving,
        Sizing
    }

}

