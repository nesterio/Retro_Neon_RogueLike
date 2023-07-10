using TMPro;
using UnityEngine;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textObj;

    public void UpdateText(string message)
    {
        if (textObj == null)
        {
            Debug.Log("Text object is null or unassigned");
            return;
        }

        textObj.text = message;
    }
}
