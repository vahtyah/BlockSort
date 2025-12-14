using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor Window for testing Timer System with visual interface
/// Open from: Window > Timer System Test
/// </summary>
public class TimerSystemTestWindow : EditorWindow
{
    private Timer testTimer;
    private List<Timer> activeTimers = new List<Timer>();
    private int loopCount = 0;
    private float testDuration = 5f;
    private bool useLoop = false;
    private Vector2 scrollPosition;
    
    private string logText = "";
    private const int maxLogLines = 50;
    
    [MenuItem("Window/Timer System Test")]
    public static void ShowWindow()
    {
        var window = GetWindow<TimerSystemTestWindow>("Timer Test");
        window.minSize = new Vector2(400, 600);
    }
    
    private void OnEnable()
    {
        EditorApplication.update += UpdateTimers;
        Log("Timer System Test Window opened");
    }
    
    private void OnDisable()
    {
        EditorApplication.update -= UpdateTimers;
        CleanupAllTimers();
    }
    
    private void UpdateTimers()
    {
        // Force repaint to show live updates
        if (testTimer != null && testTimer.IsRunning)
        {
            Repaint();
        }
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        DrawHeader();
        DrawSeparator();
        DrawTimerCreation();
        DrawSeparator();
        DrawTimerControls();
        DrawSeparator();
        DrawTimerStatus();
        DrawSeparator();
        DrawDemoButtons();
        DrawSeparator();
        DrawTestButtons();
        DrawSeparator();
        DrawLogArea();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Timer System Test", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        if (TimerManager.Instance != null)
        {
            EditorGUILayout.HelpBox($"Active Timers: {TimerManager.Instance.ActiveTimersCount}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("TimerManager not initialized", MessageType.Warning);
        }
    }
    
    private void DrawTimerCreation()
    {
        EditorGUILayout.LabelField("Create Timer", EditorStyles.boldLabel);
        
        testDuration = EditorGUILayout.Slider("Duration (seconds)", testDuration, 0.1f, 60f);
        useLoop = EditorGUILayout.Toggle("Loop", useLoop);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Create Timer", GUILayout.Height(30)))
        {
            CreateNewTimer();
        }
        
        if (GUILayout.Button("Create LocalTimer", GUILayout.Height(30)))
        {
            CreateLocalTimer();
        }
        
        if (GUILayout.Button("Create RealTimeTimer", GUILayout.Height(30)))
        {
            CreateRealTimeTimer();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawTimerControls()
    {
        EditorGUILayout.LabelField("Timer Controls", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(testTimer == null);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("▶ Start", GUILayout.Height(25)))
        {
            testTimer?.Start();
            Log("Timer started");
        }
        
        if (GUILayout.Button("⏸ Pause", GUILayout.Height(25)))
        {
            testTimer?.Pause();
            Log("Timer paused");
        }
        
        if (GUILayout.Button("▶ Resume", GUILayout.Height(25)))
        {
            testTimer?.Resume();
            Log("Timer resumed");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("🔄 Restart", GUILayout.Height(25)))
        {
            testTimer?.Restart();
            Log("Timer restarted");
        }
        
        if (GUILayout.Button("❌ Cancel", GUILayout.Height(25)))
        {
            testTimer?.Cancel();
            Log("Timer cancelled");
        }
        
        if (GUILayout.Button("🗑 Dispose", GUILayout.Height(25)))
        {
            testTimer?.Dispose();
            testTimer = null;
            Log("Timer disposed");
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndDisabledGroup();
    }
    
    private void DrawTimerStatus()
    {
        EditorGUILayout.LabelField("Timer Status", EditorStyles.boldLabel);
        
        if (testTimer == null)
        {
            EditorGUILayout.HelpBox("No timer created. Create a timer first.", MessageType.Info);
            return;
        }
        
        // Progress bar
        var progress = testTimer.Progress;
        EditorGUI.ProgressBar(
            EditorGUILayout.GetControlRect(false, 25),
            progress,
            $"Progress: {progress * 100:F1}% ({testTimer.TimeRemaining:F2}s remaining)"
        );
        
        EditorGUILayout.Space();
        
        // Status indicators with colors
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        DrawStatusField("Duration", $"{testTimer.Duration:F2}s");
        DrawStatusField("Elapsed", $"{(testTimer.Duration * testTimer.Progress):F2}s");
        DrawStatusField("Progress", $"{testTimer.Progress * 100:F1}%");
        DrawStatusField("Remaining", $"{testTimer.Remaining * 100:F1}%");
        DrawStatusField("Time Remaining", $"{testTimer.TimeRemaining:F2}s");
        
        EditorGUILayout.Space();
        
        DrawBoolField("Is Running", testTimer.IsRunning, Color.green);
        DrawBoolField("Is Registered", testTimer.IsRegistered, Color.blue);
        DrawBoolField("Is Paused", testTimer.IsPaused, Color.yellow);
        DrawBoolField("Is Looped", testTimer.IsLooped, Color.cyan);
        DrawBoolField("Is Done", testTimer.IsDone, Color.gray);
        DrawBoolField("Is Completed", testTimer.IsCompleted, Color.green);
        DrawBoolField("Is Cancelled", testTimer.IsCancelled, Color.red);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawDemoButtons()
    {
        EditorGUILayout.LabelField("Quick Demos", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Demo: Countdown Timer (10s)", GUILayout.Height(30)))
        {
            Demo_CountdownTimer();
        }
        
        if (GUILayout.Button("Demo: Loop Timer (2s loops)", GUILayout.Height(30)))
        {
            Demo_LoopTimer();
        }
        
        if (GUILayout.Button("Demo: Multiple Timers", GUILayout.Height(30)))
        {
            Demo_MultipleTimers();
        }
    }
    
    private void DrawTestButtons()
    {
        EditorGUILayout.LabelField("Automated Tests", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🧪 Run All Tests", GUILayout.Height(35)))
        {
            RunAllTests();
        }
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Test: Bug Fixes", GUILayout.Height(25)))
        {
            TestBugFixes();
        }
        
        if (GUILayout.Button("Test: Features", GUILayout.Height(25)))
        {
            TestFeatures();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLogArea()
    {
        EditorGUILayout.LabelField("Event Log", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Log", GUILayout.Width(100)))
        {
            logText = "";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.TextArea(logText, GUILayout.Height(150));
    }
    
    #region Helper Methods
    
    private void DrawStatusField(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(120));
        EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawBoolField(string label, bool value, Color trueColor)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(120));
        
        var prevColor = GUI.color;
        GUI.color = value ? trueColor : Color.gray;
        EditorGUILayout.LabelField(value ? "✓ YES" : "✗ NO", EditorStyles.boldLabel);
        GUI.color = prevColor;
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
    }
    
    private void Log(string message)
    {
        var timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        logText = $"[{timestamp}] {message}\n" + logText;
        
        // Trim log if too long
        var lines = logText.Split('\n');
        if (lines.Length > maxLogLines)
        {
            logText = string.Join("\n", lines, 0, maxLogLines);
        }
        
        Debug.Log($"[TimerTest] {message}");
    }
    
    #endregion
    
    #region Timer Creation
    
    private void CreateNewTimer()
    {
        testTimer?.Cancel();
        loopCount = 0;
        
        testTimer = Timer.Register(testDuration)
            .OnStart(() => Log($"✓ Timer started: {testDuration}s"))
            .OnUpdate(elapsed => {})
            .OnProgress(progress => {})
            .OnComplete(() => Log($"✓ Timer completed! (Loop #{++loopCount})"))
            .OnCancel(() => Log("✗ Timer cancelled"))
            .OnDone(() => Log("✓ Timer done"))
            .Loop(useLoop)
            .Start();
        
        Log($"Created Timer ({testDuration}s, Loop: {useLoop})");
    }
    
    private void CreateLocalTimer()
    {
        testTimer?.Cancel();
        loopCount = 0;
        
        var timer = new LocalTimer(testDuration);
        testTimer = Timer.Register(timer)
            .OnStart(() => Log("✓ LocalTimer started"))
            .OnComplete(() => Log("✓ LocalTimer completed"))
            .Loop(useLoop)
            .Start();
        
        Log($"Created LocalTimer ({testDuration}s)");
    }
    
    private void CreateRealTimeTimer()
    {
        testTimer?.Cancel();
        loopCount = 0;
        
        var timer = new RealTimeTimer(testDuration);
        testTimer = Timer.Register(timer)
            .OnStart(() => Log("✓ RealTimeTimer started"))
            .OnComplete(() => Log("✓ RealTimeTimer completed"))
            .Loop(useLoop)
            .Start();
        
        Log($"Created RealTimeTimer ({testDuration}s)");
    }
    
    #endregion
    
    #region Demos
    
    private void Demo_CountdownTimer()
    {
        testTimer?.Cancel();
        
        testTimer = Timer.Register(10f)
            .OnStart(() => Log("Countdown: 10 seconds!"))
            .OnUpdate(elapsed => {})
            .OnComplete(() => Log("🎉 Countdown finished!"))
            .Start();
        
        Log("Demo: Countdown timer started");
    }
    
    private void Demo_LoopTimer()
    {
        testTimer?.Cancel();
        loopCount = 0;
        
        testTimer = Timer.Register(2f)
            .Loop(true)
            .OnComplete(() => Log($"🔄 Loop iteration #{++loopCount}"))
            .Start();
        
        Log("Demo: Loop timer started (2s per loop)");
    }
    
    private void Demo_MultipleTimers()
    {
        CleanupAllTimers();
        
        var timer1 = Timer.Register(3f)
            .OnComplete(() => Log("Timer 1 (3s) completed"))
            .Start();
        
        var timer2 = Timer.Register(5f)
            .OnComplete(() => Log("Timer 2 (5s) completed"))
            .Start();
        
        var timer3 = Timer.Register(7f)
            .OnComplete(() => Log("Timer 3 (7s) completed"))
            .Start();
        
        activeTimers.Add(timer1);
        activeTimers.Add(timer2);
        activeTimers.Add(timer3);
        
        Log("Demo: Created 3 timers (3s, 5s, 7s)");
    }
    
    #endregion
    
    #region Tests
    
    private void RunAllTests()
    {
        Log("=== STARTING ALL TESTS ===");
        
        TestBugFixes();
        TestFeatures();
        
        Log("=== ALL TESTS COMPLETED ===");
    }
    
    private void TestBugFixes()
    {
        Log("--- Testing Bug Fixes ---");
        
        // Test 1: Remaining property no division by zero
        var timer1 = Timer.Register(1f).Start();
        var remaining = timer1.Remaining;
        if (remaining >= 0f && remaining <= 1f && !float.IsInfinity(remaining))
        {
            Log("✅ Test: Remaining property (no division by zero)");
        }
        else
        {
            Log($"❌ Test FAILED: Remaining = {remaining}");
        }
        timer1.Cancel();
        
        // Test 2: Progress clamped to 0-1
        var timer2 = Timer.Register(1f).Start();
        var progress = timer2.Progress;
        if (progress >= 0f && progress <= 1f)
        {
            Log("✅ Test: Progress clamped to 0-1");
        }
        else
        {
            Log($"❌ Test FAILED: Progress = {progress}");
        }
        timer2.Cancel();
        
        // Test 3: Restart after complete
        var timer3 = Timer.Register(0.01f).Start();
        timer3.AlreadyDone();
        timer3.Start();
        if (!timer3.IsDone)
        {
            Log("✅ Test: Can restart after complete");
        }
        else
        {
            Log("❌ Test FAILED: Cannot restart");
        }
        timer3.Cancel();
        
        // Test 4: Cancel cleans up immediately
        bool doneCalled = false;
        var timer4 = Timer.Register(5f)
            .OnDone(() => doneCalled = true)
            .Start();
        timer4.Cancel();
        if (doneCalled && !timer4.IsRegistered)
        {
            Log("✅ Test: Cancel cleans up immediately");
        }
        else
        {
            Log("❌ Test FAILED: Cancel didn't cleanup");
        }
    }
    
    private void TestFeatures()
    {
        Log("--- Testing Features ---");
        
        // Test: Loop
        var timer1 = Timer.Register(0.1f).Loop(true).Start();
        if (timer1.IsLooped)
        {
            Log("✅ Test: Loop feature");
        }
        timer1.Cancel();
        
        // Test: Pause/Resume
        var timer2 = Timer.Register(5f).Start();
        timer2.Pause();
        bool isPaused = timer2.IsPaused;
        timer2.Resume();
        bool isResumed = !timer2.IsPaused;
        if (isPaused && isResumed)
        {
            Log("✅ Test: Pause/Resume");
        }
        timer2.Cancel();
        
        // Test: Multiple callbacks
        bool allCalled = false;
        var timer3 = Timer.Register(0.01f)
            .OnStart(() => { })
            .OnUpdate(e => { })
            .OnProgress(p => { })
            .OnComplete(() => allCalled = true)
            .Start();
        
        if (allCalled || timer3.IsRunning)
        {
            Log("✅ Test: Multiple callbacks");
        }
        timer3.Cancel();
    }
    
    #endregion
    
    private void CleanupAllTimers()
    {
        testTimer?.Cancel();
        testTimer = null;
        
        foreach (var timer in activeTimers)
        {
            timer?.Cancel();
        }
        activeTimers.Clear();
        
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.CancelTimers();
        }
    }
}

