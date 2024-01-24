using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ### ObjectPool
 * -------
 * Implementation of an abstract object pool for efficient mass object management
 */
public class ObjectPool : MonoBehaviour
{
    /* Implemented as a Heap */ 
    private GameObject[] objectPool; //Partially Filled Array
    int count = 0;

    void Start() {}

    /**
     * #### void OnDestroy
     * Unity message for when an object is destroyed
     * Calls destroy on each owned object in order to prevent an object cascade/memory leak
     */
    private void OnDestroy()
    {
        for (int i = 0; i < count; i++) Destroy(objectPool[i]);
    }

    /**
     * #### void initializePool
     * Creates the set of objects of a given size and prepares the data structure for their retrieval
     */
    public void initializePool(int poolSize, GameObject objectClass) //Initial instantiation
    {
        objectPool = new GameObject[poolSize];
        count = poolSize;
        for (int i = 0; i < poolSize; i++)
        {
            objectPool[i] = Instantiate(objectClass, Vector3.zero, Quaternion.identity) as GameObject;
            objectPool[i].transform.parent = gameObject.transform;
            objectPool[i].SetActive(false);
        }
    }

    /**
     * #### GameObject getObject
     * Handles the retrieval of a new object from the pool
     */
    public GameObject getObject()
    {
        count--;
        objectPool[count].SetActive(true);
        return objectPool[count];
    }

    /**
     * #### void disableObject
     * A method for returning an unused object to the pool
     */
    public void disableObject(GameObject obj)
    {
        objectPool[count] = obj;
        count++;
        obj.SetActive(false);
    }
}
