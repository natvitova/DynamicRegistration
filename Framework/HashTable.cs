using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public class HashTable
    {
        // large primes, used for hashing
        const int MAGIC1 = 100000007;
        const int MAGIC2 = 161803409;
        const int MAGIC3 = 423606823;
        const int NO_DATA = -1;
        double scale_;
        int[][] voxels_;
        int[] data_;

        public HashTable(int maxpoints, double voxel)
        {
            voxels_ = new int[maxpoints][];
            data_ = new int[maxpoints];
            scale_ = 1 / voxel;

            for (int i = 0; i < data_.Length; i++)
                data_[i] = NO_DATA;
        }

        int lastkey = 0;

        public void set(int value)
        {
            data_[lastkey] = value;
        }

        bool isSame(int[] c1, int[] c2)
        {
            if (c1[0] != c2[0])
                return false;
            if (c1[1] != c2[1])
                return false;
            if (c2[2] != c2[2])
                return false;
            return true;
        }

        public int this[Point3D p]
        {
            get
            {
                int c0 = (int)Math.Floor(p.X * scale_);
                int c1 = (int)Math.Floor(p.Y * scale_);
                int c2 = (int)Math.Floor(p.Z * scale_);
                int key = (MAGIC1 * c0 + MAGIC2 * c1 + MAGIC3 * c2) % data_.Length;
                int[] c = new int[] { c0, c1, c2 };
                if (key < 0)
                    key += data_.Length;
                while (true)
                {
                    if (data_[key] == NO_DATA)
                    {
                        voxels_[key] = c;
                        break;
                    }
                    else if (isSame(voxels_[key], c))
                    {
                        break;
                    }
                    key++;
                    if (key == data_.Length) key = 0;
                }
                lastkey = key;
                return data_[key];
            }
            set
            {
                int[] c = new int[3];
                c[0] = (int)Math.Floor(p.X * scale_);
                c[1] = (int)Math.Floor(p.Y * scale_);
                c[2] = (int)Math.Floor(p.Z * scale_);
                int key = (MAGIC1 * c[0] + MAGIC2 * c[1] + MAGIC3 * c[2]) % data_.Length;
                while (true)
                {
                    if (data_[key] == NO_DATA)
                    {
                        voxels_[key] = c;
                        break;
                    }
                    else if (voxels_[key] == c)
                    {
                        break;
                    }
                    key++;
                    if (key == data_.Length) key = 0;
                }
            }
        }
    }
}
