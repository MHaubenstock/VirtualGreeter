import ModelAnimator;

class PoseCreator extends EditorWindow
{
	var modelAnimator : ModelAnimator;
	var workingAnimation : ModelAnimationRaw;
	var animationName : String = "";

	// Add menu named "My Window" to the Window menu
	@MenuItem ("Window/Pose Creator")
	static function Init ()
	{
		// Get existing open window or if none, make a new one:		
		var window = ScriptableObject.CreateInstance.<PoseCreator>();
		window.position = Rect(0, 0, 250, 80);
		window.Show();
	}

	function OnGUI()
	{
		addSpace(3);

		EditorGUILayout.PrefixLabel("Model Poser Script");
		modelAnimator = EditorGUILayout.ObjectField(modelAnimator, ModelAnimator, true);

		//If there is no Model Animator attached, then don't show the rest
		if(!modelAnimator)
			return;

		addSpace(2);

		if(!workingAnimation)
		{
			EditorGUILayout.PrefixLabel("Animation Name:");
			animationName = GUILayout.TextField(animationName);

			if(GUILayout.Button("Begin New Animation"))
			{
				//Creates new animation and sets begin state
				workingAnimation = new ModelAnimationRaw(animationName, modelAnimator.model.GetComponentsInChildren.<Transform>());
			}
		}
		else
		{
			EditorGUILayout.LabelField(animationName);

			addSpace(1);

			if(GUILayout.Button("Save State"))
			{
				getState();
				Debug.Log(workingAnimation.frames.length);
			}

			if(GUILayout.Button("Finsh & Process"))
			{
				processFinishedAnimation();
			}

			//Information
			for(var fr : AnimationFrameRaw in workingAnimation.frames.ToBuiltin(AnimationFrameRaw) as AnimationFrameRaw[])
			{
				EditorGUILayout.LabelField(fr.frameName);
			}
		}

		addSpace(3);

		//Reset
		if(GUILayout.Button("Reset"))
			reset();


		if(GUILayout.Button("Smooth Recent Animation"))
		{
			//modelAnimator.animations.Add(modelAnimator.equalizeAnimation(modelAnimator.animations[6], 1));
			modelAnimator.animations.Add(modelAnimator.setAnimationPlaybackAsCloseAsPossible(modelAnimator.animations[4], 4));
			//modelAnimator.animations.RemoveAt(14);
		}

		if(GUILayout.Button("Merge Animations"))
		{
			var modTest : ModelAnimation[] = [modelAnimator.animations[12], modelAnimator.animations[4]];
			modelAnimator.animations.Add(modelAnimator.mergeAnimations(modTest));
		}

		if(GUILayout.Button("String Together Animations"))
		{
			//var animations : ModelAnimation[] = [modelAnimator.animations[0], modelAnimator.animations[0], modelAnimator.animations[5], modelAnimator.animations[0], modelAnimator.animations[5]];
			var animations : ModelAnimation[] = [modelAnimator.animations[14], modelAnimator.animations[15]];
			var pauses : float[] = [0.05];

			modelAnimator.animations.Add(modelAnimator.stringAnimationsForNewAnimation(animations, pauses, false, false));
		}

		if(GUILayout.Button("Save All Animations"))
		{
			//get serialized animations and save them to a file
			var serializedAnimations : String = modelAnimator.serializeAnimations();
			modelAnimator.saveAnimations(serializedAnimations);			
		}

		if(GUILayout.Button("Load All Animations"))
			modelAnimator.readAnimationsFromFile();

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
		var usedATransform : boolean = false;

		var tempAnimation : ModelAnimation = new ModelAnimation();
		tempAnimation.name = animationName;
		tempAnimation.frames = new AnimationFrame[workingAnimation.frames.length];

		Debug.Log("Start Processing");
		//keep list of bools for tranforms that change throughout then use it at the end
		//to create a starting state as the first frame
		var usedTransforms : boolean[] = new boolean[workingAnimation.modelTransforms.length];

		//Get used transforms
		for(var ua : int = 1; ua < workingAnimation.frames.length; ++ua)
		{
			//compare this frame to frame a-1, keep only transforms and vector 3's that change
			for(var uf : int = 0; uf < workingAnimation.frames[ua].theTransforms.length; ++uf)
			{
				if((workingAnimation.frames[ua].positionStates[uf] != workingAnimation.frames[ua - 1].positionStates[uf]) || (workingAnimation.frames[ua].rotationStates[uf] != workingAnimation.frames[ua - 1].rotationStates[uf]))
				{
					usedTransforms[uf] = true;
					usedATransform = true;
				}
			}
		}

		//for each frame in the working animation
		for(var a : int = 1; a < workingAnimation.frames.length; ++a)
		{
			Debug.Log("Process frame: " + a);
			//compare this frame to frame a-1, keep only transforms and vector 3's that change
			for(var f : int = 0; f < workingAnimation.frames[a].theTransforms.length; ++f)
			{
				if(usedTransforms[f] || !usedATransform)
				{
					//add to array of used transforms for this frame
					transformArr.Add(workingAnimation.frames[a].theTransforms[f]);
					positionArr.Add(workingAnimation.frames[a].positionStates[f]);
					rotationArr.Add(workingAnimation.frames[a].rotationStates[f]);
				}
			}

			//Finish processing frame
			tempAnimation.frames[a] = new AnimationFrame(transformArr.length, "Frame " + a);
			tempAnimation.modelTransforms = transformArr.ToBuiltin(Transform);
			tempAnimation.frames[a].positionStates = positionArr.ToBuiltin(Vector3);
			tempAnimation.frames[a].rotationStates = rotationArr.ToBuiltin(Quaternion);

			transformArr.Clear();
			positionArr.Clear();
			rotationArr.Clear();
		}

		//process first frame as a starting state
		for(var x : int = 0; x < usedTransforms.length; ++x)
		{
			if(usedTransforms[x] || !usedATransform)
			{
				transformArr.Add(workingAnimation.frames[0].theTransforms[x]);
				positionArr.Add(workingAnimation.frames[0].positionStates[x]);
				rotationArr.Add(workingAnimation.frames[0].rotationStates[x]);
			}
		}

		tempAnimation.frames[0] = new AnimationFrame(transformArr.length, "Origin Frame");
		tempAnimation.modelTransforms = transformArr.ToBuiltin(Transform);
		tempAnimation.frames[0].positionStates = positionArr.ToBuiltin(Vector3);
		tempAnimation.frames[0].rotationStates = rotationArr.ToBuiltin(Quaternion);

		modelAnimator.animations.Add(tempAnimation);
		Debug.Log("Done Processing");
	}

