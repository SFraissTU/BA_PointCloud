# BA_PointCloud
PointCloud-BachelorThesis

Project files for my bachelor thesis on rendering point clouds in Unity.

Projects:
* Points1: A simple test project for trying to render simple point clouds.
  In this project there are three objects in the scene, of which only one should be enabled.
  Depending on what you want to try, you can enable different objects:
  PointCloud_Plane: A plane-pointcloud is created by creating random points and rendering them as circles (using the Quads-Primitive)
  PointCloud_Lion_Quads: The Lion-Cloud is loaded and rendered with circles (using the Quads-Primitive)
  PointCloud_Lion_Points: The Lion-Cloud is loaded and rendered with points (using the Points-Primitive. PointSize doesn't seem to work)
  The used PointCloud is an upscaled version of the Lion-PointCloud. It is also subsampled because the original cloud file was too big for github. So there are only ~880.000 points instead of 4 million.
