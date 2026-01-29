namespace RpcJsonContextGen
{
    internal static class ClipboardUtil
    {
        public static bool TrySetText(string text)
        {
            try
            {
                // best-effort using pbcopy/xclip/wl-copy/clip.
                if (TryPipeToProcess("pbcopy", "", text)) return true;
                if (TryPipeToProcess("clip", "", text)) return true;
                if (TryPipeToProcess("wl-copy", "", text)) return true;
                if (TryPipeToProcess("xclip", "-selection clipboard", text)) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryPipeToProcess(string fileName, string arguments, string text)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo(fileName, arguments)
                {
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = System.Diagnostics.Process.Start(psi);
                if (p is null) return false;
                p.StandardInput.Write(text);
                p.StandardInput.Close();
                p.WaitForExit(2000);
                return p.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
