using System;
using System.Collections.Generic;
using System.Text;

namespace Modelthulhu
{
    public class MaxDepthOctree : Octree
    {
        public int maxDepth;

        public MaxDepthOctree(AABB aabb, int maxDepth):base(aabb)
        {
            this.maxDepth = maxDepth;
        }
        protected override bool ConditionallySubdivide()
        {
            return maxDepth > 0;
        }
        protected override Octree Subdivision(AABB aabb)
        {
            return new MaxDepthOctree(aabb, maxDepth - 1);
        }
    }
}
