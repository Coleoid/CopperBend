namespace CopperBend.Creation
{
    public struct LateDependency<T>
        where T : class
    {
        private T value;

        public T Value
        {
            get
            {
                if (value == null)
                    value = SourceMe.The<T>();
                return value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}
