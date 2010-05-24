using System;
using System.Collections.Generic;
using System.Text;

using TheLibrary.Math3D;

namespace TheLibrary.CSG
{
    public class CutTriangle
    {
        public int objID;

        public int[] originalVerts = new int[3];
        public List<int>[] edgeCutVerts = new List<int>[] { new List<int>(), new List<int>(), new List<int>() };
        public List<int> centerVerts = new List<int>();

        public List<int[]>[] cutEdges = new List<int[]>[] { new List<int[]>(), new List<int[]>(), new List<int[]>() };        // cuts from the original edges
        public List<int[]> slicedEdges = new List<int[]>();                 // cuts across the triangle

        public List<WorkingVertex> allVerts = new List<WorkingVertex>();

        public CutTriangle(WorkingTriangle basis)
        {
            objID = basis.objID;

            List<int> otherTriangles = new List<int>();
            List<List<int>> vertsInOtherTriangles = new List<List<int>>();

            // copying original triangle's 3 verts
            for (int i = 0; i < 3; i++)
                originalVerts[i] = AddVertex(basis.verts[i]);
            // finding verts formed by cutting original triangle's 3 edges
            for (int i = 0; i < 3; i++)
            {
                WorkingEdge edge = basis.edges[i];
                Vec3 direction = basis.verts[(i + 1) % 3].position.xyz - basis.verts[i].position.xyz;
                List<int> verts = new List<int>();
                List<double> dots = new List<double>();
                List<int> indices = new List<int>();
                verts.Add(originalVerts[i]);
                dots.Add(Vec3.Dot(direction, basis.verts[i].position.xyz));
                indices.Add(indices.Count);
                foreach (EdgeIntersection x in edge.intersections)
                {
                    WorkingVertex v = new WorkingVertex();
                    v.position = x.position;
                    v.vinfo = basis.InterpolateVInfos(x.position.xyz);
                    int v_index = AddVertex(v);
                    v = allVerts[v_index];
                    edgeCutVerts[i].Add(v_index);
                    verts.Add(v_index);
                    dots.Add(Vec3.Dot(direction, v.position.xyz));
                    indices.Add(indices.Count);

                    int index = otherTriangles.IndexOf(x.triangle.triID);
                    if (index == -1)
                    {
                        otherTriangles.Add(x.triangle.triID);
                        index = vertsInOtherTriangles.Count;
                        vertsInOtherTriangles.Add(new List<int>());
                    }
                    vertsInOtherTriangles[index].Add(v_index);
                }
                verts.Add(originalVerts[(i + 1) % 3]);
                dots.Add(Vec3.Dot(direction, basis.verts[(i + 1) % 3].position.xyz));
                indices.Add(indices.Count);
                // sorting the cut edges
                indices.Sort((a, b) => (int)Math.Sign(dots[a] - dots[b]));
                for (int j = 1; j < indices.Count; j++)
                    cutEdges[i].Add(new int[] { verts[indices[j - 1]], verts[indices[j]] });
            }

            foreach (EdgeIntersection x in basis.otherObjectEdgeIntersections)
            {
                WorkingVertex v = new WorkingVertex();
                v.position = x.position;
                v.vinfo = basis.InterpolateVInfos(x.position.xyz);
                int v_index = AddVertex(v);
                v = allVerts[v_index];
                centerVerts.Add(v_index);

                foreach (WorkingTriangle tri in x.edge.triangles)
                {
                    if (tri == null)
                        continue;
                    int index = otherTriangles.IndexOf(tri.triID);
                    if (index == -1)
                    {
                        otherTriangles.Add(tri.triID);
                        index = vertsInOtherTriangles.Count;
                        vertsInOtherTriangles.Add(new List<int>());
                    }
                    else
                    { }
                    vertsInOtherTriangles[index].Add(v_index);
                }
            }

            foreach (List<int> edgeVerts in vertsInOtherTriangles)
                if (edgeVerts.Count == 2)
                    slicedEdges.Add(new int[] { edgeVerts[0], edgeVerts[1] });
        }

