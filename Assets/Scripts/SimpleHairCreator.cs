using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHairSeg
{
    private Vector3 position;

    public float Mass { get; set; }
    public Vector3 Position {
        get { return position; }
        set {
        position = value; 
        }
    }
    public Vector3 Velocity { get; set; }

    public SimpleHairSeg(float mass, Vector3 pos, Vector3 vel)
    {

        position = pos;
        Velocity = vel; 
        Mass = mass; 
    }

}

public class SimpleHairCreator 
{
    float timestep;
    float area;
    int segAmount;
    GameObject anchor;
    Vector3 anchorOffset; 

    public SimpleHairSeg[] segment;

    // Start is called before the first frame update
    public SimpleHairCreator(GameObject anchorOb, Vector3 offset, float timeStep, float crossArea, int segNum, float segLength)
    {
        timestep = timeStep;
        area = crossArea;
        segAmount = segNum;
        anchor = anchorOb;
        anchorOffset = offset;

        Vector3 currAnchorPos = anchor.transform.position + anchorOffset; 
        segment = new SimpleHairSeg[segAmount];
        for (int i = 0; i < segAmount; i++)
        {
            segment[i] = new SimpleHairSeg(30, currAnchorPos - new Vector3(0, segLength * (i + 1), 0), new Vector3(0, 0, 0));
        }

    }


    // Update is called once per frame
    public void UpdateRender(float segLength, float KStand, float damping, float gravityY)
    {
        Vector3 gravity = new Vector3(0, -gravityY, 0);
        Vector3 currAnchorPos = anchor.transform.position + anchorOffset; 
        Vector3 prevObjectPos = currAnchorPos;
        float k = (KStand * (float)Math.Pow(10, 7)) * (area * (float)Math.Pow(10, -6)) / segLength;
        for (int i = 0; i < segment.Length; i++)
        {
            Vector3 springForce = -k * (segment[i].Position - prevObjectPos);
            Vector3 dampingForce = damping * segment[i].Velocity;

            // sphere position
            Vector3 force = springForce + segment[i].Mass * gravity - dampingForce;
            Vector3 acceleration = force / segment[i].Mass;
            segment[i].Velocity = segment[i].Velocity + acceleration * timestep;
            segment[i].Position = segment[i].Position + segment[i].Velocity * timestep;

            prevObjectPos = segment[i].Position;
        }
    }

}
