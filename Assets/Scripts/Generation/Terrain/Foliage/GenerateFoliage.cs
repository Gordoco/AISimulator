using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GenerateTerrain))]
/**
 * ### GenerateFoliage
 * -------
 * Utilizes ObjectPools to create dynamic and procedural foliage on a TerrainSection
 */
public class GenerateFoliage : MonoBehaviour
{
    /*
     * Public Instance Variables
     */
    public GameObject foliageClass;
    public int foliageDensity = 1; //Number of entities per chunk
    public float yOffset = 0;
    public float radius = 10f; //Can be set in engine if not with a mesh 
    public float foliageRenderDist = -1;
    public bool b2D = false;

    /*
     * Private Instance Variables
     */
    private Mesh meshComp;
    private GameObject[] foliageObjs; //Partially filled array
    private Renderer[] foliageRenderers; //Partially filled array
    private int fCount = 0; //Counter for ^
    private GameObject Player;
    private GenerateTerrain generator;
    private ObjectPool foliagePool;
    private int poolStartIndex;

    void Start() {}

    /**
     * #### void LateUpdate
     * Unity editor event called at the end of every frame
     * Calculates whether each foliage instance is needed to be rendered based on Player position
     */
    void LateUpdate()
    {
        if (foliageRenderDist != -1)
        {
            if (Player == null) Player = GameObject.FindGameObjectWithTag("Player"); //Save proper Player Reference
            for (int i = 0; i < fCount; i++)
            {
                if (Vector3.Distance(Player.transform.position, foliageObjs[i].transform.position) < foliageRenderDist) //Compare Dists
                {
                    foliageRenderers[i].enabled = true; //Show

                    // Rotate Sprites to face Camera
                    if (b2D) foliageObjs[i].transform.rotation = Camera.main.transform.rotation; 
                    //
                }
                else
                {
                    foliageRenderers[i].enabled = false; //Hide
                }
            }
        }
    }

    /**
     * #### void initialize
     * Sets up foliage generation
     */
    public void initialize()
    {
        if (foliageClass.GetComponent<MeshFilter>()) //Check if using mesh
        {
            meshComp = foliageClass.GetComponent<MeshFilter>().sharedMesh; //Get Mesh

            /* Calculate 3D model safe spawning radius (with pre-set offset) */
            radius += Mathf.Max(meshComp.bounds.size.x * foliageClass.transform.localScale.x, meshComp.bounds.size.z * foliageClass.transform.localScale.z);
        }
        SpawnFoliage();
    }

    /**
     * #### void setFoliagePool
     * Mutator method for allowing external allocation of an ObjectPool for foliage generation
     */
    public void setFoliagePool(ObjectPool pool)
    {
        foliagePool = pool;
    }

    /**
     * #### void SpawnFoliage
     * Instantiates foliage instances across the owning terrain area
     */
    void SpawnFoliage()
    {
        /* Initialization */
        if (generator == null) generator = GetComponent<GenerateTerrain>();
        ClearFoliage();
        int xSize = generator.xSize;
        int zSize = generator.zSize;
        foliageObjs = new GameObject[foliageDensity];
        foliageRenderers = new Renderer[foliageDensity];
        int count = 0;

        for (int i = 0; i < foliageDensity; i++)
        {
            // NEED TO CHANGE TO NON-OVERLAPPING RANDOM NUMBERS
            float xVal = Random.Range(0f, xSize) + gameObject.transform.position.x;
            float zVal = Random.Range(0f, zSize) + gameObject.transform.position.z;
            //

            instantiateFoliageInstance(xVal, zVal, count);
            count++;
        }
    }

    /**
     * #### void ClearFoliage
     * Returns all foliage to its ObjectPool
     */
    void ClearFoliage()
    {
        if (foliageObjs == null) return;
        for (int i = 0; i < fCount; i++)
        {
            foliagePool.disableObject(foliageObjs[i]);
        }
        fCount = 0;
    }

    /**
     * #### void instantiateFoliageInstance
     * Creates a foliage instance from its ObjectPool
     */
    void instantiateFoliageInstance(float xVal, float zVal, int count)
    {
        GameObject foliage = foliagePool.getObject();
        foliage.transform.position = new Vector3(xVal, generator.getYAtLocation(new Vector2(xVal, zVal)) + yOffset, zVal);
        foliage.transform.rotation = Quaternion.Euler(0, Random.Range(-180, 180), 0);
        foliageObjs[count] = foliage;
        foliageRenderers[count] = foliage.GetComponent<Renderer>();
        fCount++;
    }
}
