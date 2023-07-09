using TMPro;
using UnityEngine;

namespace Interactable
{
    public class TextFieldInteraction : Interactable
    {
        [SerializeField] [TextArea] private string displayedText;
        
        public override void Use()
        {
            if(!isUsable)
                return;
            
            ShowText();
        }

        public void ShowText()
        {
            var obj = ObjectPool.Instance.SpawnFromPoolUI("TextField");
            var text = obj.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

            if (text == null)
            {
                Debug.Log("No text component on object");
                return;
            }

            text.text = displayedText;
        }
    }
}
