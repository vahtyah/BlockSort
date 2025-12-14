using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    
    private BlockData _blockData;
    private Material _material;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
        
        _material = _renderer.material;
    }
    
    public void Initialize(BlockData blockData)
    {
        _blockData = blockData;
    }
    
    public void SetColor(Color color)
    {
        _material.color = color;
    }
    
    public int GetColor()
    {
        return _blockData.Color;
    }

    private void OnValidate()
    {
        if (_renderer == null)
            _renderer = GetComponent<Renderer>();
    }
}
