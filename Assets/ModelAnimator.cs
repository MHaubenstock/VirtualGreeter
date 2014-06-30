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
		float frameProgress = 0.0F;

		//set to origin frame
		for(int o = 0; o < anim.frames[0].theTransforms.Length; ++o)
		{
			anim.frames[0].theTransforms[o].localPosition = anim.frames[0].positionStates[o];
			anim.frames[0].theTransforms[o].localEulerAngles = anim.frames[0].rotationStates[o];
		}

		//Now animate
		//for each frame....
		for(int f = 1; f < anim.frames.Length; ++f)
		{
			Debug.Log("Playing frame " + f);
			//while not finished progressing through the frame
			while(frameProgress < 100)
			{
				//for each transform in frame
				for(int t = 0; t < anim.frames[f].theTransforms.Length; ++t)
				{
					anim.frames[f].theTransforms[t].localPosition = Vector3.Lerp(anim.frames[f - 1].positionStates[t], anim.frames[f].positionStates[t], frameProgress / 100.0F);
					anim.frames[f].theTransforms[t].localEulerAngles = Vector3.Lerp(anim.frames[f - 1].rotationStates[t], anim.frames[f].rotationStates[t], frameProgress / 100.0F);
					
					frameProgress += anim.playbackSpeed;
				}

				yield return false;
			}

			frameProgress = 0;
		}

		yield return true;
	}
}

[System.Serializable]
public class ModelAnimation
{
	public Transform [] modelTransforms;
	public AnimationFrame [] frames;
	public float playbackSpeed;

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