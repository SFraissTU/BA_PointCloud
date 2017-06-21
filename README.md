# BA_PointCloud
PointCloud-BachelorThesis

Project files for my bachelor thesis on rendering large point clouds in Unity.

Projects:
* PointCloudRenderer: Main-Project.  
	This project is able to load a pointcloud in the Potree-format.
	
	Assets/Scenes/BigCloudScene.unity:
		In this example scene, all cloud from all folders in the folder "Clouds/big" are loaded. Rendering is done in Real-Time.
	
	Assets/Scenes/DemoScene.unity:
		In this example scene, four cloudes are loaded. Rendering is done in Real-Time.
	
	Assets/Scenes/OneTimeScene.unity:
		In this example scene, four cloudes are loaded. Rendering is only done when pressing X.
	
	For details about the classes, scripts and game objects (in order to use them in your own projects) please refer to the code documentation (in work).
	For details about the algorithms please refer to the bachelor thesis (will be linked here soon).
  
* Tests: Used to test several classes in the other projects.