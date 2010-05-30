namespace Modelthulhu.UserModel
{
    // Basic model data class
    // Contains 3-component verts, 2-component texture coords, and 3-component vertex normals; each triangle has 3 of each
    public class BasicModelData
    {
        // parallel vertex attrib arrays
        public double[] x, y, z;                // vertices
        public double[] u, v;                   // texture coordinates
        public double[] nx, ny, nz;             // normal vectors
        // triangle vertex attrib indices
        public uint[] a_vert, a_uv, a_norm, b_vert, b_uv, b_norm, c_vert, c_uv, c_norm;
    }
}