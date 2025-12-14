using System;
using System.Threading;
using UnityEngine;

// ============================================================================
// CANCELLATION TOKEN EXTENSIONS - UniTask Style
// ============================================================================

public static class CancellationTokenExtensions
{
    /// <summary>
    /// Get a CancellationToken that will be cancelled when the GameObject is destroyed. 
    /// Similar to UniTask's GetCancellationTokenOnDestroy()
    /// </summary>
    public static CancellationToken GetCancellationTokenOnDestroy(this MonoBehaviour mono)
    {
        if (mono == null || mono.gameObject == null)
            return new CancellationToken(true);

        return mono.gameObject.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// Get a CancellationToken that will be cancelled when the GameObject is destroyed.
    /// </summary>
    public static CancellationToken GetCancellationTokenOnDestroy(this GameObject gameObject)
    {
        if (gameObject == null)
            return new CancellationToken(true);

        return GetOrAddComponent<DestroyDetector>(gameObject).CancellationToken;
    }

    /// <summary>
    /// Get a CancellationToken that will be cancelled when the GameObject is destroyed.
    /// </summary>
    public static CancellationToken GetCancellationTokenOnDestroy(this Component component)
    {
        if (component == null || component.gameObject == null)
            return new CancellationToken(true);

        return component.gameObject.GetCancellationTokenOnDestroy();
    }

    /// <summary>
    /// Get a CancellationToken that will be cancelled when the GameObject is disabled.
    /// </summary>
    public static CancellationToken GetCancellationTokenOnDisable(this MonoBehaviour mono)
    {
        if (mono == null || mono.gameObject == null)
            return new CancellationToken(true);

        return mono.gameObject.GetCancellationTokenOnDisable();
    }

    /// <summary>
    /// Get a CancellationToken that will be cancelled when the GameObject is disabled.
    /// </summary>
    public static CancellationToken GetCancellationTokenOnDisable(this GameObject gameObject)
    {
        if (gameObject == null)
            return new CancellationToken(true);

        return GetOrAddComponent<DisableDetector>(gameObject).CancellationToken;
    }

    /// <summary>
    /// Get a CancellationToken that will be cancelled when the GameObject is disabled. 
    /// </summary>
    public static CancellationToken GetCancellationTokenOnDisable(this Component component)
    {
        if (component == null || component.gameObject == null)
            return new CancellationToken(true);

        return component.gameObject.GetCancellationTokenOnDisable();
    }

    /// <summary>
    /// Create a linked token that cancels when ANY of the source tokens cancel.
    /// Remember to dispose the returned CancellationTokenSource to avoid memory leaks. 
    /// </summary>
    public static CancellationToken LinkWith(this CancellationToken token, params CancellationToken[] otherTokens)
    {
        if (otherTokens == null || otherTokens.Length == 0)
            return token;

        var allTokens = new CancellationToken[otherTokens.Length + 1];
        allTokens[0] = token;
        Array.Copy(otherTokens, 0, allTokens, 1, otherTokens.Length);

        // Note: This creates a new CancellationTokenSource that should be disposed
        // Consider using CreateLinkedTokenSourceWithAutoDispose for automatic cleanup
        return CancellationTokenSource.CreateLinkedTokenSource(allTokens).Token;
    }

    /// <summary>
    /// Optimized GetOrAddComponent - Similar to UniTask implementation
    /// </summary>
    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
#if UNITY_2019_2_OR_NEWER
        if (!gameObject.TryGetComponent<T>(out var component))
        {
            component = gameObject.AddComponent<T>();
        }
#else
        var component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
#endif
        return component;
    }
}

// ============================================================================
// DESTROY DETECTOR COMPONENT
// ============================================================================

/// <summary>
/// Internal component to detect GameObject destruction and provide CancellationToken. 
/// Based on UniTask's AsyncDestroyTrigger implementation. 
/// </summary>
[DisallowMultipleComponent]
public sealed class DestroyDetector : MonoBehaviour
{
    private bool isDestroyCalled = false;
    private CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Gets the CancellationToken that will be cancelled when this GameObject is destroyed.
    /// Lazy initialization for better performance.
    /// </summary>
    public CancellationToken CancellationToken
    {
        get
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            return cancellationTokenSource.Token;
        }
    }

    /// <summary>
    /// Check if the token has been cancelled
    /// </summary>
    public bool IsCancelled => isDestroyCalled;

    private void OnDestroy()
    {
        if (isDestroyCalled) return;

        isDestroyCalled = true;

        // Cancel and dispose the token source
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }

#if UNITY_EDITOR
    // Debug info in Inspector
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            gameObject.name = $"{gameObject.name} [CancellationToken]";
        }
    }
#endif
}

// ============================================================================
// DISABLE DETECTOR COMPONENT
// ============================================================================

/// <summary>
/// Internal component to detect GameObject disable and provide CancellationToken. 
/// </summary>
[DisallowMultipleComponent]
public sealed class DisableDetector : MonoBehaviour
{
    private bool isDisableCalled = false;
    private CancellationTokenSource cancellationTokenSource;

    /// <summary>
    /// Gets the CancellationToken that will be cancelled when this GameObject is disabled. 
    /// Lazy initialization for better performance.
    /// </summary>
    public CancellationToken CancellationToken
    {
        get
        {
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();
            }

            return cancellationTokenSource.Token;
        }
    }

    /// <summary>
    /// Check if the token has been cancelled
    /// </summary>
    public bool IsCancelled => isDisableCalled;

    private void OnDisable()
    {
        if (isDisableCalled) return;

        isDisableCalled = true;

        // Cancel the token (but don't dispose yet, might be re-enabled)
        cancellationTokenSource?.Cancel();
    }

    private void OnEnable()
    {
        if (isDisableCalled && cancellationTokenSource != null)
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            isDisableCalled = false;
        }
    }

    private void OnDestroy()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }
}