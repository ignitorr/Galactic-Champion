using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public enum GameState { MainMenu, Controls, Running, Over, Reset, Paused, Cutscene, Scores };

public class Game : MonoBehaviour
{

    GameState gameState;

    [SerializeField] GameObject camPositions;
    [SerializeField] GameObject pointsDisplay;
    [SerializeField] GameObject gameOverPoints;
    [SerializeField] GameObject countdown;
    [SerializeField] Text highscoreDisplay;
    [SerializeField] Camera cam;
    [SerializeField] float introLength = 2.0f;
    [SerializeField] AnimationCurve curve;

    [Header("Audio")]
    [SerializeField] AudioSource menu;
    [SerializeField] AudioSource running;
    [SerializeField] AudioSource shipSound;

    float gameSpeedMod;
    float asteroidSpawnMod;


    int points;
    float fpoints;

    List<int> highscores;


    private static Game gameInstance;

    public static Game game
    {
        get
        {
            if (!gameInstance)
            {
                gameInstance = FindObjectOfType(typeof(Game)) as Game;
                if (!gameInstance)
                    Debug.LogError("ERROR: No game script found!");
                else
                    gameInstance.Init();

            }
            return gameInstance;
        }
    }

    void Init()
    {
        gameState = GameState.MainMenu;
        gameSpeedMod = 1.0f;
        asteroidSpawnMod = 1.0f;

        highscores = new List<int>(5);
        LoadScores();
    }

    private void UpdateScores()
    {
        highscoreDisplay.text = PlayerPrefs.GetInt("First", 0) + "\n" +
                                PlayerPrefs.GetInt("Second", 0) + "\n" +
                                PlayerPrefs.GetInt("Third", 0) + "\n" +
                                PlayerPrefs.GetInt("Fourth", 0) + "\n" +
                                PlayerPrefs.GetInt("Fifth", 0);
    }

    private void LoadScores()
    {
        int score = PlayerPrefs.GetInt("First", 0);
        highscores.Add(score);
        score = PlayerPrefs.GetInt("Second", 0);
        highscores.Add(score);
        score = PlayerPrefs.GetInt("Third", 0);
        highscores.Add(score);
        score = PlayerPrefs.GetInt("Fourth", 0);
        highscores.Add(score);
        score = PlayerPrefs.GetInt("Fifth", 0);
        highscores.Add(score);

        UpdateScores();
    }

    private void AddScore(int newScore)
    {
        highscores.Add(newScore);
        highscores.Sort();
        highscores.Reverse();
        highscores.RemoveAt(5);

        PlayerPrefs.SetInt("First", highscores[0]);
        PlayerPrefs.SetInt("Second", highscores[1]);
        PlayerPrefs.SetInt("Third", highscores[2]);
        PlayerPrefs.SetInt("Fourth", highscores[3]);
        PlayerPrefs.SetInt("Fifth", highscores[4]);

        UpdateScores();
    }

