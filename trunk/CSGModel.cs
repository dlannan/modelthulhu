using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;

namespace TheLibrary.CSG
{
    public class CSGModel
    {
        public static long nextID = 0;

        public long id;                                             // an identifying integer

        public List<CSGVertex> vertices = new List<CSGVertex>();    // all verts in this model
        public List<CSGEdge> edges = new List<CSGEdge>();           // all the edges in this model
        public List<CSGShape> shapes = new List<CSGShape>();        // all the flat shapes in this model
        public List<CSGSourceTriangle> sourceTriangles = new List<CSGSourceTriangle>();     // all the original triangles forming this model

        public CSGModel()
        {
            id = nextID++;
        }

        public CSGModel(List<Vec3> modelVerts, List<VInfo> matchingInfos)                      // vertices grouped into 3's (3 verts per triangle)
            : this()
        {
            List<CSGVertex> vertexMapping = new List<CSGVertex>();
            for (int i = 0; i < modelVerts.Count; i++)
                vertexMapping.Add(GetVertex(modelVerts[i], true));

            for (int i = 0; i < modelVerts.Count; i += 3)
            {
                CSGVertex[] tri = new CSGVertex[3];
                VInfo[] vinfos = new VInfo[3];
                bool acceptable = true;
                for (int j = 0; j < 3; j++)
                {
                    tri[j] = vertexMapping[i + j];
                    for (int k = 0; k < j; k++)
                        if (tri[j] == tri[k])
                            acceptable = false;
                    vinfos[j] = matchingInfos[i + j];
                    if (!acceptable)
                        break;
                }
                if (acceptable)
                {
                    CSGSourceTriangle triangle = new CSGSourceTriangle(this, tri, vinfos);
                    foreach (CSGShape shape in triangle.divisions)
                        shapes.Add(shape);
                    sourceTriangles.Add(triangle);
                }
            }
        }

        public CSGEdge GetEdge(CSGVertex a, CSGVertex b, bool create)
        {
            int index = edges.FindIndex((e) => e.endpoints[0].id == a.id && e.endpoints[1].id == b.id || e.endpoints[0].id == b.id && e.endpoints[1].id == a.id);
            if (index == -1)
                if (!create)
                    return null;
                else
                {
                    edges.Add(new CSGEdge(new CSGVertex[] { a, b }));
                    return edges[edges.Count - 1];
                }
            else
                return edges[index];
        }
        public CSGVertex GetVertex(Vec3 position, bool create)
        {
            int index = vertices.FindIndex((v) => (v.position - position).ComputeMagnitudeSquared() < 0.0001);
            if (index == -1)
                if (!create)
                    return null;
                else
                {
                    vertices.Add(new CSGVertex(position));
                    return vertices[vertices.Count - 1];
                }
            else
                return vertices[index];
        }

