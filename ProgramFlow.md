# As it is (mostly) #

  1. Inputs are specified as properties of a `CSG` object. This includes:
    * Both input models
    * 4x4 transformation matrices for each of the two input models
    * Region keep conditions for each of the two input models. These are delegate functions which determine, for some region of contiguous polygons in the cut models, whether the region should be kept as is, flipped and kept, or removed. In general, these will correspond to whether the region is inside or outside of the other model's volume. It can be customized to do things like:
      * Having an unclosed mesh as an input (where the unclosed mesh is a part of a larger mesh which is closed)
  1. The `Compute()` method of the `CSG` object is called.
    1. `ModelInput` objects are generated from the `BasicModelData` object (transformed by the transformation matrices)
    1. The bounding boxes of the objects are computed
    1. The third argument for the `Snip(...)` function (i.e. `List<TrianglePair> pairs`) will be...
      * If the bounding boxes intersect intersect, an Octree is generated within the region of intersection, and is populated with the triangles of the `ModelInput` objects (based on their AABBs)
      * Otherwise, an empty list
  1. The `Snip(...)` function is called...
    1. `WorkingModel` objects are generated from each `ModelInput` object, containing only those triangles not in the `pairs` list.
    1. `WorkingModel.Intersect(...)` is called. This figures out the intersections and cuts that must be made.
    1. Two `List<BasicModelVert>` objects are generated with data for each of the two meshes: one for the polygons which were not cut, and another for those which were cut.
    1. Duplicate vertices are eliminated.
    1. The `ScrapTrimmedStuff(...)` method is called for both objects. This finds the contiguous regions and handles them using the region keep conditions for their respective objects.
    1. The results of the calls to `ScrapTrimmedStuff(...)`, which are of type `List<BasicModelVert>`, are converted to `BasicModelData` objects using the `BMVListToModel(...)` function, and are stored where they can be read from as the output properties of the `CSG` object.
  1. The outputs are retrieved from properties of the `CSG` object.