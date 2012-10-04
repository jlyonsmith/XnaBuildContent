using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
#if WINDOWS
using Microsoft.Win32;
#endif
using System.IO;

namespace XnaBuildContent
{
    public static class ToolPaths
    {
        public static ParsedPath Inkscape { get; set; }
        public static ParsedPath RSvgConvert { get; set; }

        static ToolPaths()
        {
            try
            {
#if WINDOWS
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"svgfile\shell\Inkscape\command", false);

                if (key != null)
                {
                    string s = (string)key.GetValue("");

                    if (s != null && s.Length > 0)
                    {
                        if (s[0] == '"')
                            s = s.Substring(1, s.IndexOf('"', 1) - 1);

                        ParsedPath path  = new ParsedPath(s, PathType.File).WithExtension(".com");

                        if (File.Exists(path))
                            Inkscape = path;
                    }
                }

                key = Registry.ClassesRoot.OpenSubKey(@"svgfile\shell\Edit with GIMP\command", false);

                if (key != null)
                {
                    string s = (string)key.GetValue("");

                    if (s != null && s.Length > 0)
                    {
                        if (s[0] == '"')
                            s = s.Substring(1, s.IndexOf('"', 1) - 1);

                        ParsedPath path = new ParsedPath(s, PathType.File).WithFileAndExtension("rsvg-convert.exe");

                        if (File.Exists(path))
                            RSvgConvert = path;
                    }
                }
#elif MACOS
				ParsedPath path = new ParsedPath("/Applications/Inkscape.app/Contents/Resources/bin/inkscape", PathType.File);

				if (File.Exists(path))
					Inkscape = path;
#endif
            }
            catch
            {
            }
        }
    }
}

