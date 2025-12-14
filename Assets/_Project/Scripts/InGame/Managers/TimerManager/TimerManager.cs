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


public class Timer : IDisposable
{
    protected static TimerManager manager;

    private CancellationToken? cancellationToken;
    private CancellationTokenRegistration tokenRegistration;

    private readonly float duration;
    protected float startTime;
    protected float? timeElapsedBeforePause;
    
    private Action<float> onUpdate;
    private Action<float> onProgress;
    private Action<float> onRemaining;
    private Action<float> onTimeRemaining;

    protected Action onStart;
    private Action onComplete;
    private Action onCancel;
    private Action onDone;

    public float Duration => duration;
    public bool IsRegistered { get; private set; }
    public bool IsCompleted { get; protected set; }
    public bool IsPaused => timeElapsedBeforePause.HasValue;
    public bool IsLooped { get; private set; }

    public bool IsCancelled { get; protected set; }

    public bool IsDone => IsCompleted || IsCancelled;
    public bool IsRunning => !IsDone && !IsPaused && IsRegistered;
    public float Progress => duration > 0 ? Mathf.Clamp01(GetElapsedTime() / duration) : 0f;
    public float TimeRemaining => Mathf.Max(0f, duration - GetElapsedTime());
    public float Remaining => duration > 0 ? Mathf.Clamp01(TimeRemaining / duration) : 0f;


    protected Timer(float duration)
    {
        this.duration = Math.Max(0f, duration);
    }

    public static Timer Register(Timer timer)
    {
        EnsureManagerExists();

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

    private static void EnsureManagerExists()
    {
        if (manager == null)
            manager = TimerManager.Instance ?? new GameObject("TimerManager").AddComponent<TimerManager>();
    }

    public Timer WithCancellation(CancellationToken token)
    {
        if (tokenRegistration != default)
        {
            tokenRegistration.Dispose();
        }

        cancellationToken = token;

        if (token.CanBeCanceled)
        {
            tokenRegistration = token.Register(Cancel, useSynchronizationContext: false);
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

    public virtual Timer Start()
    {
        if (IsDone)
        {
            ResetState();
        }
        
        startTime = GetWorldTime();
        if (IsRegistered) return this;
        
        EnsureManagerExists();
        manager.RegisterTimer(this);
        onStart?.Invoke();
        return this;
    }
    
    /// <summary>
    /// Restart the timer from the beginning. This will reset all states and start again.
    /// </summary>
    /// <returns></returns>
    public Timer Restart()
    {
        if (IsRegistered)
        {
            manager?.RemoveTimer(this);
            IsRegistered = false;
        }
        
        ResetState();
        return Start();
    }
    
    private void ResetState()
    {
        IsCompleted = false;
        IsCancelled = false;
        timeElapsedBeforePause = null;
        cleanupCalled = false;
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


    public void Cancel()
    {
        if (IsCancelled) return;

        IsCancelled = true;
        onCancel?.Invoke();
        
        Cleanup();
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
        UpdateTimers();
    }

    public void RegisterTimer(Timer timer)
    {
        if(!timers.Contains(timer)) timers.Add(timer);
    }
       
    public void RemoveTimer(Timer timer)
    {
        timers.Remove(timer);
    }

    private void UpdateTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Update();
        }
    }

    public void PauseTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Pause();
        }
    }

    public void ResumeTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Resume();
        }
    }

    public void CancelTimers()
    {
        for (int i = timers.Count - 1; i >= 0; i--)
        {
            timers[i].Cancel();
        }

        timers.Clear();
    }
    
    public int ActiveTimersCount => timers.Count;
}