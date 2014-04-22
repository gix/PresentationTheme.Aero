namespace StyleCore
{
    using System.Collections;
    using System.Collections.Generic;

    public class IntList : IReadOnlyList<int>
    {
        private readonly int[] values;

        public IntList(int[] values)
        {
            this.values = values;
        }

        public int Count => values.Length;
        public int this[int index] => values[index];

        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public override string ToString()
        {
            return string.Join(", ", values);
        }
    }
}
