using UnityEngine;
using System.Collections;
using UnityEditor;
[ExecuteInEditMode]
public class ArtificialPosition : MonoBehaviour {

	public Vector2 position;
	public int layer;
	public Vector2 offset;




	void Update()
	{
		if (Application.isPlaying)
			Destroy (this);
		else if (Selection.activeGameObject == this.gameObject) {
			position=offset +(Vector2)transform.position;
			}
		}

}
