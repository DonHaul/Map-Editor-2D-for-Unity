using UnityEngine;
using System.Collections;

public class Layer : MonoBehaviour {

	public Layer(int l,int i)
	{
		id = i;
		priority = l;

	}

	public int id;
	public int priority;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
