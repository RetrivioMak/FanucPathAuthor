namespace FanucPathAuthor.Services;

public enum SimulationState
{
    Stopped,
    Playing,
    Paused
}

public sealed class PathSimulationController : IDisposable
{
    private readonly System.Windows.Forms.Timer _timer;
    private PathSimulationTimeline? _timeline;
    private double _elapsed_s;
    private SimulationPose _pose = new();
    private readonly List<(double X, double Y, bool GlueOn)> _trail = new();
    private (double X, double Y)? _lastTrailPoint;

    public PathSimulationController()
    {
        _timer = new System.Windows.Forms.Timer { Interval = 16 };
        _timer.Tick += (_, _) => AdvanceFrame();
    }

    public SimulationState State { get; private set; } = SimulationState.Stopped;

    public double SpeedMultiplier { get; set; } = 1.0;

    public SimulationPose CurrentPose => _pose;

    public IReadOnlyList<(double X, double Y, bool GlueOn)> Trail => _trail;

    public event EventHandler? StateChanged;

    public event EventHandler? FrameAdvanced;

    public void LoadTimeline(PathSimulationTimeline timeline)
    {
        _timeline = timeline;
        ResetPlayback();
    }

    public void Play()
    {
        if (_timeline == null || _timeline.Moves.Count == 0)
        {
            return;
        }

        if (_pose.IsComplete)
        {
            ResetPlayback();
        }

        State = SimulationState.Playing;
        _timer.Start();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Pause()
    {
        if (State != SimulationState.Playing)
        {
            return;
        }

        _timer.Stop();
        State = SimulationState.Paused;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        _timer.Stop();
        ResetPlayback();
        State = SimulationState.Stopped;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TogglePlayPause()
    {
        if (State == SimulationState.Playing)
        {
            Pause();
            return;
        }

        Play();
    }

    private void ResetPlayback()
    {
        _elapsed_s = 0;
        _trail.Clear();
        _lastTrailPoint = null;
        _pose = _timeline?.GetPoseAtTime(0) ?? new SimulationPose();
        AppendTrailPoint(_pose.LocalX, _pose.LocalY, _pose.GlueOn);
        FrameAdvanced?.Invoke(this, EventArgs.Empty);
    }

    private void AdvanceFrame()
    {
        if (_timeline == null)
        {
            return;
        }

        _elapsed_s += _timer.Interval / 1000.0 * Math.Max(SpeedMultiplier, 0.1);
        _pose = _timeline.GetPoseAtTime(_elapsed_s);
        AppendTrailPoint(_pose.LocalX, _pose.LocalY, _pose.GlueOn);
        FrameAdvanced?.Invoke(this, EventArgs.Empty);

        if (_pose.IsComplete)
        {
            _timer.Stop();
            State = SimulationState.Paused;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void AppendTrailPoint(double x, double y, bool glueOn)
    {
        if (_lastTrailPoint is (double lastX, double lastY))
        {
            if (Distance(lastX, lastY, x, y) < 0.5)
            {
                return;
            }
        }

        _trail.Add((x, y, glueOn));
        _lastTrailPoint = (x, y);
    }

    private static double Distance(double x1, double y1, double x2, double y2)
    {
        double dx = x2 - x1;
        double dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Dispose();
    }
}
