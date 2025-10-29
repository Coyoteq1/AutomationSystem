using System;

namespace CrowbaneArena.Scripts
{
    /// <summary>
    /// Debugging tools for the mod.
    /// </summary>
    public static class DebugTools
    {
        // Using a string identifier instead of an undefined Entity type to keep this utility self-contained.
        public static void PrintProgression(string playerIdentifier)
        {
            Console.WriteLine("Debug: Progression for player " + playerIdentifier);
        }

        public static void ToggleDebugMode()
        {
            Console.WriteLine("Debug mode toggled.");
        }

        public static void RunTests()
        {
            Console.WriteLine("Running tests...");
        }
    }
}
