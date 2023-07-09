using UnityEngine;

public static class ParticleManager
{
    public static void CreateParticles(string prefabName, Vector3 position, Quaternion rotation, string soundName = null)
    {
        GameObject particleObj = ObjectPool.Instance.SpawnFromPoolObject(prefabName, position, rotation);
        
        var particleSystem = particleObj.AddComponent(typeof(ParticleSystem)) as ParticleSystem;
        if (particleSystem == null)
        {
            Debug.Log("This is not a particle system... bruh...");
            return;
        }
        
        if(particleSystem.isPlaying)
            particleSystem.Stop();
        
        particleSystem.Play();
        
        // TODO: Play sound
        
        Object.Destroy(particleObj, 5);
    }
}