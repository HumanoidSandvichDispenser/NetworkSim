namespace NetworkSim;

public class Timer : Entity
{
    public float Duration { get; set; } = 0f;

    public float CurrentTime { get; private set; } = 0f;

    public event Action? Timeout;

    public bool IsRepeating { get; set; } = false;

    public bool IsRunning { get; set; } = false;

    public void Start(float? duration = null)
    {
        if (duration.HasValue)
        {
            Duration = duration.Value;
        }

        CurrentTime = Duration;
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public override void Update(float delta)
    {
        if (!IsRunning)
        {
            return;
        }

        CurrentTime -= delta;
        if (CurrentTime <= 0f)
        {
            Timeout?.Invoke();

            if (IsRepeating)
            {
                CurrentTime = Duration;
            }
            else
            {
                IsRunning = false;
            }
        }
    }
}
