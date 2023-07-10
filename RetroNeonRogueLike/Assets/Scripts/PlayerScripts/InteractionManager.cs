using Interactable;
using Interactable.Items;
using Interface;
using SL.Wait;
using TMPro;
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
                
                case TextFieldInteraction textFieldInteraction :
                    if(shouldUse) ProcessTextField(textFieldInteraction);
                    break;
            }
        }

        void ProcessItem(Item item)
        {
            if(item.isUsable && !item.pickedUp 
                             && !itemManager.PickingUpItem && itemManager.CanPickupItem)
                itemManager.PickUpItem(item.gameObject);
        }

        void ProcessTextField(TextFieldInteraction textFieldInteraction)
        {
            if(textFieldInteraction.isUsable)
                if(!textFieldInteraction.awaitInput)
                    textFieldInteraction.Use();
                else if(IM.Interacting)
                    textFieldInteraction.Use();
        }

        void ProcessButton()
        {
            
        }

    }
}
