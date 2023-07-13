using PlayerScripts;

namespace Interactable
{
    public class ChangeHealthInteractable : Interactable
    {
        public int changeAmount = 0;

        private void Start()
        {
            if(string.IsNullOrEmpty(viewHint))
                viewHint = "Change health by: " + changeAmount;
        }

        public override void Use()
        {
            if(changeAmount == 0)
                return;
            
            PlayerManager.PlayerStats.DrainHealth(changeAmount);
        }
    }
}