	function reset()
	{
		workingAnimation = null;
	}
}

public class ModelAnimationRaw
{
	var animationName : String;
	var modelTransforms : Transform[];
	var frames : Array = new Array();
	var frameNumber : int = 1;

	function ModelAnimationRaw(){}

	function ModelAnimationRaw(name : String, points : Transform[])
	{
		animationName = name;
		modelTransforms = points;
		getState();
	}

	function getState()
	{
		var tempFrame = new AnimationFrameRaw(modelTransforms.length, animationName + "_Frame_" + frameNumber.ToString());

		//Iterate through each transform in the model and gather its position and rotation
		for(var t : int = 0; t < modelTransforms.length; ++t)
		{
			tempFrame.theTransforms[t] = modelTransforms[t];
			tempFrame.positionStates[t] = modelTransforms[t].localPosition;
			//tempFrame.rotationStates[t] = modelTransforms[t].localEulerAngles;
			tempFrame.rotationStates[t] = modelTransforms[t].localRotation;
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
	//var rotationStates : Vector3[];
	var rotationStates : Quaternion[];

	function AnimationFrameRaw(numOfTransforms : int, name : String)
	{
		frameName = name;
		theTransforms = new Transform[numOfTransforms];
		positionStates = new Vector3[numOfTransforms];
		//rotationStates = new Vector3[numOfTransforms];
		rotationStates = new Quaternion[numOfTransforms];
	}
}
