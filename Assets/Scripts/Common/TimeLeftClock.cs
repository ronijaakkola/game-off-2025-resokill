using UnityEngine;

public static class GameTime
{
    public static double GetTime()
    {
        return Time.timeAsDouble;
    }

    public static double GetTimeRealLife()
    {
        return Time.realtimeSinceStartupAsDouble;
    }
}

public class TimeLeftClock
{
    protected double _startTime;
    protected double _timeToTrack;

    protected bool _isStopped;
    protected double _timeElapsedWhenStopped = 0;

    // For manual changes
    protected double _addedElapsedTime = 0;

    public TimeLeftClock(double timeToTrack, bool startStopped = false, bool startTimeOver = false)
    {
        // NOTE: This can cause starting time to be negative, maybe causes problems?
        if (startTimeOver)
            _startTime = GameTime.GetTime() - timeToTrack;
        else
            _startTime = GameTime.GetTime();
        
        _timeToTrack = timeToTrack;

        _timeElapsedWhenStopped = 0;
        _isStopped = startStopped;
    }

    public virtual void ResetTimer(bool startStopped = false)
    {
        _startTime = GameTime.GetTime();

        _addedElapsedTime = 0;
        _timeElapsedWhenStopped = 0;
        _isStopped = startStopped;
    }

    // Set time to zero manually
    public void SetTimeToZero()
    {
        _timeToTrack = 0.0f;
    }

    public virtual double TimeElapsed()
    {
        if (_isStopped)
            return _timeElapsedWhenStopped; // TODO: Tsekkaa ett‰ toimii
        else
            return GameTime.GetTime() - (_startTime - _addedElapsedTime);
    }

    public virtual double TimeLeft()
    {
        if (_isStopped)
            return _timeToTrack - _timeElapsedWhenStopped; // TODO: Tsekkaa ett‰ toimii
        else
            return _timeToTrack - TimeElapsed();
    }

    public virtual float PercentagePassed()
    {
        if (IsTimeOver())
            return 1;
        else
            return (float)(TimeElapsed() / _timeToTrack);
    }

    public bool IsTimeOver(bool resetIfTrue = false)
    {
        double elapsedTime = TimeElapsed();
        bool timerOver = elapsedTime >= _timeToTrack;
        if (resetIfTrue && timerOver)
            ResetTimer();

        return timerOver;
    }

    public void StopTimer()
    {
        _timeElapsedWhenStopped = TimeElapsed();
        _isStopped = true;
    }

    public void ContinueTimer()
    {
        _isStopped = false;
        _startTime = GameTime.GetTime() - _timeElapsedWhenStopped;
        _timeElapsedWhenStopped = 0;
    }

    public void ChangeTimeToTrack(double timeToTrack, bool resetTimer)
    {
        _timeToTrack = timeToTrack;

        if (resetTimer)
            ResetTimer();
    }

    public void AddElapsedTime(double addedElapsedTime)
    {
        _addedElapsedTime = addedElapsedTime;
    }

    public bool IsTimerStopped => _isStopped;
}

public class TimeLeftClockRealTime : TimeLeftClock
{
    // TODO: Lis‰‰ stopped maybe

    public TimeLeftClockRealTime(double timeToTrack) : base(timeToTrack)
    {
        _startTime = GameTime.GetTimeRealLife();
    }

    public override void ResetTimer(bool startStopped = false)
    {
        _startTime = GameTime.GetTimeRealLife();
    }

    public override double TimeElapsed()
    {
        return GameTime.GetTimeRealLife() - _startTime;
    }

    public override float PercentagePassed()
    {
        return (float)(TimeElapsed() / _timeToTrack);
    }
}