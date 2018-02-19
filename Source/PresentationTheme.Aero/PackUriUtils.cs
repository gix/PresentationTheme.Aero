namespace PresentationTheme.Aero
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.IO.Packaging;

    /// <summary>
    ///   Contains utilities for WPF Pack URIs.
    /// </summary>
    public static class PackUriUtils
    {
        private const string PackageApplicationBaseUriEscaped = "application:///";

        /// <summary>
        ///   Gets the Pack URI authority for content files.
        /// </summary>
        public static Uri ContentFileAuthority { get; } = new Uri(PackageApplicationBaseUriEscaped);

        /// <summary>Creates a Pack URI for a content file.</summary>
        /// <param name="assemblyName">
        ///   The assembly name where the content resides.
        /// </param>
        /// <param name="path">The relative pack URI of the file.</param>
        /// <returns>An absolute content file Pack URI.</returns>
        public static Uri MakeContentPackUri(AssemblyName assemblyName, string path)
        {
            var name = FormatName(assemblyName);
            return PackUriHelper.Create(
                ContentFileAuthority,
                new Uri($"/{name};component/{path}", UriKind.Relative));
        }

        private static string FormatName(AssemblyName name)
        {
            return $"{name.Name};v{name.Version}{GetPublicKeySegment(name)}";
        }

        private static string GetPublicKeySegment(AssemblyName name)
        {
            var bytes = name.GetPublicKeyToken();
            if (bytes.Length == 0)
                return string.Empty;

            var builder = new StringBuilder(1 + bytes.Length * 2);
            builder.Append(';');
            foreach (var b in bytes)
                builder.AppendFormat("{0:x2}", b);

            return builder.ToString();
        }

        internal static bool IsPackApplicationUri(Uri uri)
        {
            return
                uri != null &&

                // Is the "outer" URI absolute?
                uri.IsAbsoluteUri &&

                // Does the "outer" URI have the pack: scheme?
                string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase) &&

                // Does the "inner" URI have the application: scheme?
                string.Equals(
                    PackUriHelper.GetPackageUri(uri).GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped),
                    PackageApplicationBaseUriEscaped,
                    StringComparison.OrdinalIgnoreCase);
        }

        internal static void GetAssemblyAndPartNameFromPackAppUri(
            Uri uri, out AssemblyName assemblyName, out string partName)
        {
            if (!IsPackApplicationUri(uri))
                throw new ArgumentException("Invalid pack application uri.", nameof(uri));

            // Generate a relative Uri which gets rid of the pack://application:,,, authority part.
            var partUri = new Uri(uri.AbsolutePath, UriKind.Relative);

            GetAssemblyNameAndPart(partUri, out assemblyName, out partName);

            if (assemblyName == null) {
                // The uri doesn't contain ";component". it should map to the enty application assembly.
                assemblyName = Assembly.GetEntryAssembly().GetName();

                // The partName returned from GetAssemblyNameAndPart should be escaped.
                Debug.Assert(string.Equals(partName, uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped), StringComparison.OrdinalIgnoreCase));
            }
        }

        private const string WrongFirstSegmentMessage =
            "The required pattern for URI containing \";component\" is \"AssemblyName;Vxxxx;PublicKey;component\", where Vxxxx is the assembly version and PublicKey is the 16-character string representing the assembly public key token. Vxxxx and PublicKey are optional.";
        private const string ComponentSuffix = ";component";
        private const string VersionPrefix = "v";
        private const char ComponentDelimiter = ';';

        private static void GetAssemblyNameAndPart(
            Uri uri, out AssemblyName asmName, out string partName)
        {
            Debug.Assert(uri != null && !uri.IsAbsoluteUri, "This method accepts relative uri only.");

            string original = uri.ToString(); // only relative Uri here (enforced by Package)

            // Start and end points for the first segment in the Uri.
            int start = 0;
            int end;

            if (original[0] == '/')
                start = 1;

            asmName = null;
            partName = original.Substring(start);

            end = original.IndexOf('/', start);

            string firstSegment = string.Empty;
            bool hasComponent = false;

            if (end > 0) {
                firstSegment = original.Substring(start, end - start);

                if (firstSegment.EndsWith(ComponentSuffix, StringComparison.OrdinalIgnoreCase)) {
                    partName = original.Substring(end + 1);
                    hasComponent = true;
                }
            }

            if (!hasComponent)
                return;

            string[] assemblyInfo = firstSegment.Split(ComponentDelimiter);

            int count = assemblyInfo.Length;
            if (count < 2 || count > 4)
                throw new UriFormatException(WrongFirstSegmentMessage);

            // If the uri contains escaping character,
            // Convert it back to normal unicode string
            // so that the string as assembly name can be
            // recognized by Assembly.Load later.
            string assemblyName = Uri.UnescapeDataString(assemblyInfo[0]);
            string assemblyVersion = string.Empty;
            string assemblyKey = string.Empty;

            for (int i = 1; i < count - 1; ++i) {
                if (assemblyInfo[i].StartsWith(VersionPrefix, StringComparison.OrdinalIgnoreCase)) {
                    if (!string.IsNullOrEmpty(assemblyVersion))
                        throw new UriFormatException(WrongFirstSegmentMessage);
                    assemblyVersion = assemblyInfo[i].Substring(1); // Get rid of the leading "v"
                } else {
                    if (!string.IsNullOrEmpty(assemblyKey))
                        throw new UriFormatException(WrongFirstSegmentMessage);
                    assemblyKey = assemblyInfo[i];
                }
            }

            if (!string.IsNullOrEmpty(assemblyName)) {
                asmName = new AssemblyName(assemblyName);

                // We always use the primary assembly (culture neutral) for resource
                // manager. If the required resource lives in a satellite assembly,
                // ResourceManager can find the right satellite assembly later.
                asmName.CultureInfo = new CultureInfo(string.Empty);

                if (!string.IsNullOrEmpty(assemblyVersion))
                    asmName.Version = new Version(assemblyVersion);

                if (!string.IsNullOrEmpty(assemblyKey)) {
                    int byteCount = assemblyKey.Length / 2;
                    byte[] keyToken = new byte[byteCount];
                    for (int i = 0; i < byteCount; ++i) {
                        string byteString = assemblyKey.Substring(i * 2, 2);
                        keyToken[i] = byte.Parse(byteString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    }

                    asmName.SetPublicKeyToken(keyToken);
                }
            }
        }
    }
}
