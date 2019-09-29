using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] Vector3 home = new Vector3(0, 0, -10);
    public float maxYaw = 20.0f;
    public float maxRoll = 20.0f;
    public float maxPitch = 20.0f;
    [SerializeField] float maxOffset = 1.0f;

    float trauma = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (Game.GameState == GameState.Running || Game.GameState == GameState.Over)
        {
            float shake = trauma * trauma;

            float yaw = maxYaw * shake * Random.Range(-1.0f, 1.0f);
            float roll = maxRoll * shake * Random.Range(-1.0f, 1.0f);
            float pitch = maxPitch * shake * Random.Range(-1.0f, 1.0f);
            float offX = maxOffset * shake * Random.Range(-1.0f, 1.0f);
            float offY = maxOffset * shake * Random.Range(-1.0f, 1.0f);
            float offZ = maxOffset * shake * Random.Range(-1.0f, 1.0f);

            gameObject.transform.rotation = Quaternion.Euler(pitch, yaw, roll);

            gameObject.transform.position = home + new Vector3(offX, offY, offZ);

            if (trauma > 0)
            {
                trauma -= Time.deltaTime;
                Mathf.Clamp(trauma, 0, 1);
            }
        }
    }

    public void AddTrauma(float amount)
    {
        trauma += amount;
        Mathf.Clamp(trauma, 0, 1);
    }

    public float GetTrauma
    {
        get { return trauma; }
    }
}
