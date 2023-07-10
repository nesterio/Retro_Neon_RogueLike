using Interface;
using UnityEngine;

namespace Interactable
{
    public class TextFieldInteraction : Interactable
    {
        [SerializeField] [TextArea] private string displayedText;
        
        private readonly OnScreenMessage _message = OnScreenMessage.Instance;

        public override void Use()
        {
            if(!isUsable)
                return;
            
            ShowText();
        }

        public void ShowText()
        {
            if(OnScreenMessage.IsShowingMessage)
                _message.HideMessage();
            
            _message.ShowMessage(displayedText);
        }
    }
}
