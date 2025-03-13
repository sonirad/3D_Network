using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Netcode;
using System.Collections.Generic;

public class NetEnergyOrb : NetworkBehaviour
{
    [Tooltip("�߻� �ʱ� �ӵ�")]
    public float speed = 10.0f;
    [Tooltip("����")]
    public float lifeTime = 20.0f;
    [Tooltip("���� ����")]
    public float expolsionRadius = 5.0f;
    // private bool isUsed = false;

    private Rigidbody rigid;
    private VisualEffect effect;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        effect = GetComponent<VisualEffect>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner && IsServer)
        {
            transform.Rotate(-30.0f, 0, 0);
            rigid.velocity = speed * transform.forward;
            StartCoroutine(SelfDespawn());
        }
    }

    private IEnumerator SelfDespawn()
    {
        yield return new WaitForSeconds(lifeTime);

        if (IsOwner && this.NetworkObject.IsSpawned)
        {
            if (IsServer)
            {
                this.NetworkObject.Despawn();
            }
            else
            {
                RequestDespawnServerRpc();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ���ʰ� �ƴϸ� ����. spawn �Ǳ� ���� �Ͼ �浹�� ����.
        if (!this.NetworkObject.IsSpawned)
        {
            return;
        }

        Collider[] result = Physics.OverlapSphere(transform.position, expolsionRadius, LayerMask.GetMask("Player"));

        if (result.Length > 0)
        {
            List<ulong> targets = new List<ulong>(result.Length);

            foreach (Collider col in result)
            {
                NetPlayer hitted = col.gameObject.GetComponent<NetPlayer>();

                targets.Add(hitted.OwnerClientId);
            }

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams()
                {
                    TargetClientIds = targets.ToArray()
                }
            };

            PlayerDieClientRpc(clientRpcParams);
        }

        EffectProcessClientRpc();
    }

    /// <summary>
    /// clientRpc : ������ ��� Ŭ���̾�Ʈ���� ���ÿ��� �����϶�� ���.
    /// </summary>
    [ClientRpc]
    private void EffectProcessClientRpc()
    {
        rigid.useGravity = false;
        rigid.drag = Mathf.Infinity;

        StartCoroutine(EffectFinishProcess());
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
            if (IsOwner)
            {
                RequestDespawnServerRpc();
            }
        }
    }

    [ServerRpc]
    private void RequestDespawnServerRpc()
    {
        this.NetworkObject.Despawn();
    }

    [ServerRpc]
    private void SetVelocityServerRpc(Vector3 newVelocity)
    {
        rigid.velocity = newVelocity;
    }

    [ClientRpc]
    private void PlayerDieClientRpc(ClientRpcParams clientRpcSendParams = default)
    {
        NetPlayer player = GameManager.Instance.Player;

        player.SendChat($"[{GameManager.Instance.Player.name}]�� �׾���.");
    }
}
