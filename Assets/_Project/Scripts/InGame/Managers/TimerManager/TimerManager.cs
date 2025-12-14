using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LocalTimer : Timer
{
    public LocalTimer(float duration) : base(duration)
    {
    }

    protected override float GetWorldTime()
    {
        return Time.time;
    }
}

public class RealTimeTimer : Timer
{
    public RealTimeTimer(float duration) : base(duration)
    {
    }

    protected override float GetWorldTime()
    {
        return Time.realtimeSinceStartup;
    }
}

public class ReusableTimer : Timer
{
    public ReusableTimer(float duration) : base(duration)
    {
    }

    public override Timer Start()
    {
        startTime = GetWorldTime();
        IsCompleted = false;
        IsCancelled = false;
        timeElapsedBeforePause = null;
        cleanupCalled = false;
        onStart?.Invoke();
        return this;
    }
}

public class OneShotTimer : Timer
{
    protected OneShotTimer(float duration) : base(duration)
    {
    }

    public override Timer Start()
    {
        if (IsRegistered || IsDone)
        {
            Debug.LogWarning("Timer is already registered or done. Please use ReusableTimer instead.");
            return this;
        }

        startTime = GetWorldTime();
        manager.RegisterTimer(this);
        onStart?.Invoke();
        return this;
    }
}


public class Timer : IDisposable
{
    protected static TimerManager manager;

    private CancellationToken? cancellationToken;
    private CancellationTokenRegistration tokenRegistration;

    private readonly float duration;
    protected float startTime;
    protected float? timeElapsedBeforePause;

    protected Action onStart;
    private Action<float> onUpdate;
    private Action<float> onProgress;
    private Action<float> onRemaining;
    private Action<float> onTimeRemaining;

    private Action onComplete;
    private Action onCancel;
    private Action onDone;

    public bool IsRegistered { get; private set; }
    public float Duration => duration;
    public bool IsCompleted { get; protected set; }
    public bool IsPaused => timeElapsedBeforePause.HasValue;
    public bool IsLooped { get; private set; }

    public bool IsCancelled { get; protected set; }

    public bool IsDone => IsCompleted || IsCancelled;
    public bool IsRunning => !IsDone && !IsPaused && IsRegistered;
    public float Progress => GetElapsedTime() / duration;
    public float TimeRemaining => duration - GetElapsedTime();
    public float Remaining => duration / GetElapsedTime();


    protected Timer(float duration)
    {
        this.duration = duration;
    }

    public static Timer Register(Timer timer)
    {
        EnsureManagerExits();

        timer.OnStart(() => timer.IsRegistered = true);
        timer.OnDone(() =>
        {
            timer.IsRegistered = false;
            manager.RemoveTimer(timer);
        });
        return timer;
    }

    public static Timer Register(float duration)
    {
        var timer = new Timer(duration);
        return Register(timer);
    }

    private static void EnsureManagerExits()
    {
        if (manager == null)
            manager = TimerManager.Instance ?? new GameObject("TimerManager").AddComponent<TimerManager>();
    }

    public Timer WithCancellation(CancellationToken token)
    {
        tokenRegistration.Dispose();

        cancellationToken = token;

        if (token.CanBeCanceled)
        {
            tokenRegistration = token.Register(() => Cancel(), useSynchronizationContext: false);
        }

        return this;
    }

    #region API

    public Timer OnStart(Action onStart)
    {
        this.onStart += onStart;
        return this;
    }

    /// <summary>
    /// The time elapsed value will be between 0 and duration.
    /// </summary>
    /// <param name="onUpdate"></param>
    /// <returns></returns>
    public Timer OnUpdate(Action<float> onUpdate)
    {
        this.onUpdate += onUpdate;
        return this;
    }

    /// <summary>
    /// The progress value will be between 0 and 1.
    /// </summary>
    /// <param name="onProgress"></param>
    /// <returns></returns>
    public Timer OnProgress(Action<float> onProgress)
    {
        this.onProgress += onProgress;
        return this;
    }

    /// <summary>
    /// The time remaining value will be between 0 and duration.
    /// </summary>
    /// <param name="onRemaining"></param>
    /// <returns></returns>
    public Timer OnTimeRemaining(Action<float> onRemaining)
    {
        this.onTimeRemaining += onRemaining;
        return this;
    }

