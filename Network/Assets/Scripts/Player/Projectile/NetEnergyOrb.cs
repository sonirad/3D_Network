using System.Collections;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Netcode;
using System.Collections.Generic;

public class NetEnergyOrb : NetworkBehaviour
{
    [Tooltip("발사 초기 속도")]
    public float speed = 10.0f;
    [Tooltip("수명")]
    public float lifeTime = 20.0f;
    [Tooltip("폭발 범위")]
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
        // 오너가 아니면 무시. spawn 되기 전에 일어난 충돌은 무시.
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
    /// clientRpc : 서버가 모든 클라이언트에게 로컬에서 실행하라고 명령.
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
        // 0.5초 동안 baseSize를 expolsionRadius까지 확대
        float expendDuration = 0.5f;
        float preCompute = (1 / expendDuration) * expolsionRadius;

        while (elapsedTime < expendDuration)
        {
            elapsedTime += Time.deltaTime;
            effect.SetFloat(BaseSize_ID, elapsedTime * preCompute);

            yield return null;
        }

        // 1초 동안 baseSize가 0이 될때까지 축소
        float reductionDuration = 1.0f;
        elapsedTime = reductionDuration;
        float preCompute2 = 1 / reductionDuration;

        while (elapsedTime > 0.0f)
        {
            elapsedTime -= Time.deltaTime;
            effect.SetFloat(BaseSize_ID, elapsedTime * preCompute2 * expolsionRadius);

            yield return null;
        }

        // 파티클 생성 중지
        effect.SendEvent(EffectFinishEvent_ID);

        // 파티클 갯수가 0이 되면 게임 오브젝트 제거
        while (effect.aliveParticleCount > 0)
        {
            yield return null;
        }

        // 모든 파티클 제거
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

        player.SendChat($"[{GameManager.Instance.Player.name}]이 죽었음.");
    }
}
