using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathMeasure : MonoBehaviour
{
    // Start is called before the first frame update
    private Seeker seeker;
    private GameObject player;
    private float distance;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        player = GetComponentInParent<Enemy>().player;
        StartCoroutine(updateDistance());
    }

    // Update is called once per frame
    void Update()
    {

        
        

    }

    private IEnumerator updateDistance() {
        while (true) {
            var path = seeker.StartPath(transform.position, player.transform.position, OnPathComplete);
            yield return path.WaitForPath();
        }

    }
    void OnPathComplete(Path path)
    {
        if (path.error)
        {
            distance = float.PositiveInfinity;
        }
        distance = GetPathLength(path);
    }

    float GetPathLength(Path path)
    {
        float totalLength = 0;

        for (int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            totalLength += Vector3.Distance(path.vectorPath[i], path.vectorPath[i + 1]);
        }

        return totalLength;
    }
    public float getDistance() { return distance; }
}
