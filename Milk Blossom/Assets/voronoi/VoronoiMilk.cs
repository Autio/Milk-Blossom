using UnityEngine;

public class VoronoiMilk : MonoBehaviour {

    public float minX = -1;
    public float maxX = +1;

    public float minY = -1;
    public float maxY = +1;

    public int length = 100;
    
    public Vector2[] points;
    public Color[] colours;
    public Color[] colourChoices;
    public Vector2[] destinationPoints;
    public float[] speeds;
    public float[] tParams;

    private float moveCounter = 0f;

    private Material material;
	// Use this for initialization
	void Start () {
        material = GetComponent<Renderer>().sharedMaterial;

        points = new Vector2[length];
        destinationPoints = new Vector2[length];
        colours = new Color[length];
        speeds = new float[length];
        tParams = new float[length];

        for (int i = 0; i < length; i++)
        {
            points[i] = new Vector2
                (
                    transform.position.x + Random.Range(minX, maxX),
                    transform.position.y + Random.Range(minY, maxY)
                );

            destinationPoints[i] = new Vector2
                (
                    points[i].x + Random.Range(-1.5f, 1.5f),
                    points[i].y + Random.Range(-1.5f, 1.5f)
                );

            speeds[i] = Random.Range(0.05f, 0.1f);
            tParams[i] = Random.Range(1f, 5.00f);
                
            colours[i] = colourChoices[Random.Range(0, colourChoices.Length)];
            // shader 
            material.SetVector("_Points" + i.ToString(), points[i]);
            material.SetVector("_Colors" + i.ToString(), colours[i]);
        }
        material.SetInt("_Length", length);
	}

    [Range(0, 1)]
    public float amount = 0;
    float tParam = 0.0f;
    bool ticking = false;
    float minP = 1.42f;
    float maxP = 1.8f;
    float PMod = 5.0f;
    bool tog = false;
    float P;
    float PCounter = 0.01f;

    void FixedUpdate()
    {
        PCounter += Time.deltaTime;
        moveCounter += Time.deltaTime;

        if (moveCounter > 0.1f)
        {
            moveCounter = 0.01f;


            if (ticking)
            {
                // all points move at the same pace
                if (tParam < 1.0f)
                {
                    tParam += Time.deltaTime;
                }
                else
                {

                    SetTargetPoints();
                }

                if (amount == 0)
                    return;

                for (int i = 0; i < length; i++)
                {
                    points[i].x = Mathf.Lerp(points[i].x, destinationPoints[i].x, tParam * Time.deltaTime);//Random.Range(0.1f, 0.3f));
                    points[i].y = Mathf.Lerp(points[i].y, destinationPoints[i].y, tParam * Time.deltaTime);//Random.Range(0.1f, 0.3f));

                    // Shader 
                    material.SetVector("_Points" + i.ToString(), points[i]);
                    material.SetVector("_Colors" + i.ToString(), colours[i]);
                }
            }
            else
            {


                for (int i = 0; i < length; i++)
                {
                    // variable speeds
                    if (tParams[i] < 1.0f)
                    {
                        tParams[i] += Time.deltaTime * speeds[i];
                    }
                    else
                    {
                        tParams[i] = 0;

                        destinationPoints[i] = new Vector2
                        (
                            points[i].x + Random.Range(-1.5f, 1.5f),
                            points[i].y + Random.Range(-1.5f, 1.5f)
                        );
                    }

                    if (amount == 0)
                        return;
                    points[i].x = Mathf.Lerp(points[i].x, destinationPoints[i].x, tParams[i] * Time.deltaTime);//Random.Range(0.1f, 0.3f));
                    points[i].y = Mathf.Lerp(points[i].y, destinationPoints[i].y, tParams[i] * Time.deltaTime);//Random.Range(0.1f, 0.3f));

                    // Shader 
                    material.SetVector("_Points" + i.ToString(), points[i]);
                    material.SetVector("_Colors" + i.ToString(), colours[i]);
                }
            }
            // all points have their own speed
        }

        if (PCounter > PMod)
        {
            PCounter = 0.01f;
            if (!tog)
            {
                tog = true;
            }
            else
            {
                tog = false;
            }
        }

        if (tog)
        {
            P = Mathf.Lerp(minP, maxP, PCounter / PMod);
        }
        else
        {
            P = Mathf.Lerp(maxP, minP, PCounter / PMod);
        }
        material.SetFloat("_P", P);
        
    }

    void SetSpeeds()
    {
        for (int i = 0; i < length; i++)
        {
            speeds[i] = Random.Range(0.1f, 0.4f);
        }
    }

    void SetTargetPoints()
    {
        for (int i = 0; i < length; i++)
        {
            destinationPoints[i] = new Vector2
            (
                points[i].x + Random.Range(-0.5f, 0.5f),
                points[i].y + Random.Range(-0.5f, 0.5f)
            );
        }
    }
	
}
