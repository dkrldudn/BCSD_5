using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    bool isDead;
    Rigidbody2D rigid;
    public int nextMove;
    Animator anim;
    SpriteRenderer spriteRenderer;
    BoxCollider2D Collider;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<BoxCollider2D>();
        Invoke("Think", 5);
    }

    void FixedUpdate()
    {
        if(isDead) return;
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);

        //정지 중에는 낭떨어지 검사하지 않음
        if(nextMove == 0)
        {
            return;
        }

        //platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.2f, rigid.position.y);

        Debug.DrawRay(frontVec, Vector3.down, Color.green);

        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector2.down, 1f, LayerMask.GetMask("Platform"));

        if(rayHit.collider == null)
        {
            nextMove *= -1;

            //flip Sprite
            spriteRenderer.flipX = nextMove == 1;

            CancelInvoke(nameof(Think));
            Invoke(nameof(Think), 5f);
        }
    }

    //재귀함수
    void Think()
    {   
        if(isDead) return;
        
        //Set next active
        nextMove = Random.Range(-1, 2);

        //Sprite Animation
        anim.SetInteger("WalkSpeed", nextMove);
        
        //Flip Sprite
        if(nextMove != 0)
        {
            spriteRenderer.flipX = nextMove == 1;
        }

        //Recursive
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    public void OnDamaged()
    {
        if(isDead) return;

        isDead = true;

        //stop AI
        CancelInvoke(nameof(Think));
        nextMove = 0;
        rigid.linearVelocity = Vector2.zero;

        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        //Sprite Flip Y
        spriteRenderer.flipY = true;

        //Collider Disable
        Collider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //Destroy
        Invoke("DeActive", 5f);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
