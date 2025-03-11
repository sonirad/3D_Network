using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Netcode;

public class NetEnergyOrb : NetworkBehaviour
{
    [Tooltip("�߻� �ʱ� �ӵ�")]
    public float speed = 10.0f;
    [Tooltip("����")]
    public float lifeTime = 20.0f;
    [Tooltip("���� ����")]
    public float expolsionRadius = 5.0f;
    private bool isUsed = false;

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
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            StartCoroutine(SelfDespawn());
        }
    }

    private IEnumerator SelfDespawn()
    {
        yield return new WaitForSeconds(lifeTime);

        if (IsServer)
        {
            this.NetworkObject.Despawn();
        }
        else
        {
            RequestDespawnServerRpc();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���ʰ� �ƴϸ� ����. spawn �Ǳ� ���� �Ͼ �浹�� ����.
        if (!IsOwner && !this.NetworkObject.IsSpawned)
        {
            return;
        }

        if (!isUsed)
        {
            Collider[] result = Physics.OverlapSphere(transform.position, expolsionRadius, LayerMask.GetMask("Player"));

            if (result.Length > 0)
            {
                foreach (Collider col in result)
                {
                    Debug.Log(col.gameObject.name);
                }
            }

            EffectProcessClientRpc();
        }
    }

    /// <summary>
    /// clientRpc : ������ ��� Ŭ���̾�Ʈ���� ���ÿ��� �����϶�� ���.
    /// </summary>
    [ClientRpc]
    private void EffectProcessClientRpc()
    {
        if (IsOwner)
        {
            rigid.useGravity = false;
            rigid.isKinematic = true;

            StartCoroutine(EffectFinishProcess());

            isUsed = true;
        }
    }

    private IEnumerator EffectFinishProcess()
    {
        int BaseSize_ID = Shader.PropertyToID("BaseSize");
        int EffectFinishEvent_ID = Shader.PropertyToID("OnEffectFinish");
        float elapsedTime = 0.0f;
        // 0.5�� ���� baseSize�� expolsionRadius���� Ȯ��
        float expendDuration = 0.5f;
        float preCompute = (1 / expendDuration) * expolsionRadius;

        while (elapsedTime < expendDuration)
        {
            elapsedTime += Time.deltaTime;
            effect.SetFloat(BaseSize_ID, elapsedTime * preCompute);

            yield return null;
        }

        // 1�� ���� baseSize�� 0�� �ɶ����� ���
        float reductionDuration = 1.0f;
        elapsedTime = reductionDuration;
        float preCompute2 = 1 / reductionDuration;

        while (elapsedTime > 0.0f)
        {
            elapsedTime -= Time.deltaTime;
            effect.SetFloat(BaseSize_ID, elapsedTime * preCompute2 * expolsionRadius);

            yield return null;
        }

        // ��ƼŬ ���� ����
        effect.SendEvent(EffectFinishEvent_ID);

        // ��ƼŬ ������ 0�� �Ǹ� ���� ������Ʈ ����
        while (effect.aliveParticleCount > 0)
        {
            yield return null;
        }

        // ��� ��ƼŬ ����
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
        else
        {
            RequestDespawnServerRpc();
        }
    }

    [ServerRpc]
    private void RequestDespawnServerRpc()
    {
        this.NetworkObject.Despawn();
    }
}
