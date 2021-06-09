namespace ProCode.FileHosterRepo.WebAppBlazor
{
    public static class Util
    {
        public static void Trace(string msg)
        {
            System.Diagnostics.Trace.WriteLine("[FileHosterRepo output]> " + msg);
        }

        public static void Debug(string msg)
        {
            System.Diagnostics.Debug.WriteLine("[FileHosterRepo output]> " + msg);
        }
    }
}
