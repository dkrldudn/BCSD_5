using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    public int nextMove;
    Animator anim;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Invoke("Think", 5);
    }

    void FixedUpdate()
    {
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);

        //Platform Check
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove*0.2f, rigid.position.y);
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
        if(rayHit.collider == null)
        {
            nextMove *= -1;
            //낭떠러지에서 방향 전환
            if(nextMove != 0)
            {
                spriteRenderer.flipX = nextMove == 1;
            }

            CancelInvoke();
            Invoke("Think", 5);
        }
    }

    //재귀함수
    void Think()
    {   
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
}
