using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTPMenu : MonoBehaviour
{
    [SerializeField] GameState activeState;
    [SerializeField] AudioSource enter;

    CanvasGroup group;

    bool canInteract = false;

    void Start()
    {
        group = GetComponent<CanvasGroup>();
    }

    IEnumerator FadeIn()
    {
        while (group.alpha < 1)
        {
            group.alpha = Mathf.Clamp(group.alpha + Time.deltaTime, 0, 1);
            yield return null;
        }
        canInteract = true;
    }

    IEnumerator FadeOut()
    {
        canInteract = false;
        while (group.alpha > 0)
        {
            group.alpha = Mathf.Clamp(group.alpha - (2.0f * Time.deltaTime), 0, 1);
            yield return null;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Game.GameState == activeState)
        {
            if (canInteract)
            {
                if (Input.GetKeyDown(KeyCode.JoystickButton1))
                {
                    enter.Play();
                    Game.ReturnToMenu(0.33f);
                }
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
