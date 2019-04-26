namespace CopperBend.Fabric
{
    public class DirectedEdge
    {
        public DirectedEdge(int from, int to, double weight)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        public int From { get; private set; }

        public int To { get; private set; }

        public double Weight { get; private set; }

        public override string ToString()
        {
            return string.Format("From: {0}, To: {1}, Weight: {2}", From, To, Weight);
        }
    }
}
