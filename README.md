# BA_PointCloud
PointCloud-BachelorThesis

Project files for my bachelor thesis on rendering large point clouds in Unity.

Projects:
* PointCloudRenderer: Main-Project.  
	This project is able to load a pointcloud in the Potree-format, as described in the bachelor thesis.
	Please refer to the code documentation for details about the classes and scripts (Folder "/doc").
	For details about the algorithms please refer to the bachelor thesis (will be linked here soon).
	
	There are currently three demo projects:
	
	Assets/Scenes/BigCloudScene.unity:
		In this example scene, all clouds from all folders in the folder "Clouds/other/Simeon_Klein" are loaded.
		It contains three cameras: "Main Camera" is a freely moveable (WASDEQ, Speed Change with Shift and C) camera. DebugCamera is fixed on a certain position and closes the scene after 10 seconds.
		DebugCamera2 moves on a predefined path for 40 seconds before closing the application.
	
	Assets/Scenes/DemoScene.unity:
		In this example scene, four separate clouds are loaded.
	
	Assets/Scenes/OneTimeScene.unity:
		In this example scene, one cloud is loaded at the beginning completely (without the lod-algorithms from the thesis).
		
	Assets/Scenes/RenderTestScene.unity:
		In this scene, only 12 points are rendered in order to test the correct rendering.
	  
* Tests: Used to test several classes in the other projects.