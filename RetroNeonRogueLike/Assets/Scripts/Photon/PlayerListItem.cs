using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Photon
{
    public class PlayerListItem : MonoBehaviourPunCallbacks
    {
        [SerializeField] TMP_Text text;
        Photon.Realtime.Player _player;

        public void SetUp(Photon.Realtime.Player player) 
        {
            _player = player;
            text.text = _player.NickName;
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer) 
        {
            if (_player == otherPlayer)
                Destroy(gameObject);
        }

        public override void OnLeftRoom()=>
            Destroy(gameObject);
    }
}
