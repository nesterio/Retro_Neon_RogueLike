using Interactable;
using Interactable.Items;
using SL.Wait;
using UnityEngine;
using IM = InputManagerData;

namespace PlayerScripts
{
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private Transform cameraParentTrans;
        [SerializeField] private ItemManager itemManager;
        [Space]
        [SerializeField] private float itemPickupRange = 3f;

        private GameObject selectedObj;

        private void Update()
        {
            ItemDetection();
        }

        void ItemDetection() 
        {
            if (Physics.Raycast(cameraParentTrans.position, cameraParentTrans.forward, out var hit, itemPickupRange))
            {
                if (selectedObj != hit.collider.gameObject)
                {
                    if (selectedObj != null && selectedObj.CompareTag("Interactable"))
                        selectedObj.GetComponent<Interactable.Interactable>().HideInteractionHint();

                    selectedObj = hit.collider.gameObject;
                }
                
                if (selectedObj.CompareTag("Interactable"))
                    AwaitInteraction();
            }
        }

        void AwaitInteraction()
        {
            var interactable = selectedObj.GetComponent<Interactable.Interactable>();
            if(interactable == null) return;
            
            if(!interactable.isUsable)
                return;
            
            if(interactable.awaitInput)
                interactable.ShowInteractionHint();

            bool shouldUse = !interactable.awaitInput ||
                             interactable.awaitInput && IM.Interacting;
            switch (interactable)
            {
                case Item item :
                    if(shouldUse) ProcessItem(item);
                    break;
                
                case BasicPopupInteraction basicPopupInteraction :
                    if(shouldUse) ProcessPopup(basicPopupInteraction);
                    break;
            }
        }

        void ProcessItem(Item item)
        {
            if(item.isUsable && !item.isPickable 
                             && !itemManager.PickingUpItem && itemManager.CanPickupItem)
                itemManager.PickUpItem(item.gameObject);
        }

        void ProcessPopup(BasicPopupInteraction basicPopupInteraction)
        {
            bool canUse = false;
            
            if(basicPopupInteraction.isUsable)
                if (!basicPopupInteraction.awaitInput)
                    canUse = true;
                else if(IM.Interacting)
                    canUse = true;

            if (canUse)
            {
                basicPopupInteraction.OnCloseAction = () =>
                {
                    var wait = Wait.Seconds(0.5f, PlayerManager.UnFreezePlayer);
                    wait.Start();
                };
                basicPopupInteraction.Use();
                PlayerManager.FreezePlayer();
            }
        }

    }
}
