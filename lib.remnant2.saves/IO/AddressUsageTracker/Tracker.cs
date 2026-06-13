namespace lib.remnant2.saves.IO.AddressUsageTracker;

// This is a helper class for the ReaderBase to check if we have any unread gaps
public class Tracker
{
    private readonly SortedDictionary<int,AddressRange> _ranges = [];

    // The range the previous read extended. A forward, contiguous read just grows this in place,
    // so the common case touches no dictionary and allocates nothing.
    private AddressRange? _last;

    public Tracker() { }

    public Tracker(Tracker tracker)
    {
        _ranges = new(tracker._ranges);
    }

    public void AddRange(int start, int end)
    {
        // Fast path: a forward, contiguous read (the parser reads sequentially within a region).
        // Grow the current range in place - it is the same object stored in _ranges, so this
        // needs no dictionary operation and no allocation.
        if (_last != null && start == _last.End)
        {
            if (end > _last.End) _last.End = end;
            return;
        }

        // A seek to another region starts a new range. Ranges are merged lazily in GetRanges()
        // rather than after every read.
        if (_ranges.TryGetValue(start, out AddressRange? existing))
        {
            if (end > existing.End) existing.End = end;
            _last = existing;
        }
        else
        {
            AddressRange range = new() { Begin = start, End = end };
            _ranges[start] = range;
            _last = range;
        }
    }

    // Collapse adjacent / overlapping ranges into the minimal set of disjoint ranges. The parser
    // reads regions out of order (e.g. the object-data region grows until it meets the
    // already-read object table), so the raw ranges connect up. Returns a fresh dictionary and
    // does not mutate the accumulated ranges.
    public SortedDictionary<int, AddressRange> GetRanges()
    {
        SortedDictionary<int, AddressRange> merged = [];
        AddressRange? previous = null;
        foreach (AddressRange current in _ranges.Values)
        {
            if (previous == null || previous.End < current.Begin)
            {
                previous = new() { Begin = current.Begin, End = current.End };
                merged[previous.Begin] = previous;
            }
            else if (current.End > previous.End)
            {
                previous.End = current.End;
            }
        }
        return merged;
    }
}
