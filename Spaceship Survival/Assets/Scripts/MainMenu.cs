using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioSource enter;
    [SerializeField] Sprite active;
    [SerializeField] Sprite inactive;
    [SerializeField] Image start;
    [SerializeField] Image controls;
    [SerializeField] Image scores;
    [SerializeField] Image quit;

    enum Selection { Start = 0, Controls, Scores, Quit };
    Selection currentSelected = Selection.Start;
    Selection lastSelected;

    CanvasGroup group;
    float lastInput = 0;
    float timeSinceLast = 0;

    bool canInteract = true;

    void Start()
    {
        group = GetComponent<CanvasGroup>();
        switch (currentSelected)
        {
            case (Selection.Start):
                start.sprite = active;
                break;
            case (Selection.Controls):
                controls.sprite = active;
                break;
            case (Selection.Scores):
                scores.sprite = active;
                break;
            case (Selection.Quit):
                quit.sprite = active;
                break;
        }
    }

    void UpdateSelection()
    {
        float input = Mathf.Round(Input.GetAxis("HatVertical"));
        if ((Mathf.Abs(input) > 0 && lastInput != input) || (Mathf.Abs(input) > 0 && timeSinceLast > 0.35f))
        {
            source.Play();
            timeSinceLast = 0;
            int current = (int)currentSelected;
            current += (int)input;
            if (current >= System.Enum.GetNames(typeof(Selection)).Length)
                current = 0;
            if (current < 0)
                current = System.Enum.GetNames(typeof(Selection)).Length - 1;
            lastSelected = currentSelected;
            currentSelected = (Selection)current;

            start.sprite = inactive;
            controls.sprite = inactive;
            scores.sprite = inactive;
            quit.sprite = inactive;

            switch (currentSelected)
            {
                case (Selection.Start):
                    start.sprite = active;
                    break;
                case (Selection.Controls):
                    controls.sprite = active;
                    break;
                case (Selection.Scores):
                    scores.sprite = active;
                    break;
                case (Selection.Quit):
                    quit.sprite = active;
                    break;
            }

        }
        lastInput = input;
    }

    void ButtonPressed()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            enter.Play();
            switch (currentSelected)
            {
                case (Selection.Start):
                    Game.StartGame();
                    break;
                case (Selection.Controls):
                    Game.HowToPlay();
                    break;
                case (Selection.Scores):
                    Game.HighScores();
                    break;
                case (Selection.Quit):
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }
    }

    void ResetSelection()
    {
        currentSelected = Selection.Start;
        start.sprite = active;
        controls.sprite = inactive;
        scores.sprite = inactive;
        quit.sprite = inactive;
    }

    IEnumerator FadeIn()
    {
        ResetSelection();
        while (group.alpha < 1)
        {
            group.alpha = Mathf.Clamp(group.alpha + (1.5f * Time.deltaTime), 0, 1);
            yield return null;
        }
        canInteract = true;
    }

    IEnumerator FadeOut()
    {
        canInteract = false;
        while (group.alpha > 0)
        {
            group.alpha = Mathf.Clamp(group.alpha - (1.5f * Time.deltaTime), 0, 1);
            yield return null;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioListener.volume = AudioListener.volume == 0 ? 1 : 0;
        }

        if (Game.GameState == GameState.MainMenu)
        {
            if (canInteract)
            {
                timeSinceLast += Time.deltaTime;
                UpdateSelection();
                ButtonPressed();
            }
            else
            {
                if (group.alpha == 0)
                {
                    StartCoroutine(FadeIn());
                }
            }
        }
        else
        {
            if (canInteract)
            {
                StartCoroutine(FadeOut());
            }
        }
    }
}
