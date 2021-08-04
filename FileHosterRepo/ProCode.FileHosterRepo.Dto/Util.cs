namespace ProCode.FileHosterRepo.Common
{
    public static class Util
    {
        const string outputRecognizer = "[FileHosterRepo output]> ";
        public static void Trace(string msg)
        {
            System.Diagnostics.Trace.WriteLine(outputRecognizer + msg); // Write in DebugView
            System.Console.WriteLine(outputRecognizer + msg);           // Write in browser's console
        }

        public static void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine(outputRecognizer + msg);
            System.Console.WriteLine(outputRecognizer + msg);
        }
    }
}
