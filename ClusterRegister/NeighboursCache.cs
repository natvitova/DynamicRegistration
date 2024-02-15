using Framework;

namespace ClusterRegister
{
    class NeighboursCache
    {
        int[][] known;        

        CornerTable ct;

        public NeighboursCache(CornerTable ct, int vertexCount)
        {
            this.ct = ct;
            known = new int[vertexCount][];            
        }
    }
}
