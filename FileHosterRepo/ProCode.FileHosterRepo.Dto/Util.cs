namespace ProCode.FileHosterRepo.Common
{
    public static class Util
    {
        const string outputRecognizer = "[FileHosterRepo output]> ";
        public static void Trace(string msg)
        {
            System.Diagnostics.Trace.WriteLine(outputRecognizer + msg);
        }

        public static void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine(outputRecognizer + msg);
        }
    }
}
