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
    public void PlayParticleEffect()
    {
        Debug.Log("Particle effect triggered!");
        if (cubesEffect != null)
        {
            cubesEffect.Play();
        }
        if (poofEffect != null)
        {
            poofEffect.Play();
        }
    }
}
