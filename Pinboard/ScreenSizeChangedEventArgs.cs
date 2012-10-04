using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Jamoki.Tools.Pinboard
{
    public class ScreenSizeChangedEventArgs : EventArgs
    {
        public ScreenSizeChangedEventArgs(Size oldSize)
        {
            OldSize = oldSize;
        }

        public Size OldSize { get; set; }
    }
}
