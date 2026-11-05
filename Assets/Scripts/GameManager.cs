
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public Player player;
    public GameObject[] Stages;

    public Image[] UIhealth;
    public TMP_Text UIPoint;
    public TMP_Text UIStage;
    public GameObject RestartBtn;

    bool isGameOver;

    void Start()
    {
        Time.timeScale = 1f;

        //시작할 때 Retry 버튼 숨기기
        RestartBtn.SetActive(false);
    }

    void Update()
    {
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        if (isGameOver) return;

        //Stage Point
        totalPoint += stagePoint;
        stagePoint = 0;

        //change stage
        if(stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            //Player Reposition
            PlayerReposition();

            UIStage.text = "STAGE" + (stageIndex+1);
        }
        else
        {
            //Game Clear
            //Player Contol Lock
            Time.timeScale = 0;
            //Result UI
            Debug.Log("게임 클리어");
            //Restart Button UI
        }
    }

    public void HealthDown()
    {
        if(isGameOver || health <= 0) return;

        //health down
        health--;

        //health UI
        if(health < UIhealth.Length && UIhealth[health] != null)
        {
            UIhealth[health].color = new Color(1, 0, 0, 0.4f);
        }

        //Game over
        if(health <= 0)
        {
            health = 0;
            isGameOver = true;

            //Player Die
            if(player != null)
            {
                player.OnDie();
            }

            //Result UI
            Debug.Log("게임 오버! Retry 버튼 표시");

            RestartBtn.SetActive(true);
            RestartBtn.transform.SetAsLastSibling();

            Debug.Log("버튼 활성 상태: " + RestartBtn.activeInHierarchy);

            //Retry Button UI
            if(RestartBtn != null)
            {
                RestartBtn.SetActive(true);
                RestartBtn.transform.SetAsLastSibling();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || isGameOver)
            return;

        Rigidbody2D playerRigid = collision.attachedRigidbody;

        if (playerRigid == null)
            return;

        // Health Down
        HealthDown();

        // 게임 오버라면 리스폰하지 않음
        if (isGameOver)
            return;

        // Player Reposition
        playerRigid.linearVelocity = Vector2.zero;
        playerRigid.transform.position = new Vector3(0, 0, -1);
    }

    void PlayerReposition()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();
    }

    public void RestartGame()
    {
        Debug.Log("Retry 버튼 클릭 성공!");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
