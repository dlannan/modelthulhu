using System;
using System.Collections.Generic;
using System.Text;

namespace TheLibrary.CSG
{
    public class SparseBoolMatrix
    {
        protected SortedList<long, Column> columns = new SortedList<long,Column>();

        protected class Column
        {
            public Column(long x) { this.x = x; }

            protected long x;
            protected HashSet<long> Y = new HashSet<long>();

            public bool this[long y]
            {
                get
                {
                    return Y.Contains(y);
                }
                set
                {
                    if (value) 
                        Y.Add(y);
                    else                    
                        Y.Remove(y);
                }
            }

            public void ForAllPairs(PairCallback callback)
            {
                foreach (long y in Y)
                    callback(x, y);
            }
        }

        public bool this[long x, long y]
        {
            get
            {
                Column column = columns[x];
                if (column == null)
                    return false;
                return column[y];
            }
            set
            {
                Column column;
                if (!columns.TryGetValue(x, out column))
                    column = null;
                if (value)
                {
                    if (column == null)
                    {
                        column = new Column(x);
                        columns.Add(x, column);
                    }
                    column[y] = true;
                }
                else
                    if (column != null)
                        column[y] = false;
            }
        }
        
        // Delegate function... let's you do something with a pair of items
        public delegate void PairCallback(long first, long second);

        public void ForAllPairs(PairCallback callback)
        {
            foreach (Column col in columns.Values)
                col.ForAllPairs(callback);
        }
    }
}
