using UnityEngine;

namespace PlayerScripts
{
    public class Headphones : MonoBehaviour
    {
        public static Headphones Instance;
        
        [SerializeField] private bool autoStartAmbience = true;
        [SerializeField] private string ambienceToPlay = "Default";
        [Range(0, 1)] [SerializeField] private float ambienceIntensity = 1;

        private void Awake()
        {
            if(Instance != null)
                Destroy(this);

            Instance = this;
        }

        private void Start() => StartAmbience();

        void StartAmbience()
        {
            if(!autoStartAmbience)
                return;
            
            FModAudioManager.CreateSoundInstance(SoundInstanceType.Ambience, ambienceToPlay);
            FModAudioManager.SetInstanceParameter(SoundInstanceType.Ambience, "AmbienceIntensity", ambienceIntensity);
        }
    }
}
