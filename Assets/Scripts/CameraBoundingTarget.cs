using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundingTarget : MonoBehaviour
{
    public GameObject target;
    public Material debugMat;

    private MeshFilter filter;
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        filter = target.GetComponent<MeshFilter>();
    }

    public Bounds CreateTargetBounds()
    {
        Vector3 positionView = cam.WorldToViewportPoint(target.transform.position);

        Bounds bounds = new Bounds(new Vector3(positionView.x, positionView.y, 0), Vector3.zero);

        Matrix4x4 localToWorld = target.transform.localToWorldMatrix;

        Vector3[] vertices = filter.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {          
            Vector3 point = cam.WorldToViewportPoint(localToWorld.MultiplyPoint3x4(vertices[i]));
            bounds.Encapsulate(new Vector3(point.x, point.y, 0));
        }

        return bounds;
    }

    // Draws a line from "startVertex" var to the curent mouse position.

    void OnPostRender()
    {
        if (!debugMat)
        {
            Debug.LogWarning("Please Assign a material on the inspector");
            return;
        }

        Bounds bounds = CreateTargetBounds();

        Vector3 bottomLeft = bounds.min;
        Vector3 upperLeft = new Vector3(bounds.min.x, bounds.max.y, 0);
        Vector3 upperRight = bounds.max;
        Vector3 bottomRight = new Vector3(bounds.max.x, bounds.min.y, 0);

        GL.PushMatrix();
        debugMat.SetPass(0);
        GL.LoadOrtho();

        GL.Begin(GL.LINES);
        GL.Color(Color.magenta);

        //Left side
        GL.Vertex(bottomLeft);
        GL.Vertex(upperLeft);

        //Upper side
        GL.Vertex(upperLeft);
        GL.Vertex(upperRight);

        //Right side
        GL.Vertex(upperRight);
        GL.Vertex(bottomRight);

        //Bottom side
        GL.Vertex(bottomRight);
        GL.Vertex(bottomLeft);

        GL.End();

        GL.PopMatrix();
    }
}
