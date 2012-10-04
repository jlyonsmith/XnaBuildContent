//
// This file genenerated by the Buckle tool on 9/4/2012 at 10:19 AM. 
//
// Contains strongly typed wrappers for resources in PlayroomResources.resx
//

namespace XnaBuildContent {
using System;
using System.Reflection;
using System.Resources;
using System.Diagnostics;
using System.Globalization;


/// <summary>
/// Strongly typed resource wrappers generated from PlayroomResources.resx.
/// </summary>
public class PlayroomResources
{
    internal static readonly ResourceManager ResourceManager = new ResourceManager(typeof(PlayroomResources));

    /// <summary>
    /// File '{0}' does not exist
    /// </summary>
    public static ToolBelt.Message FileNotFound(object param0)
    {
        Object[] o = { param0 };
        return new ToolBelt.Message("FileNotFound", typeof(PlayroomResources), ResourceManager, o);
    }
}
}
