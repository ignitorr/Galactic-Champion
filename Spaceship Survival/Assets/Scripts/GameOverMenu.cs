using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioSource enter;
    [SerializeField] Sprite active;
    [SerializeField] Sprite inactive;
    [SerializeField] Image retry;
    [SerializeField] Image exit;

    enum Selection { Retry = 0, Exit };

    Selection currentSelected = Selection.Retry;
    Selection lastSelected;

    CanvasGroup group;
    float lastInput = 0;
    float timeSinceLast = 0;

    bool canInteract = false;

    void Start()
    {
        group = GetComponent<CanvasGroup>();
        switch (currentSelected)
        {
            case (Selection.Retry):
                retry.sprite = active;
                break;
            case (Selection.Exit):
                exit.sprite = active;
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
            //Debug.Log("Current selection: " + currentSelected);

            retry.sprite = inactive;
            exit.sprite = inactive;

            switch (currentSelected)
            {
                case (Selection.Retry):
                    retry.sprite = active;
                    break;
                case (Selection.Exit):
                    exit.sprite = active;
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
                case (Selection.Retry):
                    Game.ResetGame();
                    break;
                case (Selection.Exit):
                    Game.ReturnToMenu(1.0f);
                    break;
            }
        }
    }

    void ResetSelection()
    {
        currentSelected = Selection.Retry;
        retry.sprite = active;
        exit.sprite = inactive;
    }

    IEnumerator FadeIn()
    {
        ResetSelection();
        while (group.alpha < 1)
        {
            group.alpha = Mathf.Clamp(group.alpha + (1.0f * Time.deltaTime), 0, 1);
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

    // Update is called once per frame
    void Update()
    {
        if (Game.GameState == GameState.Over)
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
                    StartCoroutine(FadeIn());
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
