Rendering Large Point Clouds in Unity

Bachelor Thesis, Simon Fraiss

simon.fraiss@gmx.at

Namespaces:
* CloudData: Containing data structures for storing the cloud data
* Controllers: MonoBehaviour-Scripts for loading PointClouds
* DataStructures: Useful data structures used in the Project
* Eval: Helper classes used during Evaluation
* Loading: The GameObject-Update-System
* ObjectCreation: MeshConfigurations defining how the points should be rendered

Important classes for usage:
* Controllers.PointCloudSetRealTimeController: Attach this script to your scene to set rendering options like point budget etc.
* Controllers.DynamicLoaderController: Use this script to load one point cloud and attach it to the PointCloudSetRealTimeController
* ObjectCreation.MeshConfiguration: Attach one of its subclasses to your scene and attach it to the PointCloudSetRealTimeController, in order to determine how the points should be rendered