    /// <summary>
    /// The remaining value will be between 0 and 1.
    /// </summary>
    /// <param name="onRemaining"></param>
    /// <returns></returns>
    public Timer OnRemaining(Action<float> onRemaining)
    {
        this.onRemaining += onRemaining;
        return this;
    }

    public Timer OnComplete(Action onComplete)
    {
        this.onComplete += onComplete;
        return this;
    }

    public Timer OnDone(Action onDone)
    {
        this.onDone += onDone;
        return this;
    }

    public Timer OnCancel(Action onCancel)
    {
        this.onCancel += onCancel;
        return this;
    }

    #endregion

    public Timer Loop(bool isLooped = true)
    {
        IsLooped = isLooped;
        return this;
    }

    /// <summary>
    /// Your timer will start counting down from the moment you call this method. This method just runs only once no matter how many times you call it.
    /// </summary>
    /// <returns></returns>
    public virtual Timer Start()
    {
        if (IsRegistered || IsDone) return this;
        startTime = GetWorldTime();
        manager.RegisterTimer(this);
        onStart?.Invoke();
        // RegisterActions();
        return this;
    }

    /// <summary>
    /// Your timer will ready to be completed. Usefully when you want to skip the timer.
    /// </summary>
    /// <returns></returns>
    public Timer AlreadyDone()
    {
        IsCompleted = true;
        return this;
    }

    /// <summary>
    /// Your timer will start counting down from the moment you call this method. This method can be called multiple times and it will reset the timer.
    /// </summary>
    /// <returns></returns>
    public Timer ReStart()
    {
        return Reset().Start();
    }

    private Timer Reset()
    {
        if (IsRegistered) manager.RemoveTimer(this);
        IsRegistered = false;
        IsCompleted = false;
        IsCancelled = false;
        timeElapsedBeforePause = null;
        cleanupCalled = false;

        tokenRegistration.Dispose();
        cancellationToken = null;
        tokenRegistration = default;

        return this;
    }

    public void Cancel()
    {
        if (IsCancelled) return;

        IsCancelled = true;
        onCancel?.Invoke();
    }

    public void Pause()
    {
        if (IsPaused || IsDone) return;
        timeElapsedBeforePause = GetElapsedTime();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        startTime = GetWorldTime() - timeElapsedBeforePause.Value;
        timeElapsedBeforePause = null;
    }

    private float GetElapsedTime()
    {
        return IsCompleted ? duration : timeElapsedBeforePause ?? GetWorldTime() - startTime;
    }

    protected virtual float GetWorldTime() => Time.time;

    public void Update()
    {
        if (cancellationToken is { IsCancellationRequested: true } &&
            !IsCancelled)
        {
            Cancel();
            return;
        }

        if (IsPaused) return;

        if (IsDone)
        {
            Cleanup();
            return;
        }

        onUpdate?.Invoke(GetElapsedTime());
        onProgress?.Invoke(Progress);
        onTimeRemaining?.Invoke(TimeRemaining);
        onRemaining?.Invoke(Remaining);

        if (GetWorldTime() < startTime + duration) return;

        onComplete?.Invoke();

        if (IsLooped)
            startTime = GetWorldTime();
        else
        {
            IsCompleted = true;
            Cleanup();
        }
    }

    protected bool cleanupCalled = false;

    private void Cleanup()
    {
        if (cleanupCalled) return;
        cleanupCalled = true;

        onDone?.Invoke();

        if (IsRegistered)
        {
            manager?.RemoveTimer(this);
            IsRegistered = false;
        }
    }

    public void Dispose()
    {
        Cancel();

        // Dispose token registration
        tokenRegistration.Dispose();
        cancellationToken = null;

        onStart = null;
        onUpdate = null;
        onProgress = null;
        onTimeRemaining = null;
        onComplete = null;
        onCancel = null;
        onDone = null;
    }
}

public class TimerManager : PersistentSingleton<TimerManager>
{
    private readonly List<Timer> timers = new();

    private void Update()
    {
        RefreshTimers();
    }

    public void RegisterTimer(Timer timer)
    {
        if(!timers.Contains(timer)) timers.Add(timer);
    }

    public void RemoveTimer(Timer timer)
    {
        timers.Remove(timer);
    }

    private void RefreshTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Update();
        }
    }

    private void PauseTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Pause();
        }
    }

    private void ResumeTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Resume();
        }
    }

    private void CancelTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Cancel();
        }

        timers.Clear();
    }
}