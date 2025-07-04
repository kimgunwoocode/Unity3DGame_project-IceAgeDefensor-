﻿using UnityEngine;

public class IceManager : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private int dropPiece_min = 0;
    [SerializeField] private int dropPiece_max = 0;
    [Space]
    public IcePiece_in_IceOBJ IcePiece_In_IceOBJ;
    public Collider collider_;

    public AudioSource AudioSource_;
    public Animator body_animator;
    public Ice_HPbar Ice_HPbar;

    public ParticleSystem takedamage_particle;

    float attackRadius = 1f;
    public LayerMask playerLayer;


    [Header("Audio clips")]
    public AudioClip start_audio;
    public AudioClip hitted_audio;
    public AudioClip destroied_audio;

    private float currentHP;
    public float CurrentHP
    {
        get => currentHP;
        private set
        {
            currentHP = value;
            if (currentHP <= 0f && !isDead)
            {
                Die();
            }
        }
    }

    [HideInInspector] public bool isDead = false;

    // 폴링 관련 필드
    private IcePoolManager poolManager;
    private int typeIndex;
    [HideInInspector] public bool inPool = false;


    void Start()
    {
        CurrentHP = maxHP;
        AudioSource_.volume = GameManager.Instance.GameData.EffectSound_Volume;
        AudioSource_.clip = start_audio;
        AudioSource_.Play();
        if (IcePiece_In_IceOBJ == null)
        {
            IcePiece_In_IceOBJ = gameObject.GetComponent<IcePiece_in_IceOBJ>();
        }
    }

    private void Update()
    {
        if (currentHP <= 0f && !isDead && !inPool)
        {
            print("버그 발생. 파괴");
            ReturnToPool();
        }
    }
    public void Attack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRadius, playerLayer);

        foreach (var hit in hits)
        {
            PlayerManager player = hit.GetComponent<PlayerManager>();
            if (player != null)
            {
                player.Die_from_Ice();
            }
        }
    }


    public void Init(IcePoolManager pool, int index, float scale = 1f)
    {
        // 초기화 코드...
        inPool = false;
        poolManager = pool;
        typeIndex = index;
        isDead = false;
        collider_.enabled = true;
        body_animator.Rebind();
        CurrentHP = maxHP;
        Ice_HPbar.SetHealth(1f);
        attackRadius = gameObject.transform.localScale.x * 0.8f;

        transform.localScale = Vector3.one * scale; // 크기 반영
        gameObject.SetActive(true);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        CurrentHP -= amount;
        //print("얼음 TakeDamage : " + CurrentHP);
        Ice_HPbar.SetHealth(CurrentHP / maxHP);

        takedamage_particle.Play();

        AudioSource_.volume = GameManager.Instance.GameData.EffectSound_Volume;
        AudioSource_.clip = hitted_audio;
        AudioSource_.Play();

        if (CurrentHP <= 0)
        {
            //print("파괴 애니메이션 3");
            body_animator.SetInteger("destroy", 3);
            body_animator.SetTrigger("die");
        }
        else if(CurrentHP <= maxHP * 0.4f)
        {
            body_animator.SetInteger("destroy", 2);
            print("파괴 애니메이션 2");
        }
        else if (CurrentHP <= maxHP * 0.7f)
        {
            print("파괴 애니메이션 1");
            body_animator.SetInteger("destroy", 1);
        }
    }

    private void Die()
    {
        print("얼음파괴");
        //PlayerManager.Instance.attack_area.Exit_ice(collider_);
        isDead = true;
        collider_.enabled = false;
        AudioSource_.volume = GameManager.Instance.GameData.EffectSound_Volume;
        AudioSource_.clip = destroied_audio;
        AudioSource_.Play();
        body_animator.SetTrigger("die");

        int piece_num = Random.Range(dropPiece_min, dropPiece_max+1);
        IcePiece_In_IceOBJ.BreakAndSpawn(piece_num);
    }

    public void ReturnToPool()//애니메이션에서 타이밍맞게 부르기. 오브젝트를 아예 없애버리는거임
    {
        if (poolManager != null)
        {
            poolManager.ReturnIceToPool(gameObject, typeIndex);
        }
        else
        {
            Debug.LogWarning("PoolManager not assigned!");
            Destroy(gameObject); // 예외 상황 대비
        }
    }
}
