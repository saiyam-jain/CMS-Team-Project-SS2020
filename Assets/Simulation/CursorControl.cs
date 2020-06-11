using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorControl : MonoBehaviour
{
    public Simulation simulation;

    [Range(0f, 1f)] public float mouseRadius = 0.01f;
    private Vector2 mouseUV;
    public Vector2 MouseUV { get { return mouseUV; } }

    private void Update() //Runs when mouse is hovering on the simulation.
    {
        UpdateCursor();
        //UpdatePauseResume();
    }

    // I thought of adding pause, resume and reduce the play speed of simulation, 
    // but I realised that the below commands will pause the game, but not the shader computation
    //private void UpdatePauseResume()
    //{
    //    if (Input.GetKeyDown(KeyCode.P))
    //        Time.timeScale = 0.0f;
    //    else if (Input.GetKeyDown(KeyCode.R))
    //        Time.timeScale = 1.0f;
    //    else if (Input.GetKeyDown(KeyCode.Space))
    //        Time.timeScale = 0.5f;
    //}

    private void UpdateCursor()
    {
        RaycastHit hit;
        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Using mouse scroll to vary the radius.
        if (Input.mouseScrollDelta.y < 0)
            mouseRadius -= 0.01f;
        else if (Input.mouseScrollDelta.y > 0)
            mouseRadius += 0.01f;

        if (mouseRadius <= 0 && Input.mouseScrollDelta.y > 0)
            mouseRadius = 0.01f;

        if (!Physics.Raycast(rayOrigin, out hit))
        {
            Debug.Log("Mouse is not on Simulation");
            simulation.Shader.SetFloat("mouseradius", 0);
            return;
        }

        Debug.Log("Mouse on simulation");
        simulation.Shader.SetFloat("mouseRadius", mouseRadius);
        mouseUV = hit.textureCoord;     //Passing mouse Tex Coord to GPU
        simulation.Shader.SetVector("mouseUV", mouseUV);
    }
}