    private void Update()
    {
        if (game.gameState == GameState.Running)
        {
            asteroidSpawnMod += Time.deltaTime * Time.deltaTime * gameSpeedMod;

            //fpoints += Time.deltaTime * gameSpeedMod * gameSpeedMod * 100.0f;

            points += (int)(Time.deltaTime * gameSpeedMod * gameSpeedMod * 100.0f);
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerShip pship = player.GetComponent<PlayerShip>();
        pointsDisplay.GetComponent<Text>().text = "Points: " + points +
                                                  "\nLives: " + pship.Lives;
    }

    IEnumerator FadeVolume(AudioSource a, float vol, float time)
    {
        vol = Mathf.Clamp(vol, 0, 1);
        float step = Mathf.Abs(a.volume - vol) * (1 / time);
        while (a.volume != vol)
        {
            a.volume = Mathf.MoveTowards(a.volume, vol, Time.deltaTime * step);
            yield return null;
        }
    }

    IEnumerator CrossFade(AudioSource a, float volA, float timeA, AudioSource b, float volB, float timeB)
    {
        volA = Mathf.Clamp(volA, 0, 1);
        volB = Mathf.Clamp(volB, 0, 1);

        float stepA = Mathf.Abs(a.volume - volA) * (1 / timeA);
        float stepB = Mathf.Abs(b.volume - volB) * (1 / timeB);
        if (volA > volB)
            a.Play();
        else
            b.Play();
        while (a.volume != volA || b.volume != volB)
        {
            a.volume = Mathf.MoveTowards(a.volume, volA, Time.deltaTime * stepA);
            b.volume = Mathf.MoveTowards(b.volume, volB, Time.deltaTime * stepB);
            yield return null;
        }
    }

    IEnumerator Cutscene(GameState a, GameState b, float length)
    {
        game.gameState = GameState.Cutscene;
        Transform start = camPositions.transform.Find(a.ToString());
        Transform end = camPositions.transform.Find(b.ToString());

        float lerp = 0;
        float cutsceneStart = Time.time;
        while (lerp < 1)
        {
            lerp = Mathf.Clamp((Time.time - cutsceneStart) / length, 0, 1);
            cam.transform.position =
                Vector3.Lerp(start.position, end.position, curve.Evaluate(lerp));
            cam.transform.rotation =
                Quaternion.Lerp(start.rotation, end.rotation, curve.Evaluate(lerp));
            yield return null;
        }
    }

    IEnumerator _StartGame()
    {
        game.points = 0;
        game.asteroidSpawnMod = 1.0f;
        game.fpoints = 0;
        game.gameSpeedMod = 1.0f;
        game.StartCoroutine(game.FadeVolume(game.shipSound, 0.2f, 3));
        game.StartCoroutine(game.CrossFade(running, 0.25f, 4, menu, 0, 3));
        yield return StartCoroutine(
            game.Cutscene(GameState.MainMenu, GameState.Running, game.introLength));
        print("Game started!");
        game.pointsDisplay.GetComponent<Text>().enabled = true;
        game.gameState = GameState.Running;
    }

    IEnumerator Countdown()
    {
        for (int i = (int)introLength; i >= 0; i--)
        {
            countdown.GetComponent<Text>().text = i + "...";
            if (i == 0)
            {
                countdown.GetComponent<Text>().text = "GO!";
                game.pointsDisplay.GetComponent<Text>().enabled = true;
                game.gameSpeedMod = 1.0f;
                game.gameState = GameState.Running;
            }
            yield return new WaitForSeconds(1);
        }
        countdown.GetComponent<Text>().text = "";
    }

    IEnumerator _Reset()
    {
        game.gameState = GameState.Reset;
        game.points = 0;
        game.asteroidSpawnMod = 1.0f;
        game.fpoints = 0;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerShip pship = player.GetComponent<PlayerShip>();
        pship.InitPlayer();
        var emm = player.GetComponent<ParticleSystem>().emission;
        //emm.enabled = true;
        Vector3 desired = Vector3.zero;
        desired.y = -2.5f;
        player.transform.parent.transform.position = new Vector3(0, -2.5f, -15);
        player.GetComponent<MeshRenderer>().enabled = true;
        Vector3 start = player.transform.parent.transform.position;
        float lerp = 0;
        float timeStarted = Time.time;
        //game.StartCoroutine(game.FadeMusic(0.25f, 5))
        game.StartCoroutine(game.FadeVolume(game.shipSound, 0.2f, 1));
        game.StartCoroutine(game.FadeVolume(game.running, 0.25f, 3));
        while (lerp < 1)
        {
            lerp = Mathf.Clamp((Time.time - timeStarted) / 2.0f, 0, 1);
            player.transform.parent.transform.position = Vector3.Lerp(start, desired, curve.Evaluate(lerp));
            yield return null;
        }
        yield return StartCoroutine(game.Countdown());
    }

    IEnumerator ShipLerp(bool resetPos)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerShip pship = player.GetComponent<PlayerShip>();
        pship.InitPlayer();
        var emm = player.GetComponent<ParticleSystem>().emission;
        Vector3 desired = Vector3.zero;
        desired.y = -2.5f;
        if (resetPos)
        {
            player.transform.parent.transform.position = new Vector3(0, -2.5f, -15);
            player.GetComponent<MeshRenderer>().enabled = true;
        }
        Vector3 start = player.transform.parent.transform.position;
        float lerp = 0;
        float timeStarted = Time.time;

        while (lerp < 1)
        {
            lerp = Mathf.Clamp((Time.time - timeStarted) / 2.0f, 0, 1);
            player.transform.parent.transform.position = Vector3.Lerp(start, desired, curve.Evaluate(lerp));
            yield return null;
        }
    }

