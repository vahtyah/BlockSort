using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    
    private Material _material;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        
        _material = _renderer.material;
    }
    
    public void SetColor(Color color)
    {
        _material.color = color;
    }

    private void OnValidate()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
    }
}
