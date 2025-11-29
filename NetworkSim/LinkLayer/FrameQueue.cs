namespace NetworkSim.LinkLayer;

public class FrameQueue
{
    private readonly Queue<Frame> _queue = new();

    public uint BufferSize { get; init; }

    public int Count => _queue.Count;

    public FrameQueue(uint bufferSize)
    {
        BufferSize = bufferSize;
    }

    public bool TryEnqueue(Frame frame)
    {
        if (_queue.Sum(f => f.Size) + frame.Size > BufferSize)
        {
            // drop the frame
            return false;
        }

        _queue.Enqueue(frame);
        return true;
    }

    public Frame Dequeue()
    {
        return _queue.Dequeue();
    }

    public bool TryDequeue(out Frame? frame)
    {
        return _queue.TryDequeue(out frame);
    }
}
