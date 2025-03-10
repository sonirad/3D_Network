using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class NetEnergyOrb : MonoBehaviour
{
    [Tooltip("발사 초기 속도")]
    public float speed = 10.0f;
    [Tooltip("수명")]
    public float lifeTime = 20.0f;
    [Tooltip("폭발 범위")]
    public float expolsionRadius = 5.0f;
    private Rigidbody rigid;
    private VisualEffect effect;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        effect = GetComponent<VisualEffect>();
    }

    private void Start()
    {
        transform.Rotate(-30.0f, 0, 0);

        rigid.velocity = speed * transform.forward;

        Destroy(this.gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider[] result = Physics.OverlapSphere(transform.position, expolsionRadius, LayerMask.GetMask("Player"));

        if (result.Length > 0)
        {
            foreach (Collider col in result)
            {
                Debug.Log(col.gameObject.name);
            }
        }

        StartCoroutine(EffectFinishProcess());
    }

    private IEnumerator EffectFinishProcess()
    {
        yield return null;
    }
}
