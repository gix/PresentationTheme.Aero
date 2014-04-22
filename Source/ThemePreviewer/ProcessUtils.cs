namespace ThemePreviewer
{
    using System.Collections.Generic;
    using System.Text;

    public static class ProcessUtils
    {
        public static string GetCommandLineArgs(params string[] args)
        {
            return GetCommandLineArgs((IEnumerable<string>)args);
        }

        public static string GetCommandLineArgs(IEnumerable<string> args)
        {
            var builder = new StringBuilder();

            int idx = 0;
            foreach (var arg in args) {
                if (idx++ > 0)
                    builder.Append(' ');
                AppendQuotedArg(builder, arg, false);
            }

            return builder.ToString();
        }

        /// <summary>
        ///   Appends the given argument to a command line such that CommandLineToArgvW
        ///   will return the argument string unchanged. Arguments in a command
        ///   line should be separated by spaces; this function does not add
        ///   these spaces.
        /// </summary>
        /// <param name="commandLine">
        ///   Supplies the command line to which we append the encoded argument
        ///   string.
        /// </param>
        /// <param name="argument">Supplies the argument to encode.</param>
        /// <param name="force">
        ///   Supplies an indication of whether we should quote the argument
        ///   even if it does not contain any characters that would ordinarily
        ///   require quoting.
        /// </param>
        public static void AppendQuotedArg(StringBuilder commandLine, string argument, bool force)
        {
            // Unless we're told otherwise, don't quote unless we actually
            // need to do so --- hopefully avoid problems if programs won't
            // parse quotes properly
            if (!force &&
                argument.Length != 0 &&
                argument.IndexOfAny(new[] { ' ', '\t', '\n', '\v', '"' }) == -1) {
                commandLine.Append(argument);
                return;
            }

            commandLine.Append('"');

            for (int i = 0; ; ++i) {
                int backslashCount = 0;
                while (i != argument.Length && argument[i] == '\\') {
                    ++i;
                    ++backslashCount;
                }

                if (i == argument.Length) {
                    // Escape all backslashes, but let the terminating
                    // double quotation mark we add below be interpreted
                    // as a metacharacter.
                    commandLine.Append('\\', backslashCount * 2);
                    break;
                }

                if (argument[i] == '"') {
                    // Escape all backslashes and the following
                    // double quotation mark.
                    commandLine.Append('\\', backslashCount * 2 + 1);
                    commandLine.Append(argument[i]);
                } else {
                    // Backslashes aren't special here.
                    commandLine.Append('\\', backslashCount);
                    commandLine.Append(argument[i]);
                }
            }

            commandLine.Append('"');
        }
    }
}