        public List<WorkingVertex> GetCutTriangleVerts()
        {
            // pre-compute the plane of the triangle and some related quantities
            Vec3[] vertexPositions = new Vec3[] { allVerts[originalVerts[0]].position.xyz, allVerts[originalVerts[1]].position.xyz, allVerts[originalVerts[2]].position.xyz };
            Plane trianglePlane = Plane.FromTriangleVertices(vertexPositions[0], vertexPositions[1], vertexPositions[2]);
            Vec3 normal = trianglePlane.normal;

            int numVerts = allVerts.Count;

            List<int>[] typedVerts = new List<int>[5];
            typedVerts[0] = new List<int>(originalVerts);
            for (int i = 0; i < 3; i++)
                typedVerts[i + 1] = new List<int>(edgeCutVerts[i]);
            typedVerts[4] = new List<int>(centerVerts);

            int[] vertexTypes = new int[numVerts];
            for (int i = 0; i < typedVerts.Length; i++)
            {
                List<int> noRepeats = new List<int>();
                foreach (int vert in typedVerts[i])
                {
                    if (!noRepeats.Contains(vert))
                    {
                        vertexTypes[vert] = i;
                        noRepeats.Add(vert);
                    }
                }
                typedVerts[i] = noRepeats;
            }

            List<int[]> existingEdges = new List<int[]>();                  // get a list of all of the existing edges
            foreach (List<int[]> edgeList in cutEdges)                      // this includes the cut fragments of the original triangle's edges
                existingEdges.AddRange(edgeList);
            existingEdges.AddRange(slicedEdges);                            // it also includes the edges that were cut across the face of the original triangle

            foreach (int[] edge in existingEdges)                           // make  sure that their verts in ascending order
                if (edge[0] > edge[1])
                {
                    int temp = edge[0];
                    edge[0] = edge[1];
                    edge[1] = temp;
                }

            // get a list of all of the possible edges (note that their verts are listed in ascending order)
            List<int[]> candidateEdges = new List<int[]>();
            for (int i = 0; i < numVerts; i++)
            {
                switch (vertexTypes[i])
                {
                    case 0:
                        {
                            // type-0 verts can only connect to type 4 verts and whichever of the types 1-3 corresponds to the opposite edge
                            // the (n+1)%3 is there in order to get the opposite edge for vert[0] is the one between vert[1] and vert[2], i.e. edge[1], etc.
                            int id = (new List<int>(originalVerts).IndexOf(i) + 1) % 3;
                            for (int j = 0; j < numVerts; j++)
                                if (vertexTypes[j] == id + 1 || vertexTypes[j] == 4)
                                    candidateEdges.Add(new int[] { i, j });
                            break;
                        }
                    case 1:
                        {
                            // type-1 verts have already been matched up with the one applicable type-0 vert,
                            // but they can still connect to type-2, -3, and -4 verts
                            for (int j = 0; j < numVerts; j++)
                                if (vertexTypes[j] > 1)
                                    candidateEdges.Add(new int[] { i, j });
                            break;
                        }
                    case 2:
                        {
                            // type-2 verts have already been matched up with the one applicable type-0 vert and with the type-1 verts,
                            // but they can still connect to type-3 and -4 verts
                            for (int j = 0; j < numVerts; j++)
                                if (vertexTypes[j] > 2)
                                    candidateEdges.Add(new int[] { i, j });
                            break;
                        }
                    case 3:
                        {
                            // type-3 verts have already been matched up with the one applicable type-0 vert and with the type-1 and -2 verts,
                            // but they can still connect to type-4 verts
                            for (int j = 0; j < numVerts; j++)
                                if (vertexTypes[j] > 3)
                                    candidateEdges.Add(new int[] { i, j });
                            break;
                        }
                    case 4:
                        {
                            // type-4 verts have already been matched up with everything but the other type-4 verts
                            // they are the only category which can have connections to other verts of the same type
                            for (int j = 0; j < numVerts; j++)
                                if (vertexTypes[j] == 4 && i < j)
                                    candidateEdges.Add(new int[] { i, j });
                            break;
                        }
                }
            }

            candidateEdges.RemoveAll(
                (edge) => existingEdges.Exists(
                    (present) =>
                    {
                        if (edge[0] == present[0] && edge[1] == present[1])
                            return true;                        // invalidate the candidate if it's one of the existing edges
                        else if (edge[0] == present[0] || edge[0] == present[1] || edge[1] == present[0] || edge[1] == present[1])
                            return false;                       // if it shares one but not both verts, it's safe
                        // find the endpoints of these two edges
                        Vec2 a1 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[edge[0]].position.xyz);
                        Vec2 a2 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[edge[1]].position.xyz);
                        Vec2 b1 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[present[0]].position.xyz);
                        Vec2 b2 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[present[1]].position.xyz);
                        // invalidate the candidate if it intersects an existing edge
                        return Util.LineSegmentIntersection2D(a1, a2, b1, b2);
                    }));

            // keep cutting until no more cuts are available (all having been either used or invalidated)
            while (candidateEdges.Count != 0)
            {
                // select a random edge and move it from the candidate edges list to the existing edges list
                //int randomSelection = Random3D.RandInt(candidateEdges.Count);   // choose from the available cuts at random,
                int randomSelection = 0;
                int[] selectedEdge = candidateEdges[randomSelection];           // get the selected cut,
                existingEdges.Add(selectedEdge);                                // add it to the list of existing cuts,
                candidateEdges.RemoveAt(randomSelection);                       // and remove it from the candidate list

                // now eliminate any cuts which got invalidated by that selection,
                candidateEdges.RemoveAll(
                    (maybeInvalidEdge) =>
                    {
                        // if an edge shares a vert with the selected edge, the only place they might intersect is at that vert,
                        // so the edge is not invalidated
                        if (maybeInvalidEdge[0] == selectedEdge[0] || maybeInvalidEdge[0] == selectedEdge[1] || maybeInvalidEdge[1] == selectedEdge[0] || maybeInvalidEdge[1] == selectedEdge[1])
                            return false;

                        // find the endpoints of these two edges
                        Vec2 a1 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[selectedEdge[0]].position.xyz);
                        Vec2 a2 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[selectedEdge[1]].position.xyz);
                        Vec2 b1 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[maybeInvalidEdge[0]].position.xyz);
                        Vec2 b2 = Util.VectorToTriangleCoords(vertexPositions, normal, allVerts[maybeInvalidEdge[1]].position.xyz);

                        // invalidate the other one if they intersect; otherwise it's safe
                        return Util.LineSegmentIntersection2D(a1, a2, b1, b2);
                    });
            }

            // find out where the triangles are in that mess of edges
            List<Vec2> flattenedVerts = new List<Vec2>();
            foreach (WorkingVertex vert in allVerts)
                flattenedVerts.Add(Util.VectorToTriangleCoords(vertexPositions, normal, vert.position.xyz));

            // make a lookup table for which verts have edges between 'em
            // quick-and-dirty adaptation of the older version
            bool[,] cutTable = new bool[numVerts, numVerts];
            foreach (int[] edge in existingEdges)
                cutTable[edge[0], edge[1]] = cutTable[edge[1], edge[0]] = true;

            List<int[]> triangles = GetAtomicTriangleList(cutTable, flattenedVerts);

            // now look up those indices and shove them into a big list
            List<WorkingVertex> result = new List<WorkingVertex>();
            foreach (int[] triangle in triangles)
            {
                List<WorkingVertex> triVerts = new List<WorkingVertex>();
                List<Vec3> triVertPositions = new List<Vec3>();
                foreach (int index in triangle)
                {
                    triVerts.Add(allVerts[index]);
                    triVertPositions.Add(allVerts[index].position.xyz);
                }
                if (Vec3.Dot(Plane.FromTriangleVertices(triVertPositions[0], triVertPositions[1], triVertPositions[2]).normal, normal) > 0.0)
                {
                    result.Add(triVerts[0]);
                    result.Add(triVerts[1]);
                    result.Add(triVerts[2]);
                }
                else
                {
                    result.Add(triVerts[2]);
                    result.Add(triVerts[1]);
                    result.Add(triVerts[0]);
                }
            }

            return result;
        }

        // Get a list of atomic triangles
        protected List<int[]> GetAtomicTriangleList(bool[,] cutTable, List<Vec2> verts)
        {
            int numVerts = verts.Count;

            // find all the possible triangles
            List<int[]> trios = new List<int[]>();
            // there will be 3 edges which connect back to the first vertex
            for (int first = 0; first < numVerts; first++)
                for (int second = 0; second < numVerts; second++)
                    if (cutTable[first, second] || cutTable[second, first])
                        for (int third = 0; third < numVerts; third++)
                            if ((cutTable[second, third] || cutTable[third, second]) && third != first)
                                if (cutTable[third, first] || cutTable[first, third])             // check whether the third vertex connects back to the first; if it does, it's a triangle
                                {
                                    bool unique = true;
                                    int[] indices = new int[] { first, second, third };
                                    foreach (int[] existingTrio in trios)
                                    {
                                        int matches = 0;
                                        for (int i = 0; i < 3; i++)
                                            for (int j = 0; j < 3; j++)
                                                if (existingTrio[i] == indices[j])
                                                {
                                                    matches++;
                                                    break;
                                                }
                                        if (matches == 3)
                                        {
                                            unique = false;
                                            break;
                                        }
                                    }
                                    if (unique)
                                        trios.Add(indices);
                                }

            // figure out which triangles are atomic, and return only those
            trios.RemoveAll((trio) => !IsTriangleAtomic(trio, verts));
            return trios;
        }

        // Figure out whether a created triangle is atomic or not
        protected bool IsTriangleAtomic(int[] triangleVertexIndices, List<Vec2> verts)
        {
            int numVerts = verts.Count;
            Vec2 vertexA = verts[triangleVertexIndices[0]], vertexB = verts[triangleVertexIndices[1]], vertexC = verts[triangleVertexIndices[2]];
            double ax = vertexA.x, ay = vertexA.y;
            double bx = vertexB.x, by = vertexB.y;
            double cx = vertexC.x, cy = vertexC.y;
            double abx = bx - ax, aby = by - ay, bcx = cx - bx, bcy = cy - by, cax = ax - cx, cay = ay - cy;
            double abNx = -aby, abNy = abx, bcNx = -bcy, bcNy = bcx, caNx = -cay, caNy = cax;
            double abLength = Math.Sqrt(abx * abx + aby * aby);
            double bcLength = Math.Sqrt(bcx * bcx + bcy * bcy);
            double caLength = Math.Sqrt(cax * cax + cay * cay);

            if (abLength == 0.0 || bcLength == 0.0 || caLength == 0.0)
                return false;

            double invAbLength = 1.0 / abLength;
            double invBcLength = 1.0 / bcLength;
            double invCaLength = 1.0 / caLength;
            abNx *= invAbLength;
            abNy *= invAbLength;
            bcNx *= invBcLength;
            bcNy *= invBcLength;
            caNx *= invCaLength;
            caNy *= invCaLength;
            double abOffset = abNx * ax + abNy * ay;
            double bcOffset = bcNx * bx + bcNy * by;
            double caOffset = caNx * cx + caNy * cy;
            double centerX = (ax + bx + cx) / 3.0, centerY = (ay + by + cy) / 3.0;
            int badSignAB = (int)Math.Sign((abNx * centerX + abNy * centerY) - abOffset);
            int badSignBC = (int)Math.Sign((bcNx * centerX + bcNy * centerY) - bcOffset);
            int badSignCA = (int)Math.Sign((caNx * centerX + caNy * centerY) - caOffset);
            for (int i = 0; i < numVerts; i++)
                if (i != triangleVertexIndices[0] && i != triangleVertexIndices[1] && i != triangleVertexIndices[2])
                {
                    Vec2 pos = verts[i];
                    double posX = pos.x, posY = pos.y;
                    if (Math.Sign((posX * abNx + posY * abNy) - abOffset) == badSignAB)
                        if (Math.Sign((posX * bcNx + posY * bcNy) - bcOffset) == badSignBC)
                            if (Math.Sign((posX * caNx + posY * caNy) - caOffset) == badSignCA)
                                return false;
                }
            return true;
        }


        // adds the specified vert if it is not yet present, and returns the new vert's index
        // otherwise returns the index of the existing vert it's a duplicate of
        public int AddVertex(WorkingVertex vert)
        {
            int index = allVerts.FindIndex((v) => (v.position.xyz - vert.position.xyz).ComputeMagnitudeSquared() < 0.0000000001);
            if (index == -1)
            {
                allVerts.Add(vert);
                return allVerts.Count - 1;
            }
            else
                return index;
        }
    }
}