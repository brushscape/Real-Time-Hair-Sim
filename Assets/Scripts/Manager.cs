using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteHair
{
    GameObject hairOb;
    LineRenderer hairLine;
    SimpleHairCreator guide;
    HairCreator hair;

    public CompleteHair(Material mat, GameObject anchor, Vector3 anchorOffset, float timeStep, float crossArea, int segNum, float segLength, float guideSegLength)
    {
        hairOb = new GameObject();
        hairOb.AddComponent<LineRenderer>();
        hairOb.GetComponent<Renderer>().material = mat; 
        hairLine = hairOb.GetComponent<LineRenderer>();
        hairLine.materials[0] = mat;//new Material(Shader.Find("Sprites/Default"));
        //hairLine.SetColors(Color.white, Color.white);

        guide = new SimpleHairCreator(anchor, anchorOffset, timeStep, crossArea, segNum, guideSegLength);
        hair = new HairCreator(anchor, anchorOffset, guide, hairLine, timeStep, crossArea, segNum, segLength);
    }

    public void UpdateHair(float segLength, float kStand, 
        float connectLength, float connectKStand, 
        float exponent, float scale, 
        float guideConnectK, float guideConnectLength, float guideConnectDamp,
        float guideSegLength, float guideKStand, float guideDamping, 
        float gravityY)
    {
        guide.UpdateRender(guideSegLength, guideKStand, guideDamping, gravityY);
        hair.UpdateRender(segLength, kStand, connectLength, connectKStand, exponent, scale, guideConnectK, guideConnectLength, guideConnectDamp);
    }

}

public class Manager : MonoBehaviour
{
    public GameObject Anchor;

    [Range(5f, 20f)]
    public float segLength; //length of spring in m
    [Range(5f, 50f)]
    public float kStand; //young's modulus more or less  
    [Range(20f, 80f)]
    public float connectLength; //length of spring in m
    [Range(5f, 50f)]
    public float connectKStand; //young's modulus more or less  
    [Range(1f, 10f)]
    public float exponent; // to minimize jittering 
    [Range(1f, 5f)]
    public float scale; // to minimize jittering 
    [Range(5f, 20f)]
    public float guideConnectK;
    [Range(5f, 200f)]
    public float guideConnectLength;
    [Range(1f, 50f)]
    public float guideConnectDamp;
    [Range(5f, 20f)]
    public float guideSegLength; //length of spring in m
    [Range(5f, 50f)]
    public float guideKStand; //young's modulus in gigapascals (10^9)
    [Range(5f, 50f)]
    public float guideDamping; //damping coefficient 
    [Range(0f, 15f)]
    public float gravityY;


    const int segAmount = 20;
    const float area = 0.5f;
    const float timeStep = 0.4f;

    List<CompleteHair> hair = new List<CompleteHair>(); 

    // Start is called before the first frame update
    void Start()
    {
        segLength = 20.0f;
        kStand = 43;
        connectLength = 20.0f;
        connectKStand = 50;
        exponent = 6;
        scale = 1.8f;
        guideConnectK = 10;
        guideConnectLength = segLength;
        guideConnectDamp = 7;
        guideKStand = 18;
        guideSegLength = 5;
        guideDamping = 30;
        gravityY = 10;

        float xOffset = 3;
        float yOffset = 3;
        float xMax = 10;
        float yMax = 10;
        float xStart = -xMax / 2 * xOffset;
        float yStart = -yMax / 2 * yOffset;
        Material hairMat = (Material)Resources.Load("hair mat", typeof(Material));
        for (int i = 0; i < xMax; i++)
        {
            for(int j = 0; j < yMax; j++)
            {
                CompleteHair hairOb = new CompleteHair(hairMat, Anchor, new Vector3(xStart+xOffset*i, yStart+yOffset*j, 0), timeStep, area, segAmount, segLength, guideSegLength);
                hair.Add(hairOb); 
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < hair.Count; i++)
        {
            hair[i].UpdateHair(segLength, kStand,
                connectLength, connectKStand,
                exponent, scale,
                guideConnectK, guideConnectLength, guideConnectDamp,
                guideSegLength, guideKStand, guideDamping,
                gravityY);
        }
    }
}
