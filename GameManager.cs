using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Bate bateScript;
    public GameObject ball_prefab;
    public GameObject target_prefab;
    public AudioClip bate_golpea, pop;
    public Canvas main_canvas;
    public Transform bolas_respawn;
    public Transform level_transform_parent;
    public Transform bate;
    public Text text_points, text_combo, text_maxscore;
    Vector3 batePosition = new Vector3(0, -4, 0);
    AudioSource audioS;
    AudioHighPassFilter audioFilterHigh;
    public bool mode2H;
    public bool gameIsStop = false;
    bool gameOver = false;
    bool levelIsCompleted = false;
    int levelN = 0;
    float cd, lastTime;
    public float startTimeScale;
    GameObject menu_gameobject;
    [HideInInspector]
    public int aliveTargets_OnStrike, comboTotal = 1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        menu_gameobject = main_canvas.transform.Find("Menu").gameObject;
        audioS = GetComponent<AudioSource>();
        audioFilterHigh = GetComponent<AudioHighPassFilter>();
        Invoke("GenerateRandomLevel", 0.01f);
        CheckPlayerPrefs();
        Time.timeScale = startTimeScale;
    }

    private void Update()
    {
        if (Time.time > lastTime + cd && !gameIsStop && !gameOver && !levelIsCompleted) EscupeBolas();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (main_canvas.transform.Find("Menu").gameObject.activeSelf) Application.Quit();
            else AbrirMenu();
        }
        
        if (Input.GetKeyDown(KeyCode.Mouse0) && !menu_gameobject.activeSelf)
        {
            if (levelIsCompleted) GenerateRandomLevel();
            if (gameOver) Retry(); 
        }
    }

    private void CheckPlayerPrefs()
    {
        SetTextMaxScore();
        if (PlayerPrefs.HasKey("Mode") && PlayerPrefs.GetString("Mode") == "2H") mode2H = true;
        if (!PlayerPrefs.HasKey("Coffee")) PlayerPrefs.SetInt("Coffee", 0);
    }

    private void SetTextMaxScore()
    {
        int actualScore = GetMyActualScore();
        if (PlayerPrefs.HasKey("Score")) {
            int maxScore = PlayerPrefs.GetInt("Score");
            if (maxScore < actualScore)
            {
                text_maxscore.text = actualScore.ToString();
            }
            else
            {
                text_maxscore.text = maxScore.ToString();
            }
        } 
        else text_maxscore.text = "0";
    }

    private void GenerateRandomLevel()
    {
        levelIsCompleted = gameOver = gameIsStop = false;
        var d = Mathf.Clamp(levelN * 0.017f, 0f, 1.0f);
        int targetsN = Mathf.Clamp(3 + levelN / 8, 3, 20);
        GameObject[] targets_list = new GameObject[targetsN];
        for(var x = 0; x < targetsN; x++)
        {
            targets_list[x] = Instantiate(target_prefab, new Vector3(Random.Range(-1.9f - d, 1.9f + d), Random.Range(1f - d, 4f + d), 0f), transform.rotation);
        }
        foreach (GameObject go in targets_list) go.transform.parent = level_transform_parent;
        main_canvas.transform.Find("Congrats").gameObject.SetActive(false);
        bateScript.lastTime_moved = Time.time - bateScript.repeatCD / 3;
    }

    private void EscupeBolas()
    {
        var x = Random.Range(0, 2) > 0 ? 1 : -1;
        var go = Instantiate(ball_prefab, new Vector3(Random.Range(0, 2) > 0 ? x : 0, 7, 0), transform.rotation, bolas_respawn);

        cd = 1.6f - Mathf.Clamp(levelN * 0.008f, 0f, 1.0f);
        Vector2 vDir = batePosition - go.transform.position;
        go.GetComponent<Rigidbody2D>().velocity = vDir.normalized * (Random.Range(8, 11) + Mathf.Clamp(levelN / 6, 0, 14));
        lastTime = Time.time;
    }

    private void Retry()
    {
        levelN = 0;
        SetComboAmount(0);
        text_points.text = "0";
        foreach (Transform t in level_transform_parent) Destroy(t.gameObject);
        main_canvas.transform.Find("GameOver").gameObject.SetActive(false);
        GenerateRandomLevel();
    }

    private void CheckTargets()
    {
        if (GetAliveTargets() == 0) LevelCompleted();
    }

    private int GetAliveTargets()
    {
        return level_transform_parent.childCount;
    }

    private void AbrirMenu()
    {
        main_canvas.transform.Find("Menu").gameObject.SetActive(true);
        gameIsStop = true;
        SetTextMaxScore();
        Time.timeScale = 0f;
    }

    private void SumarPuntos(int points)
    {
        text_points.text = (int.Parse(text_points.text) + points).ToString();
    }

    private void LevelCompleted()
    {
        gameIsStop = true;
        levelIsCompleted = true;
        main_canvas.transform.Find("Congrats").gameObject.SetActive(true);
        levelN++;
        LimpiarBolas();
    }

    public void SelectMode1H()
    {
        CerrarMenu();
        bate.transform.position = new Vector3(0, -5, 0);
        mode2H = false;
        PlayerPrefs.SetString("Mode", "1H");
    }

    public void SelectMode2H()
    {
        CerrarMenu();
        mode2H = true;
        PlayerPrefs.SetString("Mode", "2H");
    }

    public void CerrarMenu()
    {
        main_canvas.transform.Find("Menu").gameObject.SetActive(false);
        gameIsStop = false;
        Time.timeScale = 1f;
    }

    public void SoundPop()
    {
        audioS.PlayOneShot(pop);
    }

    public void PlayerFailure()
    {
        main_canvas.transform.Find("GameOver").gameObject.SetActive(true);
        LimpiarBolas();
        gameOver = true;
        int score = GetMyActualScore();
        if (PlayerPrefs.GetInt("Score") < score) PlayerPrefs.SetInt("Score", score);
    }

    public int GetMyActualScore()
    {
        return int.Parse(text_points.text);
    }

    public void LimpiarBolas()
    {
        foreach (Transform t in bolas_respawn) Destroy(t.gameObject);
    }

    public void TargetHit()
    {
        SoundPop();
        Invoke("CheckTargets", 0.1f);
        SetComboAmount(++comboTotal);
        SumarPuntos(10 * comboTotal);
    }

    public void BallOut()
    {
        if (GetAliveTargets() == aliveTargets_OnStrike) SetComboAmount(0);
    }

    public void SetComboAmount(int amount)
    {
        comboTotal = amount;
        text_combo.text = amount <= 1 ? "" : "x" + amount.ToString();
    }

    public void BallStrike(float force)
    {
        Vibration.Vibrate(13 + (long)force);
        audioFilterHigh.cutoffFrequency = 1500 - force * 28;
        audioS.PlayOneShot(bate_golpea);
        aliveTargets_OnStrike = GetAliveTargets();
    }


}
