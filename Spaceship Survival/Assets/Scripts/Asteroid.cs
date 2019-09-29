using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public float speed = 0;
    public Vector3 dir = Vector3.zero;

    private bool triggered;
    private void Start()
    {
        triggered = false;
    }

    void Update()
    {
        Transform t = gameObject.transform;

        t.position += (dir * speed * Time.deltaTime * Game.GameSpeed);

        if (t.position.z < 0)
        {
            float alpha = Mathf.Clamp(t.position.z / -5.0f, 0, 1);
            Material mat = gameObject.GetComponent<Renderer>().material;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1.0f - alpha);

            if (alpha == 1.0)
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        PlayerShip player = other.gameObject.GetComponent<PlayerShip>();

        if (player)
        {
            triggered = true;
            player.TakeDamage();
        }
    }

    IEnumerator Fade()
    {
        float timeStarted = Time.time;
        Material mat = gameObject.GetComponent<Renderer>().material;
        float startAlpha = mat.color.a;
        while (mat.color.a > 0)
        {
            float lerp = Mathf.Clamp((Time.time - timeStarted) / 0.5f, 0, 1);
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, Mathf.Lerp(startAlpha, 0, lerp));
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Vanish()
    {
        StartCoroutine(Fade());
    }
}
