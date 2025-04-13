using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicParticleManager : MonoBehaviour
{
    public enum Particle  // for referencing which particle should be shown -> index matches particlePrefabs list
    {
        None,
        DrawingStars
    }
    
    public static MagicParticleManager Instance;
    
    [SerializeField]
    List<GameObject> particlePrefabs;

    Dictionary<Particle, List<GameObject>> particlePools;

    private void Awake()
    {
        if (Instance) Destroy(Instance);
        
        Instance = this;
        particlePools = new Dictionary<Particle, List<GameObject>>();
    }

    public void GetParticle(Particle type, Vector3 worldPos)
    {
        if (type == Particle.None) return;
        else if ((int)type - 1 > particlePrefabs.Count)
        {
            Debug.LogError($"No particle assigned to {type}! Cannot spawn particle");
            return;
        }
        
        // get particle (reused or new)
        GameObject particle;
        if (particlePools.ContainsKey(type) && particlePools[type].Count > 0)
        {
            // take from pool
            particle = particlePools[type][0];
            particlePools[type].Remove(particle);
        }
        else
        {
            // create new particle
            particle = Instantiate(particlePrefabs[(int)type - 1]);
        }
        
        // visualize particle
        particle.transform.position = worldPos;
        particle.SetActive(true);
        
        // allow particle to reset (return to pool)
        var particleTime = particle.GetComponent<ParticleSystem>()?.main.duration ?? 1F;
        StartCoroutine(ReturnParticle(particle, type, particleTime));
    }

    /// <summary>
    /// Returns particle to pool so it can be reused
    /// </summary>
    IEnumerator ReturnParticle(GameObject pref, Particle type, float time)
    {
        yield return new WaitForSeconds(time);
        yield return null;

        if (!particlePools.ContainsKey(type))
        {
            particlePools.Add(type, new List<GameObject>());
        }
        
        pref.SetActive(false);
        particlePools[type].Add(pref);
    }
}