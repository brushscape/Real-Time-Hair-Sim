using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrag : MonoBehaviour
{
    private Color mouseOverColor = Color.grey;
    private Color originalColor = Color.white;
    private bool dragging = false;
    private float distance;


    void OnMouseEnter()
    {
        this.GetComponent<Renderer>().material.color = mouseOverColor;
    }

    void OnMouseExit()
    {
        this.GetComponent<Renderer>().material.color = originalColor;
    }

    void OnMouseDown()
    {
        distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);
        dragging = true;
    }

    void OnMouseUp()
    {
        dragging = false;
    }

    void Update()
    {
        if (dragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);
            this.transform.position = new Vector3(rayPoint.x, rayPoint.y, 0);
        }
    }
}
