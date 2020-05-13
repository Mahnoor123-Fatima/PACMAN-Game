using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nodes : MonoBehaviour {

    public nodes[] neighbours;
    public Vector2[] validDirections;
	// Use this for initialization
	void Start () {

        validDirections = new Vector2[neighbours.Length];

        for (int i = 0; i < neighbours.Length; i++)
        {
            nodes neighbour = neighbours[i];
            Vector2 tempVector = neighbour.transform.localPosition - transform.localPosition;

            validDirections[i] = tempVector.normalized;
        }

	}
	
	// Update is called once per frame

}
