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
        public virtual string UseSoundName() => "Use";

        public override void Use()
        {
            if(!IsUsable || !PlayerManager.CanUse)
                return;
            
            FModAudioManager.PlaySound(UseSoundName(), PlayerManager.ItemsManager.transform.position);
            
            ShowText();
        }

        private void ShowText()
        {
            var popup = Lean.Pool.LeanPool.Spawn(PopupPrefab, MainCanvas.transform);

            popup.GetComponent<TextUpdater>().UpdateText(displayedText);

            var leanWindow = popup.GetComponent<LeanWindow>();
            leanWindow.OnOn.AddListener(() =>
            {
                IsUsable = false;

                var wait = Wait.Seconds(1f, 
                    () => Wait.For(() => InputManagerData.Shooting, leanWindow.TurnOff).Start());

                wait.Start();
            });
            leanWindow.OnOff.AddListener(() =>
            {
                var wait = Wait.Seconds(1f, ()=> IsUsable = true);
                wait.Start();
                
                OnCloseAction?.Invoke();

                leanWindow.OnOn.RemoveAllListeners();
                leanWindow.OnOff.RemoveAllListeners();
            });
            IsUsable = false;
            leanWindow.TurnOn();
        }
    }
}
