using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoadMeshSectionType
{
    Road,
    Interesction
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class RoadMeshSection : MonoBehaviour
{
    public RoadMeshSectionType sectionType;

    public List<RoadMeshSection> linkedSections = new List<RoadMeshSection>(); 

    public List<Vector3> nodePositions = new List<Vector3>();
    public List<Quaternion> nodeRotation = new List<Quaternion>();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public float defaultWidth = 30f;
    public float defaultHeight = 15f;

    public Vector3[] vertices;
    public Vector3[] normals;
    public Vector2[] uv;
    public int[] triangles;
   

    public void BuildMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        int nodeLength = nodePositions.Count - 1;

        if (nodeLength <= 0) { return; }


        Mesh mesh = new Mesh();

        //Create 4 vertices for each node position, except the last one (as this controls the height).
        vertices = new Vector3[(4 * nodeLength)];

        normals = new Vector3[vertices.Length];

        uv = new Vector2[vertices.Length];

        triangles = new int[6 * nodeLength];

        //Add the vertices;
        int i;
        for (i = 0; i < nodeLength; i++)
        {
            Vector3 nodePosition = nodePositions[i];
            Vector3 nextNodePosition = nodePositions[i + 1];

            Quaternion thisRotation = nodeRotation[i];
            Quaternion nextNodeRotation = nodeRotation[i + 1];

            int offset = i * 4;


            Vector3 center = nodePosition;
            float _angle = thisRotation.eulerAngles.y;
            _angle = _angle - 90;
            _angle = _angle * Mathf.Deg2Rad;
            Vector3 rotation = new Vector3(Mathf.Sin(_angle), 0, Mathf.Cos(_angle)) * (defaultWidth / 2);

            vertices[offset + 0] = center + rotation;

            rotation = new Vector3(Mathf.Sin(_angle), 0, Mathf.Cos(_angle)) * (-defaultWidth / 2);
            vertices[offset + 1] = center + rotation;


            center = nextNodePosition;
            _angle = nextNodeRotation.eulerAngles.y;
            _angle = _angle - 90;
            _angle = _angle * Mathf.Deg2Rad;
            rotation = new Vector3(Mathf.Sin(_angle), 0, Mathf.Cos(_angle)) * (defaultWidth / 2);

            vertices[offset + 2] = center + rotation;

            rotation = new Vector3(Mathf.Sin(_angle), 0, Mathf.Cos(_angle)) * (-defaultWidth / 2);
            vertices[offset + 3] = center + rotation;
        }
        //Assign the vertices
        mesh.vertices = vertices;

        //Add the triangles
        for (i = 0; i < nodeLength; i++)
        {
            Vector3 nodePosition = nodePositions[i];

            int offset = i * 6;
            int tri = 4 * i;

            triangles[0 + offset] = 0 + tri;
            triangles[1 + offset] = 2 + tri;
            triangles[2 + offset] = 1 + tri;
            triangles[3 + offset] = 2 + tri;
            triangles[4 + offset] = 3 + tri;
            triangles[5 + offset] = 1 + tri;

        }
        //Add the triangles
        mesh.triangles = triangles;

        for (i = 0; i < nodeLength; i++)
        {
            normals[0 + (4 * i)] = -Vector3.forward;
            normals[1 + (4 * i)] = -Vector3.forward;
            normals[2 + (4 * i)] = -Vector3.forward;
            normals[3 + (4 * i)] = -Vector3.forward;
        }

        mesh.normals = normals;

        for (i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];

            int point = i % 4;
            float distance = 0;

            //If this 
            if (i > 1)
            {
                //get the vertex beneath me
                Vector3 lowerVertex = vertices[i - 2];
                distance = Vector3.Distance(vertex, lowerVertex) / defaultHeight;

            }


            if (point == 0)
            {
                uv[i] = new Vector2(0, distance);
            }
            if (point == 1)
            {
                uv[i] = new Vector2(1, distance);
            }
            if (point == 2)
            {
                uv[i] = new Vector2(0, distance);
            }
            if (point == 3)
            {
                uv[i] = new Vector2(1, distance);
            };
        }

        mesh.uv = uv;

        //Create the mesh
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

    }

}
