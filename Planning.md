# CSG with Arbitrary N-Sided Faces #

## Preliminary Very-High-Level Pseudocode: ##

_This is basically just copied from a comment on [Issue #2](https://code.google.com/p/modelthulhu/issues/detail?id=#2), but with formatting added._

**0 (Import / Preconditions).** Input polygons are cut from some original input triangle. Vertex
info for every vertex of the polygon can be linearly interpolated based on the vertex
position. The vertex positions themselves may even be specified as linear
interpolations of the original triangle's vertices. References to user-side vertex
info (integer indices or somesuch) are stored for each of the three verts of the
original triangle.

**1.** Find polygon/polygon intersections (edge cuts)

**2.** Cut polygons which have intersections

**3.** Divide model into contiguous regions (cut edges form the boundaries between
regions)

**4.** Eliminate/flip regions of each mesh based on which side of the other mesh they're
on

**5.** The remaining polygons may be used as the input for subsequent CSG operations

**6 (Export).** When a series of CSG operations is completed and a triangulated mesh is required,
interpolate the vertex info referenced by the polygons' vertices to get the vertex
info for the created vertices. Alternatively, it could be set up so that this data
can be cached user-side, with some sort of conditional calculation (only generating
new vertex info (uv coords, per-vertex normals, etc.) when a new vertex is created).