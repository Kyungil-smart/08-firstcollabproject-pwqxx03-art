using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Paddle : MonoBehaviour
{
    [Multiline(11)]
    public string[] StageStr;
    public Sprite[] B;
    public GameObject P_Item;
    public SpriteRenderer P_ItemSr;

    public TMP_Text stageText;
    public TMP_Text scoreText;

    public GameObject Life_01;
    public GameObject Life_02;
    public GameObject victoryPanel;
    public GameObject GameOverPanel;
    public GameObject PausePanel;

    public AudioSource S_Break;
    public AudioSource S_Eat;
    public AudioSource S_Fail;
    public AudioSource S_HardBreak;
    public AudioSource S_Paddle;
    public AudioSource S_Victory;

    public Transform ItemsTr;
    public Transform BlocksTr;
    public BoxCollider2D[] BlockCol;

    public GameObject[] Ball;
    public Animator[] BallAni;
    public Transform[] BallTr;
    public SpriteRenderer[] BallSr;
    public Rigidbody2D[] BallRg;

    public SpriteRenderer PaddleSr;
    public BoxCollider2D PaddleCol;

    bool isStart = false;

    public float paddleSpeed = 8f;
    public float ballSpeed = 300f;

    float oldBallSpeed = 300f;
    float paddleBorder = 2.262f;
    float paddleSize = 1.58f;

    int combo;
    int score;
    int stage = 1;

    void Start()
    {
        Time.timeScale = 1;

        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (GameOverPanel != null)
            GameOverPanel.SetActive(false);

        if (stageText != null)
            stageText.text = "STAGE - " + stage;

        if (scoreText != null)
            scoreText.text = "SCORE " + score;
    }

    void Update()
    {
        PauseGame();
        MovePaddle();
        BallReadyPosition();
        StartBall();
    }

    void PauseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PausePanel == null) return;

            if (PausePanel.activeSelf)
            {
                PausePanel.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                PausePanel.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }

    void MovePaddle()
    {
        float h = Input.GetAxisRaw("Horizontal");
        Vector3 pos = transform.position;

        pos.x += h * paddleSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, -paddleBorder, paddleBorder);

        transform.position = pos;
    }

    void BallReadyPosition()
    {
        if (!isStart && BallTr.Length > 0)
        {
            BallTr[0].position = new Vector3(transform.position.x, transform.position.y + 0.35f, 0f);
        }
    }

    void StartBall()
    {
        if (!isStart && Input.GetKeyDown(KeyCode.Space))
        {
            isStart = true;

            if (BallRg.Length > 0)
            {
                BallRg[0].linearVelocity = Vector2.zero;
                BallRg[0].AddForce(Vector2.up * ballSpeed);
            }

            if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine("InfinityLoop")
        }   
    }

    IEnumerator InfinityLoop()
    { 
         while(true)
        {
            if (Input.GetMouseButton(0))
            yield return new WaitForSeconds(0.01f);
        } 
    }