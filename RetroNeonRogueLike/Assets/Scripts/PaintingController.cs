using System;
using TMPro;
using UnityEngine;

public class PaintingController : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private TextMeshPro _tmPro;
    [Space]
    //[SerializeField] private Material material;
    
    [Header("Size and scale")]
    [SerializeField] private int originalWidth;
    [SerializeField] private int originalHeight;
    [SerializeField] private float scaleMultiplier;
    [SerializeField] private float zSize;

    [Header("Text")] 
    [SerializeField] private string paintingName;
    [SerializeField] private string authorName;

    void Start() => OnEnable();
    private void OnEnable()
    {
        //_renderer.material = material;
        
        UpdateSizeScale();
        
        UpdateText();
    }

    void UpdateSizeScale()
    {
        var ratio = (float)originalHeight / originalWidth;
        var newWidth = 1 * scaleMultiplier;
        var newHeight = 1 * ratio * scaleMultiplier;

        _renderer.transform.localScale = new Vector3(newWidth, newHeight, zSize);
    }

    void UpdateText()
    {
        _tmPro.text = $"{paintingName}{Environment.NewLine}{authorName}";
    }
}
