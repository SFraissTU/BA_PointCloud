Rendering Large Point Clouds in Unity

Bachelor Thesis, Simon Fraiss

simon.fraiss@gmx.at

Namespaces:
* CloudData: Containing data structures for storing the cloud data
* CloudController: MonoBehaviour-Scripts for loading PointClouds
* Controllers: Contains a CameraController for simple navigation.
* DataStructures: Useful data structures used in the Project
* Loading: The GameObject-Update-System
* ObjectCreation: MeshConfigurations defining how the points should be rendered
* Edl: Classes for Eye-Dome-Lighting

Important classes for usage:
* CloudController.StaticPointCloudSet: Needed for simple point cloud rendering (everything is rendered at once)
* CloudController.DynamicPointCloudSet: Needed for dynamic point cloud rendering (based on current camera position)
* CloudController.PointCloudLoader: Use this script to load one point cloud and attach it to the PointCloudSet
* CloudController.DirectoryLoader: Useful if you have several point clouds in the same folder and want to load all of them.
* ObjectCreation.MeshConfiguration: Attach one of its subclasses to your scene and attach it to the PointCloudSet, in order to determine how the points should be rendered
* CloudController.Preview: Use this to show a preview of your point cloud inside the editor.

Thanks to Kazys Stepanas, Garrison Price, Pablo Vidaurre, Chris Traxler, Cognitive Data and everyone else who added contributions to this project!