using System.Runtime.CompilerServices;

namespace rd2parser.IO
{
    public  class WriterBase
    {
        private readonly MemoryStream _stream = new();
        public void Write<T>(T value)
        {

            byte[] buf = new byte[Unsafe.SizeOf<T>()];
            Unsafe.WriteUnaligned(ref buf[0], value);
            WriteBytes(buf);
        }
        public void WriteBytes(byte[] value)
        {
            _stream.Write(new ReadOnlySpan<byte>(value));
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }

        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public MemoryStream Stream => _stream;
    }
}
