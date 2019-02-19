Rendering Large Point Clouds in Unity

Bachelor Thesis, Simon Fraiss

simon.fraiss@gmx.at

Namespaces:
* CloudData: Containing data structures for storing the cloud data
* CloudController: MonoBehaviour-Scripts for loading PointClouds
* Controllers: Mostly old obselete classes. You can use the CameraController if you want though.
* DataStructures: Useful data structures used in the Project
* Loading: The GameObject-Update-System
* ObjectCreation: MeshConfigurations defining how the points should be rendered

Important classes for usage:
* CloudController.StaticPointCloudSet: Needed for simple point cloud rendering (everything is rendered at once)
* CloudController.DynamicPointCloudSet: Needed for dynamic point cloud rendering (based on current camera position)
* CloudController.PointCloudLoader: Use this script to load one point cloud and attach it to the PointCloudSet
* ObjectCreation.MeshConfiguration: Attach one of its subclasses to your scene and attach it to the PointCloudSet, in order to determine how the points should be rendered

