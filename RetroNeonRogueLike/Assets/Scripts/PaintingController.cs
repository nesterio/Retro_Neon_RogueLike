using System;
using TMPro;
using UnityEngine;

public class PaintingController : MonoBehaviour
{
    [Header("Size and scale")]
    [SerializeField] private int originalWidth;
    [SerializeField] private int originalHeight;
    [SerializeField] private float scaleMultiplier;
    [SerializeField] private float zSize;

    [Header("Text")] 
    [SerializeField] private string paintingName;
    [SerializeField] private string authorName;
    [SerializeField] private float textBottomPadding = 0.25f;

    private Transform paintingTransform;
    private TextMeshPro tmPro;
    
    private bool initialized = false;

    void Start()
    {
        paintingTransform = Utils.FindGameObjectByName("PaintingObj", transform).transform;
        tmPro = Utils.FindGameObjectByName("Text", transform).GetComponent<TextMeshPro>();

        if (paintingTransform == null || tmPro == null)
        {
            Debug.LogError("Couldn't initialize a painting: " + paintingName);
            return;
        }
        initialized = true;
        
        OnEnable();
    }

    private void OnEnable()
    {
        if(!initialized)
            return;
        
        UpdateSizeScale();
        
        UpdateText();
    }

    void UpdateSizeScale()
    {
        var ratio = (float)originalHeight / originalWidth;
        var newWidth = 1 * scaleMultiplier;
        var newHeight = 1 * ratio * scaleMultiplier;

        paintingTransform.localScale = new Vector3(newWidth, newHeight, zSize);
    }

    void UpdateText()
    {
        tmPro.text = $"{paintingName}{Environment.NewLine}{authorName}";

        var newPos = tmPro.transform.localPosition;
        newPos = new Vector3(newPos.x, -(paintingTransform.localScale.y / 2 + textBottomPadding),newPos.z);
        tmPro.transform.localPosition = newPos;
    }
}
