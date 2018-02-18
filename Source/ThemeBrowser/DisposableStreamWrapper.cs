namespace ThemeBrowser
{
    using System;
    using System.IO;

    public sealed class DisposableStreamWrapper : Stream
    {
        private Stream stream;

        public DisposableStreamWrapper(Stream stream)
        {
            this.stream = stream;
        }

        protected override void Dispose(bool disposing)
        {
            stream = null;
            base.Dispose(disposing);
        }

        public override bool CanRead
        {
            get
            {
                if (stream == null)
                    throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
                return stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (stream == null)
                    throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
                return stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (stream == null)
                    throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
                return stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (stream == null)
                    throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
                return stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                if (stream == null)
                    throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
                return stream.Position;
            }
            set
            {
                if (stream == null)
                    throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
                stream.Position = value;
            }
        }

        public override void Flush()
        {
            if (stream == null)
                throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (stream == null)
                throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (stream == null)
                throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
            stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (stream == null)
                throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
            return stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (stream == null)
                throw new ObjectDisposedException(nameof(DisposableStreamWrapper));
            stream.Write(buffer, offset, count);
        }
    }
}
