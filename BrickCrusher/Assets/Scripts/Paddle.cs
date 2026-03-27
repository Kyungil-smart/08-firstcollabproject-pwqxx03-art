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
    public GameObject Life_03;
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
    bool isPaused = false;
    bool isEnding = false;

    public float paddleSpeed = 8f;
    public float ballSpeed = 300f;

    float oldBallSpeed = 300f;
    float paddleBorder = 2.262f;
    float paddleSize = 1.58f;
    public float paddleX;

    int combo;
    int score;
    int stage = 1;
    int life = 3;

    Coroutine paddleSizeCoroutine;
    Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Start()
    {
        Time.timeScale = 1f;

        if (PausePanel != null) PausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (GameOverPanel != null) GameOverPanel.SetActive(false);

        UpdateUI();
        UpdateLifeUI();
        GenerateStage();
        ResetBallState();
    }

    void Update()
    {
        HandlePause();

        if (isPaused || isEnding)
            return;

        MovePaddle();
        BallReadyPosition();
        StartBall();
    }

    void HandlePause()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (PausePanel == null)
            return;

        isPaused = !isPaused;
        PausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    void MovePaddle()
    {
        if (mainCam == null)
            return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        paddleX = Mathf.Clamp(mouseWorldPos.x, -paddleBorder, paddleBorder);

        transform.position = new Vector3(
            paddleX,
            transform.position.y,
            transform.position.z
        );
    }

    void BallReadyPosition()
    {
        if (isStart)
            return;

        if (BallTr == null || BallTr.Length == 0 || BallTr[0] == null)
            return;

        BallTr[0].position = new Vector3(
            transform.position.x,
            transform.position.y + 0.35f,
            0f
        );
    }

    void StartBall()
    {
        if (isStart)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (BallRg == null || BallRg.Length == 0 || BallRg[0] == null)
            return;

        isStart = true;
        BallRg[0].linearVelocity = Vector2.zero;
        BallRg[0].AddForce(new Vector2(0.15f, 1f).normalized * ballSpeed);
    }

    public void BallHitPaddle(Transform hitBall, Rigidbody2D hitRb)
    {
        if (hitBall == null || hitRb == null)
            return;

        Vector2 dir = (hitBall.position - transform.position).normalized;

        if (dir.y < 0.2f)
            dir.y = 0.2f;

        hitRb.linearVelocity = Vector2.zero;
        hitRb.AddForce(dir.normalized * ballSpeed);

        combo = 0;

        if (S_Paddle != null)
            S_Paddle.Play();
    }

    public void BallOut(GameObject targetBall)
    {
        if (targetBall != null)
            targetBall.SetActive(false);

        int activeBallCount = 0;

        for (int i = 0; i < Ball.Length; i++)
        {
            if (Ball[i] != null && Ball[i].activeSelf)
                activeBallCount++;
        }

        if (activeBallCount > 0)
            return;

        life--;
        UpdateLifeUI();

        if (S_Fail != null)
            S_Fail.Play();

        if (life <= 0)
        {
            StartCoroutine(EndGame(false));
            return;
        }

        StartCoroutine(ResetRound());
    }

    IEnumerator ResetRound()
    {
        isEnding = true;
        isStart = false;
        combo = 0;

        DisableExtraBalls();

        yield return new WaitForSeconds(0.7f);

        ResetBallState();
        isEnding = false;
    }

    void ResetBallState()
    {
        DisableExtraBalls();

        if (Ball.Length > 0 && Ball[0] != null)
            Ball[0].SetActive(true);

        if (BallRg.Length > 0 && BallRg[0] != null)
        {
            BallRg[0].linearVelocity = Vector2.zero;
            BallRg[0].angularVelocity = 0f;
        }

        if (BallTr.Length > 0 && BallTr[0] != null)
        {
            BallTr[0].position = new Vector3(
                transform.position.x,
                transform.position.y + 0.35f,
                0f
            );
        }
    }

    void DisableExtraBalls()
    {
        for (int i = 0; i < Ball.Length; i++)
        {
            if (Ball[i] == null)
                continue;

            if (i == 0)
                Ball[i].SetActive(true);
            else
                Ball[i].SetActive(false);

            if (i < BallRg.Length && BallRg[i] != null)
            {
                BallRg[i].linearVelocity = Vector2.zero;
                BallRg[i].angularVelocity = 0f;
            }
        }
    }

    public void HitBlock(GameObject blockObj, SpriteRenderer blockSr, Animator blockAni)
    {
        if (blockObj == null)
            return;

        string blockName = blockObj.name;

        if (blockName.StartsWith("HardBlock0"))
        {
            blockObj.name = "HardBlock1";

            if (blockSr != null && B.Length > 9)
                blockSr.sprite = B[9];

            if (S_HardBreak != null)
                S_HardBreak.Play();

            return;
        }

        if (blockName.StartsWith("HardBlock1"))
        {
            blockObj.name = "HardBlock2";

            if (blockSr != null && B.Length > 10)
                blockSr.sprite = B[10];

            if (S_HardBreak != null)
                S_HardBreak.Play();

            return;
        }

        if (blockName.StartsWith("HardBlock2") || blockName.StartsWith("Block"))
        {
            BreakBlock(blockObj, blockAni);
        }
    }

    void BreakBlock(GameObject blockObj, Animator blockAni)
    {
        DropItem(blockObj.transform.position);

        combo++;
        score += combo > 3 ? 3 : combo;
        UpdateUI();

        if (blockAni != null)
            blockAni.SetTrigger("Break");

        if (S_Break != null)
            S_Break.Play();

        StartCoroutine(DisableObjectLater(blockObj, 0.2f));
        StartCoroutine(CheckStageClear());
    }

    IEnumerator DisableObjectLater(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null)
            obj.SetActive(false);
    }

    IEnumerator CheckStageClear()
    {
        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < BlockCol.Length; i++)
        {
            if (BlockCol[i] != null && BlockCol[i].gameObject.activeSelf)
                yield break;
        }

        stage++;
        UpdateUI();

        if (stage > StageStr.Length)
        {
            StartCoroutine(EndGame(true));
            yield break;
        }

        GenerateStage();
        StartCoroutine(ResetRound());
    }

    void DropItem(Vector2 pos)
    {
        if (P_Item == null || ItemsTr == null)
            return;

        if (Random.value > 0.08f)
            return;

        int itemType = Random.Range(0, 3);

        GameObject item = Instantiate(P_Item, pos, Quaternion.identity, ItemsTr);

        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

        switch (itemType)
        {
            case 0:
                item.name = "Item_TripleBall";
                if (sr != null && B.Length > 11) sr.sprite = B[11];
                break;

            case 1:
                item.name = "Item_Big";
                if (sr != null && B.Length > 12) sr.sprite = B[12];
                break;

            case 2:
                item.name = "Item_Small";
                if (sr != null && B.Length > 13) sr.sprite = B[13];
                break;
        }

        if (rb != null)
            rb.AddForce(Vector2.down * 0.008f);

        Destroy(item, 7f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null)
            return;

        string itemName = other.gameObject.name;
        Destroy(other.gameObject);

        if (S_Eat != null)
            S_Eat.Play();

        switch (itemName)
        {
            case "Item_TripleBall":
                TripleBall();
                break;

            case "Item_Big":
                ApplyPaddleSize(2.42f, 1.963f);
                break;

            case "Item_Small":
                ApplyPaddleSize(0.82f, 2.521f);
                break;
        }
    }

    void TripleBall()
    {
        GameObject baseBall = GetActiveBall();
        if (baseBall == null)
            return;

        Vector3 basePos = baseBall.transform.position;

        for (int i = 0; i < Ball.Length; i++)
        {
            if (Ball[i] == null || i >= BallRg.Length || i >= BallTr.Length)
                continue;

            if (BallRg[i] == null || BallTr[i] == null)
                continue;

            if (Ball[i] == baseBall)
                continue;

            Ball[i].SetActive(true);
            BallTr[i].position = basePos;
            BallRg[i].linearVelocity = Vector2.zero;
            BallRg[i].angularVelocity = 0f;
            BallRg[i].AddForce(Random.insideUnitCircle.normalized * ballSpeed);
        }
    }

    GameObject GetActiveBall()
    {
        for (int i = 0; i < Ball.Length; i++)
        {
            if (Ball[i] != null && Ball[i].activeSelf)
                return Ball[i];
        }

        return null;
    }

    void ApplyPaddleSize(float size, float border)
    {
        if (paddleSizeCoroutine != null)
            StopCoroutine(paddleSizeCoroutine);

        paddleSizeCoroutine = StartCoroutine(PaddleSizeRoutine(size, border));
    }

    IEnumerator PaddleSizeRoutine(float size, float border)
    {
        SetPaddleSize(size, border);
        yield return new WaitForSeconds(7.5f);
        SetPaddleSize(1.58f, 2.262f);
        paddleSizeCoroutine = null;
    }

    void SetPaddleSize(float size, float border)
    {
        Vector3 scale = transform.localScale;
        scale.x = size;
        transform.localScale = scale;

        paddleSize = size;
        paddleBorder = border;
    }

    void GenerateStage()
    {
        if (stage < 1 || stage > StageStr.Length)
            return;

        string stageData = StageStr[stage - 1];

        for (int i = 0; i < BlockCol.Length; i++)
        {
            if (BlockCol[i] == null)
                continue;

            GameObject block = BlockCol[i].gameObject;

            if (i >= stageData.Length)
            {
                block.SetActive(false);
                continue;
            }

            char c = stageData[i];

            if (c == ' ')
            {
                block.SetActive(false);
                continue;
            }

            string blockName = "Block";
            int spriteIndex = 0;

            if (c == '8')
            {
                blockName = "HardBlock0";
                spriteIndex = 8;
            }
            else if (c == '9')
            {
                spriteIndex = Random.Range(0, 8);
            }
            else
            {
                spriteIndex = int.Parse(c.ToString());
            }

            block.name = blockName;

            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            if (sr != null && spriteIndex < B.Length)
                sr.sprite = B[spriteIndex];

            Animator ani = block.GetComponent<Animator>();
            if (ani != null)
                ani.Rebind();

            block.SetActive(true);
        }
    }

    void UpdateUI()
    {
        if (stageText != null)
            stageText.text = stage.ToString();

        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    void UpdateLifeUI()
    {
        if (Life_01 != null) Life_01.SetActive(life >= 1);
        if (Life_02 != null) Life_02.SetActive(life >= 2);
        if (Life_03 != null) Life_03.SetActive(life >= 3);
    }

    IEnumerator EndGame(bool isVictory)
    {
        isEnding = true;
        isStart = false;

        DisableExtraBalls();

        if (ItemsTr != null)
        {
            for (int i = ItemsTr.childCount - 1; i >= 0; i--)
                Destroy(ItemsTr.GetChild(i).gameObject);
        }

        if (isVictory)
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(true);

            if (S_Victory != null)
                S_Victory.Play();
        }
        else
        {
            if (GameOverPanel != null)
                GameOverPanel.SetActive(true);
        }

        yield return null;
    }
}