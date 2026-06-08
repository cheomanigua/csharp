using System;

namespace Core;

public static class DebugLog
{
    public static bool Enabled = false; // Toggle this from Program.cs

    public static void Log(string message)
    {
        if (Enabled) Console.WriteLine($"[DEBUG] {message}");
    }
}
