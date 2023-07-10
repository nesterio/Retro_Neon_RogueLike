using SL.Wait;
using TMPro;
using UnityEngine;

namespace Interface
{
    public class OnScreenMessage : MonoBehaviour
    {
        public static OnScreenMessage Instance;
        private TextMeshProUGUI _textMesh;

        public static bool IsShowingMessage { get; private set; } = false;

        private void Start()
        {
            if(Instance != null)
                Destroy(gameObject);

            Instance = this;

            _textMesh = GetComponent<TextMeshProUGUI>();

            if (_textMesh.enabled)
                _textMesh.enabled = false;
        }

        public void ShowMessage(string message, float timer = 0)
        {
            if (IsShowingMessage)
                return;
            
            IsShowingMessage = true;
            _textMesh.text = message;
            _textMesh.enabled = true;

            if (timer > 0)
                Wait.Seconds(timer, HideMessage);
        }

        public void HideMessage()
        {
            IsShowingMessage = false;
            _textMesh.text = "";
            _textMesh.enabled = false;
        }
    }
}
