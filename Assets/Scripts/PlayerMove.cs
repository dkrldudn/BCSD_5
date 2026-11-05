using UnityEngine;

public class Player : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;
    public float maxSpeed;
    public float jumpPower;

    bool isDead;
    bool isInvulnerable;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    BoxCollider2D Collider;
    AudioSource audioSource;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        Collider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySound(string action)
    {
        AudioClip clip = null;

        switch (action)
        {
            case "JUMP":
                clip = audioJump;
                break;
            case "ATTACK":
                clip = audioAttack;
                break;
            case "DAMAGED":
                clip = audioDamaged;
                break;
            case "ITEM":
                clip = audioItem;
                break;
            case "DIE":
                clip = audioDie;
                break;
            case "FINISH":
                clip = audioFinish;
                break;
        }

        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void Update()
    {
        if(isDead) return;

        //점프
        if(Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        //stop speed
        if(Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        //방향 전환
        if(Input.GetButton("Horizontal"))
        {
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
        }

        //애니메이션
        if(Mathf.Abs(rigid.linearVelocity.x) < 0.3)
        {
            anim.SetBool("isWalking", false);
        }
        else
        {
            anim.SetBool("isWalking", true);
        }
    }

    void FixedUpdate()
    {
        if(isDead) return;

        //move speed
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if(rigid.linearVelocity.x > maxSpeed)
        {
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        }
        else if(rigid.linearVelocity.x < -maxSpeed)
        {
            rigid.linearVelocity = new Vector2(-maxSpeed, rigid.linearVelocity.y);
        }

        //landing platform
        if (rigid.linearVelocity.y <= 0)
        {
            Debug.DrawRay(rigid.position, Vector2.down * 1.2f, Color.green);

            RaycastHit2D rayHit = Physics2D.Raycast(
                rigid.position,
                Vector2.down,
                1.2f,
                LayerMask.GetMask("Platform")
            );

            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.7f)
                {
                    anim.SetBool("isJumping", false);
                }
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //attack
            if(rigid.linearVelocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else
            {
                OnDamaged(collision.transform.position);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Item")
        {
            //point
            bool isBronze = collision.gameObject.name.Contains("Item_bronze");
            bool isSilver = collision.gameObject.name.Contains("Item_silver");
            bool isGold = collision.gameObject.name.Contains("Item_gold");

            if(isBronze)
            {
                gameManager.stagePoint += 50;
            }
            else if(isSilver)
            {
                gameManager.stagePoint += 100;
            }
            else if(isGold)
            {
                gameManager.stagePoint += 300;
            }
            
            PlaySound("ITEM");

            // Deactive Item
            collision.gameObject.SetActive(false);
        }
        else if(collision.gameObject.tag == "Finish")
        {
            Debug.Log("Finish 도착!");
            PlaySound("FINISH");
            gameManager.NextStage();
        }
    }

    void OnAttack(Transform enemy)
    {
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();

        if(enemyMove == null)
        {
            Debug.LogWarning(enemy.name + "오브젝트에 EnemyMove가 없습니다.");
            return;
        }

        //point
        gameManager.stagePoint += 100;

        //Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        
        //enemy die
        enemyMove.OnDamaged();

        PlaySound("ATTACK");
    }

    void OnDamaged(Vector3 targetPos)
    {
        //사망 또는 무적 상태라면 피격 무시
        if(isDead || isInvulnerable) return;

        //health down
        gameManager.HealthDown();

        //마지막 체력을 잃었다면 사망 처리만 실행
        if(isDead) return;

        PlaySound("DAMAGED");

        isInvulnerable = true;

        //change layer 
        gameObject.layer = 11;
        //view alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //reaction force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc,1)*7, ForceMode2D.Impulse);

        //애니메이션
        anim.SetTrigger("doDamaged");

        Invoke("OffDamaged", 3);
    }

    void OffDamaged()
    {
        if(isDead) return;

        isInvulnerable = false;

        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        if(isDead) return;

        isDead = true;

        PlaySound("DIE");

        //예약된 무적 해제 취소
        CancelInvoke(nameof(OffDamaged));

        //Camera stop follow
        if(Camera.main != null && Camera.main.transform.IsChildOf(transform))
        {
            Camera.main.transform.SetParent(null, true);
        }

        //sprite alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //sprite flip y
        spriteRenderer.flipY = true;
        //collider disable
        Collider.enabled = false;
        //die effect jump
        rigid.linearVelocity = Vector2.zero;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.linearVelocity = Vector2.zero;
    }
}
