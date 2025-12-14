using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(TimerManager))]
public class TimerManagerEditor : Editor
{
    private bool showTimers = true;
    private Vector2 scrollPosition;
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        TimerManager manager = (TimerManager)target;
        
        // Get the private timers list using reflection
        FieldInfo timersField = typeof(TimerManager).GetField("timers", BindingFlags.NonPublic | BindingFlags.Instance);
        if (timersField != null)
        {
            List<Timer> timers = timersField.GetValue(manager) as List<Timer>;
            
            if (timers != null)
            {
                EditorGUILayout.LabelField($"Active Timers: {timers.Count}", EditorStyles.helpBox);
                
                if (timers.Count > 0)
                {
                    showTimers = EditorGUILayout.Foldout(showTimers, "Timer Details", true);
                    
                    if (showTimers)
                    {
                        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(400));
                        
                        for (int i = 0; i < timers.Count; i++)
                        {
                            DrawTimerInfo(timers[i], i);
                        }
                        
                        EditorGUILayout.EndScrollView();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No active timers", MessageType.Info);
                }
            }
        }
        
        // Auto-repaint when playing to update timer values in real-time
        if (Application.isPlaying)
        {
            EditorUtility.SetDirty(target);
            Repaint();
        }
    }
    
    private void DrawTimerInfo(Timer timer, int index)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.LabelField($"Timer #{index}", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++;
        
        // Duration
        EditorGUILayout.LabelField("Duration:", timer.Duration.ToString("F2") + "s");
        
        // Progress
        float progress = timer.Progress;
        EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progress, $"Progress: {(progress * 100):F1}%");
        
        // Time Remaining
        EditorGUILayout.LabelField("Time Remaining:", timer.TimeRemaining.ToString("F2") + "s");
        
        // Status
        string status = "";
        Color statusColor = Color.white;
        
        if (timer.IsRunning)
        {
            status = "Running";
            statusColor = Color.green;
        }
        else if (timer.IsPaused)
        {
            status = "Paused";
            statusColor = Color.yellow;
        }
        else if (timer.IsCompleted)
        {
            status = "Completed";
            statusColor = Color.blue;
        }
        else if (timer.IsCancelled)
        {
            status = "Cancelled";
            statusColor = Color.red;
        }
        else if (timer.IsDone)
        {
            status = "Done";
            statusColor = Color.gray;
        }
        
        GUI.color = statusColor;
        EditorGUILayout.LabelField("Status:", status);
        GUI.color = Color.white;
        
        // Additional flags
        EditorGUILayout.BeginHorizontal();
        if (timer.IsLooped) EditorGUILayout.LabelField("🔄 Looped", GUILayout.Width(80));
        if (timer.IsRegistered) EditorGUILayout.LabelField("📝 Registered", GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.indentLevel--;
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
}

