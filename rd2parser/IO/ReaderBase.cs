using System.Runtime.CompilerServices;
using rd2parser.IO.AddressUsageTracker;

namespace rd2parser.IO;

// This is a utility class for reading from a byte array
// Node that the entire buffer resides in memory so it's not suitable
// for large files, for our purposes though it's fine
public class ReaderBase
{
    public ReaderBase(byte[] buffer)
    {
        _buffer = buffer;
    }

    private readonly byte[] _buffer;
    private int _index;
    private readonly Tracker _tracker = new();
    private bool _trackerActive;

    public int Position
    {
        get => _index;
        set
        {
            if (value < 0 || value > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _index = value;
        }
    }

    public int Size => _buffer.Length;

    public T Read<T>()
    {
        int size = Unsafe.SizeOf<T>();
        if (_trackerActive)
        {
            _tracker.AddRange(_index, _index + size);
        }
        _index += size;
        return Unsafe.ReadUnaligned<T>(ref _buffer[_index - size]);
    }

    public byte[] ReadBytes(int size)
    {
        byte[] result = new byte[size];
        Array.Copy(_buffer, _index, result, 0, size);
        if (_trackerActive)
        {
            _tracker.AddRange(_index, _index + size);
        }
        _index += size;
        return result;
    }

    public bool IsEof => _index >= _buffer.Length;

    public void ActivateTracker()
    {
        _trackerActive = true;
    }

    public Tracker GetTracker()
    {
        return new Tracker(_tracker);
    }
}
