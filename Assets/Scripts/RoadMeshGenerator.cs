using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(RoadMeshGenerator))]
public class RoadMeshGeneratorEditor : Editor
{
    private RoadMeshGenerator instance;

    private void Awake()
    {
        instance = (RoadMeshGenerator)target;
    }

    Vector2 toolbarPosition;
    private void OnSceneGUI()
    {
        //Create a button at the transform;
        Handles.BeginGUI();
       
        Vector2 position = HandleUtility.WorldToGUIPoint(instance.transform.position);
        Vector3 forward = Vector3.forward;
        Vector3 offset = Vector3.right;
        Vector3 nodePosition = new Vector3();

        if (selectedSection != null)
        {
            nodePosition = selectedSection.nodePositions[selectedNode] + instance.transform.position;
            position = HandleUtility.WorldToGUIPoint(nodePosition);
            forward = selectedSection.nodeRotation[selectedNode] * Vector3.forward;
        }

        if (selectedSection != null && selectedSection.sectionType == RoadMeshSectionType.Interesction)
        {
            offset = selectedSection.nodeRotation[selectedNode] * Vector3.right;
            offset = offset * -15;
            forward = forward * -15;
            position = HandleUtility.WorldToGUIPoint(nodePosition + offset + forward);

            if (GUI.Button(new Rect(position, new Vector2(150, 20)), "New Road"))
            {
                Quaternion rotation = selectedSection.nodeRotation[selectedNode];

                Vector3 eulers = rotation.eulerAngles;
                //Rotate it to face away
                rotation = Quaternion.Euler(eulers.x, eulers.y - 90, eulers.z);

                //This should come with the way to spawn roads.
                CreateLinkedRoadSection(nodePosition + offset + forward, rotation);

            }

            offset = selectedSection.nodeRotation[selectedNode] * Vector3.right;
            offset = offset * 15;
            forward = selectedSection.nodeRotation[selectedNode] * Vector3.forward;
            forward = forward * -15;
            position = HandleUtility.WorldToGUIPoint(nodePosition + offset + forward);

            //position = HandleUtility.WorldToGUIPoint(nodePosition + new Vector3(15, 0, -15));

            if (GUI.Button(new Rect(position, new Vector2(150, 20)), "New Road"))
            {
                Quaternion rotation = selectedSection.nodeRotation[selectedNode];

                Vector3 eulers = rotation.eulerAngles;
                //Rotate it to face away
                rotation = Quaternion.Euler(eulers.x, eulers.y + 90, eulers.z);

                //This should come with the way to spawn roads.
                CreateLinkedRoadSection(nodePosition + offset + forward, rotation);
            }

            position = HandleUtility.WorldToGUIPoint(nodePosition + new Vector3(0, 0, 0));

            if (GUI.Button(new Rect(position, new Vector2(150, 20)), "New Road"))
            {
                Quaternion rotation = selectedSection.nodeRotation[selectedNode];

                //This should come with the way to spawn roads.
                CreateLinkedRoadSection(nodePosition, rotation);
            }
        }
        else
        {
            position.y += 20;

            if (GUI.Button(new Rect(position, new Vector2(150, 20)), "Add Node"))
            {
                AddNodeToCurrentSection();
            }

            position.y += 20;

            if (GUI.Button(new Rect(position, new Vector2(150, 20)), "Create New Road"))
            {
                CreateNewRoadSection();
            }

            position.y += 20;
            if (GUI.Button(new Rect(position, new Vector2(150, 20)), "Create Intersection"))
            {
                //This should come with the way to spawn roads.
                CreateIntersection();
            }
        }




        Handles.EndGUI();

        //Draw handle for each of the sections
        if (instance.roadMeshSections == null)
        {
            instance.roadMeshSections = new List<RoadMeshSection>();
        }

        int sections = instance.roadMeshSections.Count;
        for (int i = 0; i < sections; i++)
        {

            DrawSectionHandles(i);
            
        }

    }


    private void CreateNewRoadSection()
    {
        //Create a new road section at the current point;
        GameObject obj = new GameObject("Road Section", typeof(RoadMeshSection));
        obj.transform.SetParent(instance.transform, false);

        RoadMeshSection section = obj.GetComponent<RoadMeshSection>();
        section.sectionType = RoadMeshSectionType.Road;
        instance.roadMeshSections.Add(section);
        

        //Get the last node position
        Vector3 lastPos = new Vector3(instance.transform.position.x, instance.transform.position.y);
        Quaternion lastRotation = Quaternion.identity;

        if (section.nodePositions.Count == 0)
        {
            section.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z));
            section.nodeRotation.Add(lastRotation);

