using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    [SerializeField] private ParticleSystem cubesEffect;
    [SerializeField] private ParticleSystem poofEffect;

    public static ParticlePlayer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayParticleEffect(Vector3 position)
    {
        transform.position = position;

        if (cubesEffect != null)
            cubesEffect.Play();

        if (poofEffect != null)
            poofEffect.Play();
    }

    public ParticleSystem PlayAttackParticle(Vector3 position, ParticleSystem effectPrefab, Transform target)
    {
        transform.position = position;

        // Rotate the particle so its forward direction points toward the target
        Vector3 direction = (target.position - position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        ParticleSystem effect = Instantiate(effectPrefab, position, rotation);
        effect.Play();

        return effect;
    }
}