        // clone a model (scraps any WIP intersection info)
        // not suitable if there's custom vinfo
        public CSGModel CloneModel()
        {
            return CloneModel(new ModelUtil.CopyAndTransformVInfo(
                (VInfo basis, out VInfo result) =>
                {
                    result = new VInfo();
                }));
        }
        // clone a model and transform its vinfos (scraps any WIP intersection info)
        public CSGModel CloneModel(ModelUtil.CopyAndTransformVInfo vinfoCopier)
        {
            return CloneAndTransform(new ModelUtil.CopyAndTransformVert(
                (Vec3 basis, out Vec3 result) =>
                {
                    result = basis;
                }), vinfoCopier);
        }
        // clone a model and transform its verts and vinfos (scraps any WIP intersection info)
        public CSGModel CloneAndTransform(ModelUtil.CopyAndTransformVert vertCopier, ModelUtil.CopyAndTransformVInfo vinfoCopier)
        {
            CSGModel result = new CSGModel();

            Dictionary<long, CSGVertex> vertexMapping = new Dictionary<long, CSGVertex>();
            Dictionary<long, CSGEdge> edgeMapping = new Dictionary<long, CSGEdge>();
            Dictionary<long, CSGShape> shapeMapping = new Dictionary<long, CSGShape>();
            Dictionary<long, CSGSourceTriangle> triangleMapping = new Dictionary<long, CSGSourceTriangle>();

            foreach (CSGVertex vert in vertices)
            {
                CSGVertex v = new CSGVertex();
                vertexMapping[vert.id] = v;
                result.vertices.Add(v);
            }
            foreach (CSGEdge edge in edges)
            {
                CSGEdge e = new CSGEdge();
                edgeMapping[edge.id] = e;
                result.edges.Add(e);
            }
            foreach (CSGShape shape in shapes)
            {
                CSGShape s = new CSGShape();
                shapeMapping[shape.id] = s;
                result.shapes.Add(s);
            }
            foreach (CSGSourceTriangle triangle in sourceTriangles)
            {
                CSGSourceTriangle t = new CSGSourceTriangle();
                triangleMapping[triangle.id] = t;
                result.sourceTriangles.Add(t);
            }

            foreach (CSGVertex iVertex in vertices)
            {
                CSGVertex rVertex = vertexMapping[iVertex.id];
                if(iVertex.parentTriangle != null)
                    rVertex.parentTriangle = triangleMapping[iVertex.parentTriangle.id];
                vertCopier(iVertex.position, out rVertex.position);
                foreach(CSGEdge edge in iVertex.neighbors)
                    rVertex.neighbors.Add(edgeMapping[edge.id]);
            }

            foreach (CSGEdge iEdge in edges)
            {
                CSGEdge rEdge = edgeMapping[iEdge.id];
                if(iEdge.parentTriangle != null)
                    rEdge.parentTriangle = triangleMapping[iEdge.parentTriangle.id];
                for (int i = 0; i < 2; i++)
                {
                    rEdge.endpoints[i] = vertexMapping[iEdge.endpoints[i].id];
                    if (iEdge.separatedShapes[i] != null)
                        rEdge.separatedShapes[i] = shapeMapping[iEdge.separatedShapes[i].id];
                }
            }

            foreach (CSGSourceTriangle iTriangle in sourceTriangles)
            {
                CSGSourceTriangle rTriangle = triangleMapping[iTriangle.id];
                for (int i = 0; i < 3; i++)
                    rTriangle.sourceVerts[i] = vertexMapping[iTriangle.sourceVerts[i].id];
                foreach (long key in iTriangle.vertexVInfos.Keys)
                {
                    VInfo vinfo;
                    vinfoCopier(iTriangle.vertexVInfos[key], out vinfo);
                    rTriangle.vertexVInfos[key] = vinfo;
                }
            }

            foreach (CSGShape iShape in shapes)
            {
                CSGShape rShape = shapeMapping[iShape.id];
                rShape.parentTriangle = triangleMapping[iShape.parentTriangle.id];
                foreach (CSGVertex vert in iShape.boundary)
                    rShape.boundary.Add(vertexMapping[vert.id]);
                foreach (List<CSGVertex> iHole in iShape.holes)
                {
                    List<CSGVertex> rHole = new List<CSGVertex>();
                    foreach (CSGVertex vert in iHole)
                        rHole.Add(vertexMapping[vert.id]);
                    rShape.holes.Add(rHole);
                }
                foreach (CSGEdge iEdge in iShape.edges)
                    rShape.edges.Add(edgeMapping[iEdge.id]);
            }

            return result;
        }

        // Computes the AABB... doesn't store it anywhere, that's up to you. Use the return value wisely
        public AABB ComputeAABB()
        {
            return AABB.FitPointList(vertices.ConvertAll<Vec3>((vert) => vert.position));
        }

        public IEnumerable<Octree.Item> GetSourceTriangleOctreeData()
        {
            foreach (CSGSourceTriangle tri in sourceTriangles)
                yield return tri.ToOctreeItem();
        }
    }
}
