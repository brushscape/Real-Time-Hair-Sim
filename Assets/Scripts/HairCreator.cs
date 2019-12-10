using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HairSegment
{
    private Vector3 position;
    private GameObject spring;
    private LineRenderer springLine;

    private int index;
    
    public float Mass { get; set; }
    public Vector3 Position {
        get { return position; }
        set {
        position = value; 
        }
    }
    public Vector3 Velocity { get; set; }

    public HairSegment(int segIndex, float mass, Vector3 pos, Vector3 vel)
    {

        position = pos;
        Velocity = vel; 
        Mass = mass;
        index = segIndex; 
    }

}

public class HairCreator
{
    HairSegment[] segment;

    float area; //cross section area of spring for young's modulus
    int segAmount; //number of segments in hair
    float timestep;
    GameObject anchor;
    Vector3 anchorOffset; 

    float damping = 7; 
    float connectDamping = 11; 


    LineRenderer hair;
    SimpleHairCreator guideCreator; 


    // Start is called before the first frame update
    public HairCreator(GameObject anchorOb, Vector3 offset, SimpleHairCreator guide, LineRenderer hairLine, 
        float timeStep, float crossArea, int segNum, float segLength)
    {
        timestep = timeStep;
        area = crossArea;
        segAmount = segNum;
        anchor = anchorOb;
        anchorOffset = offset; 

        guideCreator = guide;
        hair = hairLine;

        Vector3 currAnchorPos = anchor.transform.position + anchorOffset; 

        //set up individual hair segments
        segment = new HairSegment[segAmount];
        for (int i = 0; i < segAmount; i++)
        {
            // index, mass, initial position, initial velocity 
            segment[i] = new HairSegment(i, 25, currAnchorPos - new Vector3(0, segLength * (i+1), 0), new Vector3(0, 0, 0));
        }
        
    }


    // Update is called once per frame
    public void UpdateRender(float segLength, float kStand, 
        float connectLength, float connectKStand, 
        float exponent, float scale, 
        float guideConnectK, float guideConnectLength, float guideConnectDamp)
    {
        //get all hair positions at current time (then won't be inconsistencies when doing math with connections)
        List<Vector3> hairPointPos = new List<Vector3>(); 
        for(int i = 0; i < segAmount; i++)
        {
            hairPointPos.Add(segment[i].Position);
        }

        //get current grav and k strengths 
        float k = calculateK(kStand, area, segLength);
        float connectK = calculateK(connectKStand, area, connectLength);
        Vector3 anchorPos = anchor.transform.position + anchorOffset;


        //previous point in hair chain
        Vector3 prevObjectPos = anchorPos;
        //slightly offset anchors for points close to anchor to connect to (adds curl at top of hair)
        Vector3 anchorStand1 = anchorPos - new Vector3(1, 1, 2);
        Vector3 anchorStand2 = anchorPos - new Vector3(-1, 2, 1);

        //Math time
        for (int i = 0; i < segAmount; i++)
        {
            //get force from gravity and connection to previous hair point
            Vector3 springForce = calculateSpringForce(segLength, k, damping, segment[i].Position, prevObjectPos, segment[i].Velocity);
            Vector3 force = springForce;

            //add force from connection to next hair point
            if (i < segAmount - 1)
            {
                Vector3 nextobjpos = hairPointPos[i + 1];
                force += calculateSpringForce(segLength, k, damping, segment[i].Position, nextobjpos, segment[i].Velocity);
            }

            //add connection to offset anchors for first two points in hair chain (ensures curl at top of chain)
            switch (i)
            {
                case 0:
                    force += calculateSpringForce(connectLength, connectK, connectDamping, segment[i].Position, anchorStand1, segment[i].Velocity);

                    goto case 1;
                case 1:
                    force += calculateSpringForce(connectLength, connectK, connectDamping, segment[i].Position, anchorStand2, segment[i].Velocity);
                    break;
            }

            //add connections to 2 point before and 2 points after immediate hair neighbors 
            List<Vector3> connectPositions = new List<Vector3>();
            if (i == 1 || i == 2)
            {
                connectPositions.Add(anchorPos);
            }
            if (i > 1)
            {
                connectPositions.Add(hairPointPos[i - 2]);
            }
            if (i > 2)
            {
                connectPositions.Add(hairPointPos[i - 3]);
            }
            if (i < segAmount - 2)
            {
                connectPositions.Add(hairPointPos[i + 2]);
            }
            if (i < segAmount - 3)
            {
                connectPositions.Add(hairPointPos[i + 3]);
            }

            for (int j = 0; j < connectPositions.Count; j++)
            {
                force += calculateSpringForce(connectLength, connectK, connectDamping, segment[i].Position, connectPositions[j], segment[i].Velocity);
            }

            //add force to guide
            Vector3 approxPos = guideCreator.segment[i].Position;
            force += calculateSpringForce(guideConnectLength, guideConnectK, guideConnectDamp, segment[i].Position, approxPos, segment[i].Velocity, false);

            //calculate new position for hair point 
            Vector3 acceleration = force / segment[i].Mass;
            Vector3 newVel = segment[i].Velocity + acceleration * timestep;
            Vector3 newPos = segment[i].Position + newVel * timestep;

            //adjust new position based on current distance from guide strand 
            float distance = (newPos - guideCreator.segment[i].Position).magnitude;
            float difference = Mathf.Abs(distance-guideConnectLength);

            // multiplier between 0 and 1: 0 when distance is very small and near 1 when distance is large 
            float multiplier = 1 - (1 / (scale * difference + 1));
            multiplier = Mathf.Pow(multiplier, exponent);
            acceleration *= multiplier;
            newVel = segment[i].Velocity + acceleration * timestep;
            newVel *= multiplier/2;
            newPos = segment[i].Position + newVel * timestep;

            segment[i].Velocity = newVel;
            segment[i].Position = newPos;
      

            prevObjectPos = segment[i].Position;

        }

        //NOT MY CODE; lifted from internet to make a smooth curve between all the points instead of jagged lines 
        Vector3[] linePositions = new Vector3[segAmount+1];
        linePositions[0] = anchorPos; 
        for (int i = 0; i < segAmount; i++)
        {
            linePositions[i+1] = segment[i].Position; 
        }
        float lineSegmentSize = 3f; 

        //get smoothed values
        Vector3[] smoothedPoints = LineSmoother.SmoothLine(linePositions, lineSegmentSize);

        //set line settings
        hair.positionCount = smoothedPoints.Length;
        hair.SetPositions(smoothedPoints);
    }

    //uses Young's modulus 
    private float calculateK(float young, float crossArea, float length)
    {
        return (young * (float)Math.Pow(10, 7)) * (crossArea * (float)Math.Pow(10, -6)) / length;
    }

    //uses Hooke's Law
    private Vector3 calculateSpringForce(float length, float kCoef, float dampingCoef, Vector3 pointPos, Vector3 connectPos, Vector3 velocity, bool adjust = true)
    {
        Vector3 heading = connectPos - pointPos;
        float distance = heading.magnitude;
        Vector3 direction = heading / distance; //normalized spring vector 
        if (adjust)
        {
            distance /= 3;
        }

        float springForceMag = kCoef * distance; // Hooke's law to get spring force magnitude 
        Vector3 springForce = distance > length ? springForceMag * direction : -1 * springForceMag * direction; //gets correct spring force direction
        Vector3 dampingForce = dampingCoef * velocity;
        return springForce - dampingForce; 
    }

}
