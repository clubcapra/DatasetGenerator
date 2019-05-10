using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(CameraPathing), typeof(Camera))]
public class DatasetGenerator : MonoBehaviour
{
    public GameObject label;
    public DatasetSettings dataset;
    public RenderTexture renderTexture;
    public Material labelMat;
    public Material debugMat;

    private MeshFilter targetMesh;
    private CameraPathing cameraPathing;
    private Camera mainCamera;

    private Coroutine simulation;

    private void Start()
    {
        targetMesh = label.GetComponent<MeshFilter>();
        cameraPathing = GetComponent<CameraPathing>();
        mainCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && simulation != null)
        {
            StopCoroutine(simulation);
        }

        if (Input.GetKeyDown(KeyCode.Space) && simulation == null)
        {
            CreateDataset();
        }
    }

    public void CreateDataset()
    {
        simulation = StartCoroutine(StartSimulation());
    }

    private void MoveCamera(Path path)
    {
        transform.position = path.position;
        Vector3 targetPos = path.target - transform.position;
        transform.rotation = Quaternion.LookRotation(targetPos, Vector3.up);
    }

    IEnumerator StartSimulation()
    {
        List<Path> paths = cameraPathing.GetPaths(label.transform.position);

        string rootPath = GetFolderPath(); 
        foreach (var datClass in dataset.classes)
        {
            int count = 1;
            string folderPath = rootPath + datClass.name + "/";
            Directory.CreateDirectory(folderPath);
            labelMat.mainTexture = datClass.texture;
            foreach (var path in paths)
            {
                MoveCamera(path);
                yield return new WaitForEndOfFrame();
                string fileName = datClass.name + count;
                TakeAndLabelPicture(datClass.objectClass, fileName, folderPath);
                count++;
            }
        }
    }

    private void TakeAndLabelPicture(int objectClass, string file, string folder)
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        mainCamera.targetTexture = renderTexture;

        //Need the RenderTexture first or the viewport will be wrong
        Bounds bounds = CreateTargetBounds();

        mainCamera.Render();

        Texture2D Image = new Texture2D(renderTexture.width, renderTexture.height);
        Image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
        mainCamera.targetTexture = null;

        var Bytes = Image.EncodeToPNG();
        Destroy(Image);

        File.WriteAllBytes(folder + file + ".png", Bytes);
        //If you have question on the format https://github.com/AlexeyAB/Yolo_mark/issues/60
        File.WriteAllText(folder + file + ".txt", string.Format("{0} {1} {2} {3} {4}", objectClass, bounds.center.x, bounds.center.y, bounds.extents.x, bounds.extents.y));
    }

    private string GetFolderPath()
    {
        return Application.dataPath + "/../Dataset/" + DateTime.Now.ToFileTime() + "/";
    }

    private Bounds CreateTargetBounds()
    {
        Vector3 positionView = mainCamera.WorldToViewportPoint(label.transform.position);

        Bounds bounds = new Bounds(new Vector3(positionView.x, positionView.y, 0), Vector3.zero);

        Matrix4x4 localToWorld = targetMesh.transform.localToWorldMatrix;

        Vector3[] vertices = targetMesh.mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 point = mainCamera.WorldToViewportPoint(localToWorld.MultiplyPoint3x4(vertices[i]));
            bounds.Encapsulate(new Vector3(point.x, point.y, 0));
        }

        return bounds;
    }

    // Draws a line from "startVertex" var to the curent mouse position.

    void OnPostRender()
    {
        if (simulation != null)
        {
            return;
        }

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
