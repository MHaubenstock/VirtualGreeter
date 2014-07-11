using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ModelAnimator : MonoBehaviour
{
	public GameObject model;
	public List<ModelAnimation> animations;

	// Use this for initialization
	void Start ()
	{
		//Debug.Log(serializeAnimations());
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Input.GetKeyDown(KeyCode.Alpha1))
			StartCoroutine(animateModel(0));

		/*
		if(Input.GetKeyDown(KeyCode.Alpha2))
			StartCoroutine(animateModel(1));

		if(Input.GetKeyDown(KeyCode.Alpha3))
			StartCoroutine(animateModel(2));
		*/
	}

	void OnGUI()
	{
		for(int a = 0; a < animations.Count; ++a)
		{
			if(GUI.Button(new Rect(0, 31 * a, animations[a].name.Length * 8, 30), animations[a].name))
			{
				StartCoroutine(animateModel(a));
			}
		}
	}

	//runs as a coroutine and animates the model
	IEnumerator animateModel(int index)
	{
		ModelAnimation anim = animations[index];
		float frameProgress = 0.0F;

		//set to origin frame
		for(int o = 0; o < anim.modelTransforms.Length; ++o)
		{
			anim.modelTransforms[o].localPosition = anim.frames[0].positionStates[o];
			anim.modelTransforms[o].localRotation = anim.frames[0].rotationStates[o];
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
				for(int t = 0; t < anim.modelTransforms.Length; ++t)
				{
					anim.modelTransforms[t].localPosition = Vector3.Lerp(anim.frames[f - 1].positionStates[t], anim.frames[f].positionStates[t], frameProgress / 100.0F);
					anim.modelTransforms[t].localRotation = Quaternion.Lerp(anim.frames[f - 1].rotationStates[t], anim.frames[f].rotationStates[t], frameProgress / 100.0F);
					
					if(anim.frames[f].playbackSpeed == 0)
						frameProgress += anim.playbackSpeed;
					else
						frameProgress += anim.frames[f].playbackSpeed;
				}

				yield return false;
			}

			frameProgress = 0;
		}

		yield return true;
	}

	//For saving the animation
	public string serializeAnimations()
	{
		string serializedAnimations = "";

		//for each animation
		foreach(ModelAnimation anim in animations)
		{
			serializedAnimations += anim.serializeAnimation() + "\n";
		}

		return serializedAnimations;
	}

	public bool saveAnimations(string serializedAnimations)
	{
		using(StreamWriter outfile = new StreamWriter("Assets/Resources/AlexisAnimations.txt"))
        {
           	outfile.Write(serializedAnimations);
        }

        return true;
	}

	public bool readAnimationsFromFile()
	{
		StreamReader inFile = new StreamReader("Assets/Resources/AlexisAnimations.txt");

		return true;
	}
}

[System.Serializable]
public class ModelAnimation
{
	public string name;
	public Transform [] modelTransforms;
	public AnimationFrame [] frames;
	public float playbackSpeed = 1;

	public ModelAnimation(){}

	//For saving the animation
	public string serializeAnimation()
	{
		string serializedAnimation = "";

		//serialize name
		serializedAnimation += name + "\n";

		//Number of transforms
		serializedAnimation += modelTransforms.Length + "\n";

		//serialize transform hierarchies
		foreach(Transform tr in modelTransforms)
		{
			serializedAnimation += getHierarchy(tr) + "\n";
		}

		//Number of frames
		serializedAnimation += frames.Length + "\n";

		//serialize frames
		foreach(AnimationFrame anim in frames)
		{
			serializedAnimation += anim.serializeFrame() + "\n";
		}

		//serialize playback speed
		serializedAnimation += playbackSpeed + "\n";

		return serializedAnimation;
	}

	//Recursively builds the transforms hierarchy
	string getHierarchy(Transform tr)
	{
		//base case
		if(tr == tr.root)
			return tr.name;

		return getHierarchy(tr.parent) + "/" + tr.name;
	}
}

[System.Serializable]
public class AnimationFrame
{
	public string frameName;
	public Vector3 [] positionStates;
	public Quaternion [] rotationStates;
	public float playbackSpeed = 0;

	public AnimationFrame(int numOfTransforms, string name)
	{
		frameName = "Frame: " + name;
		positionStates = new Vector3[numOfTransforms];
		rotationStates = new Quaternion[numOfTransforms];
	}

	//For saving the frame
	public string serializeFrame()
	{
		string serializedFrame = "";

		//serialize frame name
		serializedFrame += frameName + "\n";

		//Number of position states
		serializedFrame += positionStates.Length + "\n";

		//serialize position states
		foreach(Vector3 pos in positionStates)
		{
			serializedFrame += pos.x + " " + pos.y + " " + pos.z + "\n";
		}

		//Number of rotation states
		serializedFrame += rotationStates.Length + "\n";

		//serialize rotation states
		foreach(Quaternion rot in rotationStates)
		{
			serializedFrame += rot.x + " " + rot.y + " " + rot.z + " " + rot.w + "\n";
		}

		//serialize playback speed
		serializedFrame += playbackSpeed;

		return serializedFrame;
	}
}