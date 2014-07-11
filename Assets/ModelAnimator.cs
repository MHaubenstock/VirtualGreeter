using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		//serialize number of animations
		serializedAnimations += animations.Count + "\n";

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

		//read number of animations
		int numOfAnimations = int.Parse(inFile.ReadLine());
		List<ModelAnimation> animations = new List<ModelAnimation>();

		//split animations into separate strings
		for(int a = 0; a < numOfAnimations; ++a)
        {
        	string animationString = "";
        	string fileLine;
        	bool readingFrame = false;

        	//read all animation data into string then send to ModelAnimation to return the animation
        	//read until you encounter an empty line
        	while((fileLine = inFile.ReadLine()) != "")
        	{
        		if(fileLine == "beginframe")
        		{
        			readingFrame = true;
        			continue;
        		}
        		else if(fileLine == "endframe")
        		{
        			readingFrame = false;
        			//ended frame, skip to next line
        			animationString += "\n";
        			continue;
        		}

        		animationString += fileLine + (readingFrame ? "\r" : "\n");
        	}

        	//send to ModelAnimation to return an animation
        	animations.Add(ModelAnimation.deserializeAnimation(animationString, model));

        	animationString = "";
        }

        //Finished creating animations from file, now assign it to our list of animations
        this.animations = animations;

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

		//insert beginning of animation indicator
		//serializedAnimation += "beginanimation\n";

		//serialize name
		serializedAnimation += name + "\n";

		//serialize playback speed
		serializedAnimation += playbackSpeed + "\n";

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
			serializedAnimation += anim.serializeFrame();
		}

		//insert ending animation indicator
		//serializedAnimation += "endanimation\n";

		return serializedAnimation;
	}

	public static ModelAnimation deserializeAnimation(string serAnim, GameObject animModel)
	{
		ModelAnimation anim = new ModelAnimation();
		Queue<string> animData = new Queue<string>(serAnim.Split('\n'));

		//Store animation name
		anim.name = animData.Dequeue();

		//Store animation playback speed
		anim.playbackSpeed = float.Parse(animData.Dequeue());

		//Create array for transform of length of number of transforms
		Transform [] transforms = new Transform[int.Parse(animData.Dequeue())];

		//Load model transforms into transform array
		for(int t = 0; t < transforms.Length; ++t)
		{
			transforms[t] = animModel.transform.Find(animData.Dequeue());
		}

		//set animation transforms
		anim.modelTransforms = transforms;

		//Next dequeue gets number of animations
		anim.frames = new AnimationFrame[int.Parse(animData.Dequeue())];

		//Get animation frames
		for(int a = 0; a < anim.frames.Length; ++a)
		{
			anim.frames[a] = AnimationFrame.deserializeFrame(animData.Dequeue());
		}

		return anim;
	}

	//Recursively builds the transforms hierarchy
	string getHierarchy(Transform tr)
	{
		//base case
		if(tr == tr.root)
			//return tr.name;
			return "";

		return getHierarchy(tr.parent) + (tr.parent == tr.root ? "" : "/") + tr.name;
	}
}

[System.Serializable]
public class AnimationFrame
{
	public string frameName;
	public Vector3 [] positionStates;
	public Quaternion [] rotationStates;
	public float playbackSpeed = 0;

	public AnimationFrame(){}

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

		//insert beginning frame indicator
		serializedFrame += "beginframe\n";

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
		serializedFrame += playbackSpeed + "\n";

		//insert ending frame indicator
		serializedFrame += "endframe\n";

		return serializedFrame;
	}

	public static AnimationFrame deserializeFrame(string serFrame)
	{
		AnimationFrame frame = new AnimationFrame();
		//Has an extra empty line
		string [] data = serFrame.Split('\r');
		//This constructs a queue with the empty line removed
		Queue<string> frameData = new Queue<string>(data.Take(data.Length - 1));

		//Store frame name
		frame.frameName = frameData.Dequeue();

		//Create array of positions (Vector3s)
		frame.positionStates = new Vector3[int.Parse(frameData.Dequeue())];

		//Store position states
		for(int p = 0; p < frame.positionStates.Length; ++p)
		{
			string [] pos = frameData.Dequeue().Split(' ');

			frame.positionStates[p].x = float.Parse(pos[0]);
			frame.positionStates[p].y = float.Parse(pos[1]);
			frame.positionStates[p].z = float.Parse(pos[2]);
		}

		//Create array of rotations (Quaternions)
		frame.rotationStates = new Quaternion[int.Parse(frameData.Dequeue())];

		//Store rotation states
		for(int r = 0; r < frame.rotationStates.Length; ++r)
		{
			string [] rots = frameData.Dequeue().Split(' ');

			frame.rotationStates[r].x = float.Parse(rots[0]);
			frame.rotationStates[r].y = float.Parse(rots[1]);
			frame.rotationStates[r].z = float.Parse(rots[2]);
			frame.rotationStates[r].w = float.Parse(rots[3]);
		}

		//Store animation playback speed
		frame.playbackSpeed = float.Parse(frameData.Dequeue());

		return frame;
	}
}





