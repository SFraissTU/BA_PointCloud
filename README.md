# BA_PointCloud
PointCloud-BachelorThesis

Project files for my bachelor thesis on rendering point clouds in Unity.

Projects:
* Points1: A simple test project for trying to render simple point clouds. Open Assets/Scene1.unity!
  In this project there are three objects in the scene, of which only one should be enabled.
  Depending on what you want to try, you can enable different objects:
  * PointCloud_Plane: A plane-pointcloud is created by creating random points and rendering them as circles (using the Quads-Primitive). If you press the space button, the points are newly generated.
  * PointCloud_Lion_Quads: The Lion-Cloud is loaded and rendered with circles (using the Quads-Primitive)
  * PointCloud_Lion_Points: The Lion-Cloud is loaded and rendered with points (using the Points-Primitive. PointSize doesn't seem to work)
  
  The used PointCloud is an upscaled version of the Lion-PointCloud. It is also subsampled because the original cloud file was too big for github. So there are only ~880.000 points instead of 4 million.

* PointCloudRenderer: Main-Project.  
	This project is able to load a pointcloud in the Potree-format.
	
	Assets/Scenes/OneTimeScene.unity:
	
		There are two ways of rendering in this file:
			- Static Rendering: The PointCloud is loaded completely in the beginning and for every node a GameObject is created.
				This can be testet with the object "PointCloudLoader" in the scene.
			- Semi-Dynamic Rendering: Several PointClouds can be loaded. In the scene, the objects "CloudA", "CloudB" and so on definde several clouds to be loaded in this way.
				The hierarchies are loaded in the beginning. When you press "X", it is checked, which parts of the clouds are seen and for those GameObjects are created.
				The loading happens in an own thread. The object "CloudList" defines the PointBudget and the Min Projected Node Size. Also you can move the whole cloud to the origin with this object.
				You can also choose between using Multithreading or not.
		
	Assets/Scenes/DemoScene.unity (Main-Scene!!):
	
		This scene demonstrates Real-Time-Rendering: The PointCloud is constantly loaded according to your camera position.
		Four cloudes are in the scene. In the object "CloudList" you can define the PointBudget and the Min Projected Node Size.
		You can also choose between using Multithreading or not.
  
  In all methods you can also choose a MeshConfiguration by clicking on the small circle beside the textfield. The following configurations are provided:
  * DefaultPointMeshConfiguration: Draws the points as single 1px-points.
  * 4_RectQuadMeshConfiguration: Draws the points as squares. The size on screen is changeable in the inspector of this object. Implemented by giving every point 4 times to the vertex shader to create quads.
  * 4_CircleQuadMeshConfiguration: Draws the points as circles. The size on screen is changeable in the inspector of this object. Implemented by giving every point 4 times to the vertex shader to create quads.
  * GEO_RectQuadMeshConfiguration: Draws the points as squares. The size on screen is changeable in the inspector of this object. Implemented using the geometry shader.
  * GEO_CircleQuadMeshConfiguration: Draws the points as circles. The size on screen is changeable in the inspector of this object. Implemented using the geometry shader.
  * Bill_RectQuadMeshConfiguration: Draws the points as screen-faced squares. The size in the world is changeable in the inspector of this object. Implemented using the geometry shader.
  * Bill_CircleQuadMeshConfiguration: Draws the points as screen-faced circles. The size in the world is changeable in the inspector of this object. Implemented using the geometry shader.
  
* Tests: Used to test several classes in the other projects.