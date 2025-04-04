using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Octopus.CommandLine.Plumbing;

[SuppressMessage("ReSharper", "DE0009")] // suppresses a deprecation warning for Environment.OSVersion
[SuppressMessage("ReSharper", "DE0007")] // suppresses a deprecation warning for PlatformID.Win*
[SuppressMessage("ReSharper", "PC003")] // suppresses a warning that libc!uname is unavailable in UWP
static class ExecutionEnvironment
{
    /// <summary>
    /// Based on some internal methods used my mono itself
    /// https://github.com/mono/mono/blob/master/mcs/class/corlib/System/Environment.cs
    /// </summary>
    public static bool IsRunningOnNix => Environment.OSVersion.Platform == PlatformID.Unix && !IsRunningOnMac;

    public static bool IsRunningOnWindows => Environment.OSVersion.Platform == PlatformID.Win32NT ||
        Environment.OSVersion.Platform == PlatformID.Win32S ||
        Environment.OSVersion.Platform == PlatformID.Win32Windows ||
        Environment.OSVersion.Platform == PlatformID.WinCE;

    public static bool IsRunningOnMac
    {
        get
        {
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                return true;
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return false;

            var buf = IntPtr.Zero;
            try
            {
                buf = Marshal.AllocHGlobal(8192);
                // Get sysname from uname ()
                if (uname(buf) == 0)
                {
                    var os = Marshal.PtrToStringAnsi(buf);
                    if (os == "Darwin")
                        return true;
                }
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (buf != IntPtr.Zero)
                    Marshal.FreeHGlobal(buf);
            }

            return false;
        }
    }

    //from https://github.com/jpobst/Pinta/blob/master/Pinta.Core/Managers/SystemManager.cs#L162
    //(MIT license)
    [DllImport("libc")] //From Managed.Windows.Forms/XplatUI
    static extern int uname(IntPtr buf);
}