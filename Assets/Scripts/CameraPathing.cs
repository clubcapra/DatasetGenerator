using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPathing : MonoBehaviour
{
    [Header("Radius")]
    public float maxRadius = 1f;
    public float minRadius = 0.1f;
    public int stepRadius = 10;

    [Header("Distance")]
    public float maxDistance = 15f;
    public float minDistance = 10f;
    public int stepDistance = 10;

    [Header("Offset")]
    public float maxOffsetRadius = 1f;
    public int stepOffset = 10;

    [Header("Random")]
    public float random = 0.05f;
    public int seed = 12345;

    public List<Path> GetPaths(Vector3 position)
    {
        Random.InitState(seed);

        float distanceIncrement = (maxDistance - minDistance) / stepDistance;
        float radiusIncrement = (maxRadius - minRadius) / stepRadius;

        float radRadiusIncrement = Mathf.PI / stepRadius;

        List<Path> paths = new List<Path>();

        for (int i = 0; i < stepDistance; i++)
        {
            Vector3 disPos = position;
            disPos.z = -(Random.Range(-random, random) + minDistance + distanceIncrement * i);

            for (int j = 0; j < stepOffset; j++)
            {
                Vector3 offPos = position;
                offPos.y += Random.Range(-random, random) + Random.Range(-maxOffsetRadius, maxOffsetRadius);
                offPos.x += Random.Range(-random, random) + Random.Range(-maxOffsetRadius, maxOffsetRadius);

                for (int k = 0; k < stepRadius; k++)
                {
                    for (int kk = 0; kk < stepRadius; kk++)
                    {
                        Vector3 radPos = disPos;
                        radPos.y += Random.Range(-random, random) + Mathf.Sin(radRadiusIncrement * k) * radiusIncrement * kk;
                        radPos.x += Random.Range(-random, random) + Mathf.Cos(radRadiusIncrement * k) * radiusIncrement * kk;

                        paths.Add(new Path(radPos, offPos));
                    }
                }
            }
        }

        return paths;
    }   
}

public struct Path
{
    public Vector3 position;
    public Vector3 target;

    public Path(Vector3 position, Vector3 target)
    {
        this.position = position;
        this.target = target;
    }
}
