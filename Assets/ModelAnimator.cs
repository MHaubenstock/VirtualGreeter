using UnityEngine;
using System.Collections;

public class ModelAnimator : MonoBehaviour
{
	public GameObject model;
	public ModelAnimation [] animations;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnGUI()
	{
		if(GUI.Button(new Rect(0, 0, 50, 30), "Blink"))
		{
			StartCoroutine(animateModel(0));
		}
	}

	//runs as a coroutine and animates the model
	IEnumerator animateModel(int index)
	{
		ModelAnimation anim = animations[index];
		float playbackSpeed = 0.5F;
		float frameProgress = 0.0F;

		//set to origin frame
		for(int o = 0; o < anim.frames[0].theTransforms.Length; ++o)
		{
			anim.frames[0].theTransforms[o].position = anim.frames[0].positionStates[o];
			anim.frames[0].theTransforms[o].eulerAngles = anim.frames[0].rotationStates[o];
		}

		//Now animate
		//for each frame....
		for(int f = 1; f < anim.frames.Length; ++f)
		{
			//while not finished progressing through the frame
			while(frameProgress < 100)
			{
				//anim.frames
				//Vector3.Lerp()

				yield return false;
			}
		}

		yield return true;
	}
}

[System.Serializable]
public class ModelAnimation
{
	public Transform [] modelTransforms;
	public AnimationFrame [] frames;

	public ModelAnimation(){}
}

[System.Serializable]
public class AnimationFrame
{
	public string frameName;
	public Transform [] theTransforms;
	public Vector3 [] positionStates;
	public Vector3 [] rotationStates;

	public AnimationFrame(int numOfTransforms, string name)
	{
		frameName = "Frame: " + name;
		theTransforms = new Transform[numOfTransforms];
		positionStates = new Vector3[numOfTransforms];
		rotationStates = new Vector3[numOfTransforms];
	}
}