namespace ThemePreviewer
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public static class ProcessExtensions
    {
        /// <summary>
        ///   Waits until the main window of the specified process is created.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <returns>
        ///   <c>true</c> if the process runs and the main window is created;
        ///   otherwise <c>false</c>.
        /// </returns>
        public static bool WaitForMainWindow(this Process process)
        {
            while (!process.HasExited && process.MainWindowHandle == IntPtr.Zero)
                Thread.Sleep(10);
            return !process.HasExited;
        }

        /// <summary>
        ///   Waits until the main window of the specified process is created.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="timeout">
        ///   A value of 1 to <see cref="int.MaxValue"/> that specifies the amount
        ///   of time, in milliseconds, to wait for the associated process to
        ///   create its main window. A value of 0 specifies an immediate return,
        ///   and a value of -1 specifies an infinite wait.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the process runs and the main window is created;
        ///   otherwise <c>false</c>.
        /// </returns>
        public static bool WaitForMainWindow(this Process process, int timeout)
        {
            var start = DateTime.Now;
            while (!process.HasExited && process.MainWindowHandle == IntPtr.Zero) {
                Thread.Sleep(10);
                if ((DateTime.Now - start).TotalMilliseconds >= timeout)
                    return false;
            }

            return !process.HasExited;
        }
    }
}
