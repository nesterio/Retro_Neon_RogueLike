using System;
using Lean.Gui;
using PlayerScripts;
using SL.Wait;
using UnityEngine;

namespace Interactable
{
    public class BasicPopupInteraction : Interactable
    {
        [SerializeField] private GameObject MainCanvas;
        [SerializeField] private GameObject PopupPrefab;
        [SerializeField] [TextArea] private string displayedText;
        public Action OnCloseAction;

        public override void Use()
        {
            if(!isUsable || !PlayerManager.CanUse)
                return;
            
            ShowText();
        }

        private void ShowText()
        {
            var popup = Lean.Pool.LeanPool.Spawn(PopupPrefab, MainCanvas.transform);

            popup.GetComponent<TextUpdater>().UpdateText(displayedText);

            var leanWindow = popup.GetComponent<LeanWindow>();
            leanWindow.OnOn.AddListener(() =>
            {
                isUsable = false;

                var wait = Wait.Seconds(1f, 
                    () => Wait.For(() => InputManagerData.Shooting, leanWindow.TurnOff).Start());

                wait.Start();
            });
            leanWindow.OnOff.AddListener(() =>
            {
                var wait = Wait.Seconds(1f, ()=> isUsable = true);
                wait.Start();
                
                OnCloseAction?.Invoke();

                leanWindow.OnOn.RemoveAllListeners();
                leanWindow.OnOff.RemoveAllListeners();
            });
            isUsable = false;
            leanWindow.TurnOn();
        }
    }
}
