using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] float asteroidSpeed;
    [SerializeField] float randOffset; // random offset for asteroid speed
    [SerializeField] float asteroidLife;
    [SerializeField] float spawnTime;
    [SerializeField] GameObject asteroid;
    [SerializeField] float spawnRange = 3.0f;

    float timeSinceLastSpawn = 0.0f;
    int frames = 0;

    void SpawnAsteroid()
    {
        Vector3 pos1 = Vector3.zero;
        Vector3 pos2 = Vector3.zero;
        Vector3 center = Vector3.zero;
        float dist = 0;

        pos1 = Random.insideUnitCircle.normalized * spawnRange;
        pos2 = Random.insideUnitCircle.normalized * spawnRange;
        pos1.z = gameObject.transform.position.z;
        pos2.z = gameObject.transform.position.z;


        dist = Mathf.Sqrt(Mathf.Pow(pos2.x - pos1.x, 2) + Mathf.Pow(pos2.y - pos1.y, 2));
        if (dist < 4.0f)
        {
            pos2 = new Vector3(-pos2.x, -pos2.y, pos2.z);
            dist = Mathf.Sqrt(Mathf.Pow(pos2.x - pos1.x, 2) + Mathf.Pow(pos2.y - pos1.y, 2));
        }
        center = (pos1 + pos2) / 2;
      

        GameObject newAsteroid = Instantiate(asteroid);
        newAsteroid.transform.position = center;
        newAsteroid.transform.rotation = Quaternion.LookRotation(pos1 - center, Vector3.up);
        newAsteroid.transform.localScale = new Vector3(1, 1, dist);

        Asteroid script = newAsteroid.GetComponent<Asteroid>();
        script.speed = asteroidSpeed;
        script.dir = new Vector3(0, 0, -1);
        script.enabled = true;
    }

    void Update()
    {
        if (Game.GameState == GameState.Running)
        {
            timeSinceLastSpawn += Time.deltaTime * Game.GameSpeed * Game.SpawnMod;
            frames++;
            if (timeSinceLastSpawn >= spawnTime)
            {
                SpawnAsteroid();
                //Debug.Log("Spawned: " + frames + " passed");
                frames = 0;
                timeSinceLastSpawn = 0;
            }
        }
    }
}
