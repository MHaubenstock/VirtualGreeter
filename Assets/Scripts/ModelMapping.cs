using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModelMapping : MonoBehaviour
{
	public GameObject model;

	public Joint[] joints;
	public Muscle[] muscles;

	private ModelPoint lastSearchedFor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/*
	Transform getPointByID(string id)
	{
		//once found set lastSearchedFor = found point for easy relookup when getting mirrored point as well
	}

	Transform getMirrorPointByID(string id)
	{
		
	}
	*/
}

public class ModelPoint
{
	public string id;

	//The GameOebjct for the point on the model this manipulates
	public Transform point;
	public Transform pointMirror;

	//This is how the mirrored point behaves relative to the other point
	public bool mirrorsPosX;
	public bool mirrorsPosY;
	public bool mirrorsPosZ;

	public bool mirrorsRotX;
	public bool mirrorsRotY;
	public bool mirrorsRotZ;
}

[System.Serializable]
public class Joint : ModelPoint
{

}

[System.Serializable]
public class Muscle : ModelPoint
{
	
}