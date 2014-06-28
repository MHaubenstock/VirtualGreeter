import ModelAnimator;

class PoseCreator extends EditorWindow
{
	var modelAnimator : ModelAnimator;
	var workingAnimation : ModelAnimationRaw;

	// Add menu named "My Window" to the Window menu
	@MenuItem ("Window/Pose Creator")
	static function Init ()
	{
		// Get existing open window or if none, make a new one:		
		var window = ScriptableObject.CreateInstance.<PoseCreator>();
		window.position = Rect(0, 0, 250, 80);
		window.Show();
	}
	
	function OnEnable()
	{
		
	}

	function OnGUI()
	{
		addSpace(3);

		EditorGUILayout.PrefixLabel("Model Poser Script");
		modelAnimator = EditorGUILayout.ObjectField(modelAnimator, ModelAnimator, true);

		addSpace(2);

		if(!workingAnimation)
		{
			if(GUILayout.Button("Begin New Animation"))
			{
				//Creates new animation and sets begin state
				workingAnimation = new ModelAnimationRaw(modelAnimator.model.GetComponentsInChildren.<Transform>());
			}
		}
		else
		{
			if(GUILayout.Button("Save State"))
			{
				getState();
				Debug.Log(workingAnimation.frames.length);
			}

			if(GUILayout.Button("Finsh & Process"))
			{
				processFinishedAnimation();
			}
		}

		//Information
		//Show frames
		if(workingAnimation)
			for(var fr : AnimationFrameRaw in workingAnimation.frames.ToBuiltin(AnimationFrameRaw) as AnimationFrameRaw[])
			{
				EditorGUILayout.LabelField(fr.frameName);
			}

		addSpace(3);

		//Reset
		if(GUILayout.Button("Reset"))
			reset();
	}

	function addSpace(spaces : int)
	{
		for(var x : int = 0; x < spaces; ++x)
			EditorGUILayout.Space();
	}

	function getState()
	{
		if(workingAnimation)
			workingAnimation.getState();
	}

	function processFinishedAnimation()
	{
		//work in here now
		//can gather animation frames so minimize lists to only transforms that changed

		//Placeholders
		var transformArr : Array = new Array();
		var positionArr : Array = new Array();
		var rotationArr : Array = new Array();

		var tempAnimation : ModelAnimation = new ModelAnimation();
		tempAnimation.frames = new AnimationFrame[workingAnimation.frames.length];

		Debug.Log("Start Processing");
		//keep list of bools for tranforms that change throughout then use it at the end
		//to create a starting state as the first frame
		var usedTransforms : boolean[] = new boolean[workingAnimation.modelTransforms.length];

		//for each frame in the working animation
		for(var a : int = 1; a < workingAnimation.frames.length; ++a)
		{
			Debug.Log("Process frame: " + a);
			//compare this frame to frame a-1, keep only transforms and vector 3's that change
			for(var f : int = 0; f < workingAnimation.frames[a].theTransforms.length; ++f)
			{
				if((workingAnimation.frames[a].positionStates[f] != workingAnimation.frames[a - 1].positionStates[f]) || (workingAnimation.frames[a].rotationStates[f] != workingAnimation.frames[a - 1].rotationStates[f]))
				{
					usedTransforms[f] = true;

					//add to array of used transforms for this frame
					transformArr.Add(workingAnimation.frames[a].theTransforms[f]);
					positionArr.Add(workingAnimation.frames[a].positionStates[f]);
					rotationArr.Add(workingAnimation.frames[a].rotationStates[f]);
				}
			}

			//Finish processing frame
			//tempAnimation.frames[a].theTransforms = new Transform[transformArr.length];
			//tempAnimation.frames[a].positionStates = new Vector3[positionArr.length];
			//tempAnimation.frames[a].rotationStates = new Vector3[rotationArr.length];
			tempAnimation.frames[a] = new AnimationFrame(transformArr.length, "Frame " + a);
			tempAnimation.frames[a].theTransforms = transformArr.ToBuiltin(Transform);
			tempAnimation.frames[a].positionStates = positionArr.ToBuiltin(Vector3);
			tempAnimation.frames[a].rotationStates = rotationArr.ToBuiltin(Vector3);

			transformArr.Clear();
			positionArr.Clear();
			rotationArr.Clear();
		}

		//process first frame as a starting state
		for(var x : int = 0; x < usedTransforms.length; ++x)
		{
			if(usedTransforms[x])
			{
				transformArr.Add(workingAnimation.frames[0].theTransforms[x]);
				positionArr.Add(workingAnimation.frames[0].positionStates[x]);
				rotationArr.Add(workingAnimation.frames[0].rotationStates[x]);
			}
		}

		tempAnimation.frames[0] = new AnimationFrame(transformArr.length, "Origin Frame");
		tempAnimation.frames[0].theTransforms = transformArr.ToBuiltin(Transform);
		tempAnimation.frames[0].positionStates = positionArr.ToBuiltin(Vector3);
		tempAnimation.frames[0].rotationStates = rotationArr.ToBuiltin(Vector3);

		modelAnimator.animations[0] = tempAnimation;
		Debug.Log("Done Processing");
	}

	function reset()
	{
		workingAnimation = null;
	}
}

public class ModelAnimationRaw
{
	var modelTransforms : Transform[];
	var frames : Array = new Array();
	var frameNumber : int = 1;

	function ModelAnimationRaw(){}

	function ModelAnimationRaw(points : Transform[])
	{
		Debug.Log(points.length);
		modelTransforms = points;
		getState();
	}

	function getState()
	{
		var tempFrame = new AnimationFrameRaw(modelTransforms.length, frameNumber.ToString());

		//Iterate through each transform in the model and gather its position and rotation
		for(var t : int = 0; t < modelTransforms.length; ++t)
		{
			tempFrame.theTransforms[t] = modelTransforms[t];
			tempFrame.positionStates[t] = modelTransforms[t].position;
			tempFrame.rotationStates[t] = modelTransforms[t].eulerAngles;
		}

		//Add frame to array of frames
		frames.Add(tempFrame);
		++frameNumber;
	}
}

public class AnimationFrameRaw
{
	var frameName : String;
	var theTransforms : Transform[];
	var positionStates : Vector3[];
	var rotationStates : Vector3[];

	function AnimationFrameRaw(numOfTransforms : int, name : String)
	{
		frameName = "Frame: " + name;
		theTransforms = new Transform[numOfTransforms];
		positionStates = new Vector3[numOfTransforms];
		rotationStates = new Vector3[numOfTransforms];
	}
}
