using UnityEngine;
using UnityEngine.Events;

public class ParticlePlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem cubesEffect;
    [SerializeField] private ParticleSystem poofEffect;
    public static ParticlePlayer Instance { get; private set; }
    private UnityEvent OnParticleEffectTriggered;
        private void Start()
        {
            Instance = this;
            
        }
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void PlayParticleEffect(Vector3 position)
    {
        Debug.Log("Particle effect triggered!");

        // Move the particle systems to the enemy's position
        transform.position = position;

        if (cubesEffect != null)
            cubesEffect.Play();

        if (poofEffect != null)
            poofEffect.Play();
    }

}