    IEnumerator _ReturnToMenu(float mod)
    {
        game.points = 0;
        game.asteroidSpawnMod = 1.0f;
        game.fpoints = 0;


        if (game.gameState == GameState.Over)
        {
            //game.StartCoroutine(game.FadeMusic(0, 6));
            game.StartCoroutine(game.CrossFade(running, 0, 2, menu, 0.5f, 3));

            StartCoroutine(game.ShipLerp(true));
            yield return game.StartCoroutine(game.Cutscene(GameState.Running, GameState.MainMenu, game.introLength * mod));
        }
        else if (game.gameState == GameState.Controls)
        {
            StartCoroutine(game.ShipLerp(false));
            game.StartCoroutine(game.Cutscene(GameState.Controls, GameState.MainMenu, game.introLength * mod));
        }
        else
        {
            StartCoroutine(game.ShipLerp(false));
            game.StartCoroutine(game.Cutscene(GameState.Scores, GameState.MainMenu, game.introLength * mod));
        }
        game.gameState = GameState.MainMenu;
    }

    IEnumerator HTP()
    {
        StartCoroutine(game.Cutscene(GameState.MainMenu, GameState.Controls, game.introLength * 0.33f));
        game.gameState = GameState.Controls;
        yield return null;
    }

    IEnumerator HS()
    {
        StartCoroutine(game.Cutscene(GameState.MainMenu, GameState.Scores, game.introLength * 0.33f));
        game.gameState = GameState.Scores;
        yield return null;
    }

    public static void StartGame()
    {
        if (game.gameState == GameState.Running) return;
        game.StartCoroutine(game._StartGame());
        game.StartCoroutine(game.Countdown());
    }

    public static void GameOver()
    {
        game.gameState = GameState.Over;
        game.AddScore(game.points);
        game.gameSpeedMod = 0.0f;
        game.pointsDisplay.GetComponent<Text>().enabled = false;
        game.gameOverPoints.GetComponent<Text>().text = "Score\n" + game.points;
        game.StartCoroutine(game.FadeVolume(game.running, 0.1f, 0.75f));
    }

    public static void ResetGame()
    {
        print("Resetting game!");
        // clear asteroids, reset player & points
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        for (int i = 0; i < asteroids.Length; i++)
        {
            //Destroy(asteroids[i]);
            asteroids[i].GetComponent<Asteroid>().Vanish();
        }
        game.StartCoroutine(game._Reset());
    }

    public static void ReturnToMenu(float mod)
    {
        print("Returning to main menu!");
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        for (int i = 0; i < asteroids.Length; i++)
        {
            asteroids[i].GetComponent<Asteroid>().Vanish();
        }
        game.StartCoroutine(game._ReturnToMenu(mod));
    }

    public static void HowToPlay()
    {
        print("Showing htp");
        game.StartCoroutine(game.HTP());
    }

    public static void HighScores()
    {
        print("Showing high scores");
        game.StartCoroutine(game.HS());
    }

    public static float GameSpeed
    {
        get { return game.gameSpeedMod; }
        set { game.gameSpeedMod = value; }
    }

    public static float SpawnMod
    {
        get { return game.asteroidSpawnMod; }
    }

    public static GameState GameState
    {
        get { return game.gameState; }
    }
}