            section.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z + section.defaultHeight));
            section.nodeRotation.Add(lastRotation);

            section.BuildMesh();
        }

        obj.GetComponent<MeshRenderer>().material = GetMaterial("Road");
    }

    private void CreateLinkedRoadSection(Vector3 position, Quaternion rotation)
    {
        //Create a new road section at the current point;
        GameObject obj = new GameObject("Road Section", typeof(RoadMeshSection));
        obj.transform.SetParent(instance.transform, false);

        RoadMeshSection section = obj.GetComponent<RoadMeshSection>();
        section.sectionType = RoadMeshSectionType.Road;
        instance.roadMeshSections.Add(section);

        Vector3 lastPos = position;
        Quaternion lastRotation = rotation;
        
        if (section.nodePositions.Count == 0)
        {
            section.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z));
            section.nodeRotation.Add(lastRotation);

            Vector3 forward = lastRotation * Vector3.forward;
            forward = forward * section.defaultHeight;

            section.nodePositions.Add(section.nodePositions[section.nodePositions.Count - 1] + forward);

            section.nodeRotation.Add(lastRotation);

            section.BuildMesh();
        }

        obj.GetComponent<MeshRenderer>().material = GetMaterial("Road");

    }

    private void CreateIntersection()
    {
        //Create a new road section at the current point;
        GameObject obj = new GameObject("Intersection", typeof(RoadMeshSection));
        obj.transform.SetParent(instance.transform, false);

        RoadMeshSection section = obj.GetComponent<RoadMeshSection>();
        section.sectionType = RoadMeshSectionType.Interesction;
        instance.roadMeshSections.Add(section);

        section.defaultHeight = selectedSection.defaultWidth;
        
        //Get the last node position from the selected node
        Vector3 lastPos = new Vector3(instance.transform.position.x, instance.transform.position.y);
        Quaternion lastRotation = Quaternion.identity;

        if (selectedSection != null)
        {
            lastPos = selectedSection.nodePositions[selectedSection.nodePositions.Count - 1];
            lastRotation = selectedSection.nodeRotation[selectedSection.nodeRotation.Count - 1];
        }

        section.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z));
        section.nodeRotation.Add(lastRotation);

        Vector3 forward = lastRotation * Vector3.forward;
        forward = forward * section.defaultHeight;

        section.nodePositions.Add(section.nodePositions[section.nodePositions.Count - 1] + forward);

        //section.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z + section.defaultHeight));
        section.nodeRotation.Add(lastRotation);

        section.BuildMesh();

        section.linkedSections.Add(selectedSection);

        obj.GetComponent<MeshRenderer>().material = GetMaterial("Intersection");
    }

    private void AddNodeToCurrentSection()
    {
        Vector3 lastPos = selectedSection.nodePositions[selectedSection.nodePositions.Count - 1];
        Quaternion lastRotation = selectedSection.nodeRotation[selectedSection.nodePositions.Count - 1];

        //Calculate the position

        Vector3 forward = lastRotation * Vector3.forward;
        forward = forward * selectedSection.defaultHeight;

        selectedSection.nodePositions.Add(selectedSection.nodePositions[selectedSection.nodePositions.Count - 1] + forward);


        selectedSection.nodeRotation.Add(lastRotation);

        selectedSection.BuildMesh();

        selectedNode = (selectedSection.nodePositions.Count - 1);

    }

    RoadMeshSection selectedSection;
    int selectedNode;

    private void DrawSectionHandles(int index)
    {
        RoadMeshSection section = instance.roadMeshSections[index];

        Vector3 lastPos = new Vector3();

        Vector3 position;
        Quaternion rotation;

        if (section.sectionType == RoadMeshSectionType.Road) {
            for (int i = 0; i < section.nodePositions.Count; i++)
            {
                Handles.color = Color.white * 0.5f;


                //Draw a movement handle if this is a road 

                if (section.sectionType == RoadMeshSectionType.Road)
                {

                    EditorGUI.BeginChangeCheck();


                    rotation = Handles.RotationHandle(section.nodeRotation[i], section.transform.position + section.nodePositions[i]);

                    position = Handles.PositionHandle(section.transform.position + section.nodePositions[i], rotation);
                    

                   
                    if (i > 0)
                    {
                        Handles.color = Color.blue;
                        Handles.DrawLine(lastPos, position);
                    }
                    lastPos = position;

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(instance, "Change Road");
                        position -= section.transform.position;
                        section.nodePositions[i] = position;
                        section.nodeRotation[i] = rotation;
                        section.BuildMesh();

                        selectedSection = section;
                        selectedNode = i;
                    }

                   
                }
            }
        }
        else if(section.sectionType == RoadMeshSectionType.Interesction)
            {
                // Create four points
                for(int sides=0; sides<4; sides++)
                {
                    float point = (section.defaultWidth / 2);

                    Vector3 nodePosition = section.transform.position + section.nodePositions[0];
                    Vector3 offset = new Vector3();

                    if (sides == 0)
                    {
                        offset = new Vector3(0, 0, 0);
                    }
                    if (sides == 1)
                    {
                        offset = new Vector3(-point, 0, point);
                    }
                    if (sides == 2)
                    {
                        offset = new Vector3(0, 0, point*2);
                    }
                    if (sides == 3)
                    {
                        offset = new Vector3(point, 0, point);
                    }

                    rotation = section.nodeRotation[0];

                Handles.CubeHandleCap(0, nodePosition + offset, rotation, 1f, EventType.Repaint);
                }

            if (section.linkedSections.Count > 0)
            {
                RoadMeshSection linkedSection = section.linkedSections[0];
                section.nodePositions[0] = linkedSection.nodePositions[linkedSection.nodePositions.Count - 1];
                section.nodeRotation[0] = linkedSection.nodeRotation[linkedSection.nodePositions.Count - 1];
                section.BuildMesh();
            }

            //Check if there's another linked section




            EditorGUI.BeginChangeCheck();

            //Create one for the other node
            rotation = Handles.RotationHandle(section.nodeRotation[1], section.transform.position + section.nodePositions[1]);

            position = Handles.PositionHandle(section.transform.position + section.nodePositions[1], rotation);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(instance, "Change Road");
                position -= section.transform.position;
                section.nodePositions[1] = position;
                section.nodeRotation[1] = rotation;
                section.BuildMesh();

                selectedSection = section;
                selectedNode = 1;
            }




        }

        if (section.vertices != null)
        {
            //Make a label at each vertice
            for (int i = 0; i < section.vertices.Length; i++)
            {
                Vector3 vertex = section.vertices[i];
                Handles.RectangleHandleCap(i, instance.transform.position + vertex, instance.transform.rotation, 0.1f, EventType.Repaint);
                Handles.Label(instance.transform.position + vertex, string.Format("{0}:{1}", i, vertex.ToString()));
            }
        }


    }
    //

    public Material GetMaterial(string materialName)
    {
        return Resources.Load<Material>(materialName);
    }
}

    /*
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Add"))
        {
            //Get the last node position
            Vector3 lastPos = new Vector3(instance.transform.position.x, instance.transform.position.y);
            Quaternion lastRotation = Quaternion.identity;
            
            if (instance.nodePositions.Count == 0) {
                instance.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z));
                instance.nodeRotation.Add(lastRotation);

                instance.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z + instance.defaultHeight));
                instance.nodeRotation.Add(lastRotation);
                

            }
            else
            {
                lastPos = instance.nodePositions[instance.nodePositions.Count - 1];
                lastRotation = instance.nodeRotation[instance.nodeRotation.Count - 1];

                instance.nodePositions.Add(new Vector3(lastPos.x, lastPos.y, lastPos.z));
                instance.nodeRotation.Add(lastRotation);
            }

        }
    }


    protected virtual void OnSceneGUI()
    {
        Vector3 lastPos = new Vector3();

        for (int i = 0; i < instance.nodePositions.Count; i++)
        {
            Handles.color = Color.white * 0.5f;

            EditorGUI.BeginChangeCheck();

            Vector3 position = Handles.FreeMoveHandle(instance.transform.position + instance.nodePositions[i], instance.transform.rotation, 1f, new Vector3(), Handles.SphereHandleCap);

            Quaternion rotation = Handles.RotationHandle(instance.nodeRotation[i], instance.transform.position + instance.nodePositions[i]);

            if (i>0){
                Handles.color = Color.blue;
                Handles.DrawLine(lastPos, position);
            }


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(instance, "Change Road");
                instance.nodePositions[i] = position;
                instance.nodeRotation[i] = rotation;
            }

            lastPos = position;
        }

        if (instance.vertices != null)
        {
            //Make a label at each vertice
            for (int i = 0; i < instance.vertices.Length; i++)
            {
                Vector3 vertex = instance.vertices[i];
                Handles.RectangleHandleCap(i, instance.transform.position + vertex, instance.transform.rotation, 0.1f, EventType.Repaint);
                Handles.Label(instance.transform.position + vertex, string.Format("{0}:{1}", i, vertex.ToString()));
            }
        }
    }
    */

#endif


public class RoadMeshGenerator : MonoBehaviour
{
    public List<RoadMeshSection> roadMeshSections;
}
