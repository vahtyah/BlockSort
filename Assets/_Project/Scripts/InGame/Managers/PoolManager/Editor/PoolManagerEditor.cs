using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(PoolManager))]
public class PoolManagerEditor : Editor
{
    private Vector2 _scrollPosition;
    private Dictionary<GameObject, bool> _poolFoldouts = new Dictionary<GameObject, bool>();
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        PoolManager manager = (PoolManager)target;
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Pool Manager is only available during Play Mode.", MessageType.Info);
            return;
        }
        
        // Get private fields using reflection
        var prefabToPoolField = typeof(PoolManager).GetField("prefabToPool", BindingFlags.NonPublic | BindingFlags.Instance);
        var instanceToPoolField = typeof(PoolManager).GetField("instanceToPool", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (prefabToPoolField == null || instanceToPoolField == null)
        {
            EditorGUILayout.HelpBox("Unable to access pool data via reflection. Field names may have changed.", MessageType.Warning);
            return;
        }
        
        var prefabToPoolValue = prefabToPoolField.GetValue(manager);
        var instanceToPoolValue = instanceToPoolField.GetValue(manager);
        
        if (prefabToPoolValue == null || instanceToPoolValue == null)
        {
            EditorGUILayout.HelpBox("Pool dictionaries are null. PoolManager may not be initialized properly.", MessageType.Warning);
            return;
        }
        
        // Cast to proper dictionary type
        var prefabToPool = prefabToPoolValue as System.Collections.IDictionary;
        var instanceToPool = instanceToPoolValue as System.Collections.IDictionary;
        
            if (prefabToPool != null && instanceToPool != null)
            {
                int totalPools = prefabToPool.Count;
                int totalActiveInstances = instanceToPool.Count;
                int totalAvailableObjects = 0;
                
                // Calculate total available objects across all pools
                foreach (System.Collections.DictionaryEntry entry in prefabToPool)
                {
                    var pool = entry.Value;
                    var availableObjectsField = pool.GetType().GetField("availableObjects", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (availableObjectsField != null)
                    {
                        var availableObjects = availableObjectsField.GetValue(pool) as System.Collections.ICollection;
                        if (availableObjects != null)
                        {
                            totalAvailableObjects += availableObjects.Count;
                        }
                    }
                }
                
                // Statistics Box
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                DrawStatBox("Pools", totalPools.ToString(), new Color(0.3f, 0.6f, 1f));
                DrawStatBox("Active", totalActiveInstances.ToString(), new Color(1f, 0.8f, 0.3f));
                DrawStatBox("Available", totalAvailableObjects.ToString(), new Color(0.5f, 1f, 0.5f));
                DrawStatBox("Total", (totalActiveInstances + totalAvailableObjects).ToString(), new Color(0.8f, 0.8f, 0.8f));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Clear All Pools Button
                if (totalPools > 0)
                {
                    GUI.color = new Color(1f, 0.5f, 0.5f);
                    if (GUILayout.Button("Clear All Pools", GUILayout.Height(30)))
                    {
                        if (EditorUtility.DisplayDialog("Clear All Pools",
                            "Are you sure you want to clear all pools? This will destroy all pooled objects.",
                            "Yes", "Cancel"))
                        {
                            manager.ClearAllPools();
                            EditorUtility.SetDirty(manager);
                        }
                    }
                    GUI.color = Color.white;
                    
                    EditorGUILayout.Space(10);
                    
                    // Pool Details Section
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Pool Details ({totalPools})", EditorStyles.boldLabel);
                    
                    EditorGUILayout.Space(5);
                    
                    // Calculate dynamic scroll height based on expanded/collapsed state
                    float totalContentHeight = 0f;
                    float collapsedItemHeight = 35f; // Collapsed pool item + spacing
                    float expandedItemHeight = 55f; // Expanded pool item with details + spacing
                    
                    foreach (System.Collections.DictionaryEntry entry in prefabToPool)
                    {
                        var prefab = entry.Key as GameObject;
                        if (prefab != null && _poolFoldouts.ContainsKey(prefab) && _poolFoldouts[prefab])
                        {
                            totalContentHeight += expandedItemHeight;
                        }
                        else
                        {
                            totalContentHeight += collapsedItemHeight;
                        }
                    }
                    
                    float dynamicHeight = Mathf.Min(totalContentHeight, 500f);
                    
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(dynamicHeight));
                    
                    foreach (System.Collections.DictionaryEntry entry in prefabToPool)
                    {
                        DrawPoolInfo(entry.Key as GameObject, entry.Value, instanceToPool);
                        EditorGUILayout.Space(3);
                    }
                    
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.Space(10);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Pool Details (0)", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.HelpBox("No pools registered yet. Pools will be created automatically when objects are spawned.", MessageType.Info);
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Unable to cast pool dictionaries. This may be a type mismatch issue.", MessageType.Warning);
            }
        
        // Auto-repaint when playing to update pool values in real-time
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
    
    private void DrawStatBox(string label, string value, Color color)
    {
        GUI.color = color;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.color = Color.white;
        
        GUILayout.Label(label, EditorStyles.miniBoldLabel);
        GUILayout.Label(value, EditorStyles.largeLabel);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPoolInfo(GameObject prefab, object pool, System.Collections.IDictionary instanceToPool)
    {
        if (prefab == null) return;
        
        // Get pool data using reflection
        var availableObjectsField = pool.GetType().GetField("availableObjects", BindingFlags.NonPublic | BindingFlags.Instance);
        var parentTransformField = pool.GetType().GetField("parentTransform", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (availableObjectsField == null || parentTransformField == null) return;
        
        var availableObjects = availableObjectsField.GetValue(pool) as Queue<GameObject>;
        var parentTransform = parentTransformField.GetValue(pool) as Transform;
        
        int availableCount = availableObjects?.Count ?? 0;
        
        // Count active instances for this pool
        int activeCount = 0;
        if (instanceToPool != null)
        {
            foreach (System.Collections.DictionaryEntry entry in instanceToPool)
            {
                if (entry.Value == pool)
                {
                    activeCount++;
                }
            }
        }
        
        // Initialize foldout state
        if (!_poolFoldouts.ContainsKey(prefab))
        {
            _poolFoldouts[prefab] = false;
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with fixed height for proper alignment
        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect headerRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(lineHeight + 4));
        
        // Make entire header clickable for foldout
        if (Event.current.type == EventType.MouseDown && headerRect.Contains(Event.current.mousePosition))
        {
            _poolFoldouts[prefab] = !_poolFoldouts[prefab];
            Event.current.Use();
        }
        
        // Foldout arrow
        _poolFoldouts[prefab] = EditorGUILayout.Toggle(_poolFoldouts[prefab], EditorStyles.foldout, GUILayout.Width(15), GUILayout.Height(lineHeight));
        
        EditorGUILayout.ObjectField(prefab, typeof(GameObject), false, GUILayout.Width(250), GUILayout.Height(lineHeight + 3));
        GUILayout.FlexibleSpace();
        
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.alignment = TextAnchor.MiddleRight;
        labelStyle.fixedHeight = lineHeight;
        
        GUI.color = new Color(0.5f, 1f, 0.5f);
        GUILayout.Label($"Available: {availableCount}", labelStyle, GUILayout.Width(80));
        GUI.color = new Color(1f, 1f, 0.5f);
        GUILayout.Label($"Active: {activeCount}", labelStyle, GUILayout.Width(70));
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.Label($"Total: {availableCount + activeCount}", labelStyle, GUILayout.Width(60));
        GUI.color = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Expanded view
        if (_poolFoldouts[prefab])
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parent:", GUILayout.Width(80));
            EditorGUILayout.ObjectField(parentTransform, typeof(Transform), true);
            EditorGUILayout.EndHorizontal();
            
            // Progress bar visualization
            // EditorGUILayout.Space(5);
            // EditorGUILayout.LabelField("Pool Usage:");
            // DrawPoolUsageBar(activeCount, availableCount);
            //
            // EditorGUILayout.Space(5);
            
            // Clear pool button
            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (availableCount > 0 && GUILayout.Button("Clear Pool", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear Pool",
                    $"Clear pool for '{prefab.name}'?",
                    "Yes", "Cancel"))
                {
                    var clearMethod = pool.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                    if (clearMethod != null)
                    {
                        clearMethod.Invoke(pool, null);
                        Repaint();
                        Debug.Log($"[Pool Manager] Cleared pool for '{prefab.name}'");
                    }
                    else
                    {
                        Debug.LogError($"[Pool Manager] Clear method not found for pool '{prefab.name}'");
                    }
                }
            }
            GUI.color = Color.white;
            
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPoolUsageBar(int active, int available)
    {
        Rect rect = GUILayoutUtility.GetRect(18, 18, "TextField");
        
        int total = active + available;
        if (total == 0) return;
        
        float activeRatio = (float)active / total;
        float availableRatio = (float)available / total;
        
        // Draw background
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        // Draw active portion
        Rect activeRect = new Rect(rect.x, rect.y, rect.width * activeRatio, rect.height);
        EditorGUI.DrawRect(activeRect, new Color(1f, 0.8f, 0.3f));
        
        // Draw available portion
        Rect availableRect = new Rect(rect.x + activeRect.width, rect.y, rect.width * availableRatio, rect.height);
        EditorGUI.DrawRect(availableRect, new Color(0.5f, 1f, 0.5f));
        
        // Draw label
        GUI.Label(rect, $"  Active: {active} | Available: {available}", EditorStyles.whiteLabel);
    }
}

