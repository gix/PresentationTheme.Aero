namespace ThemeBrowser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class CombinedList<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T>[] lists;

        public CombinedList(params IReadOnlyList<T>[] lists)
        {
            if (lists == null)
                throw new ArgumentNullException(nameof(lists));
            if (lists.Any(x => x == null))
                throw new ArgumentException("Sub lists must not be null.", nameof(lists));

            this.lists = lists.ToArray();
        }

        public int Count => lists.Sum(x => x.Count);

        public T this[int index]
        {
            get
            {
                int firstIndex = 0;
                for (int i = 0; i < lists.Length; ++i) {
                    int realIndex = index - firstIndex;
                    var list = lists[i];
                    if (realIndex < list.Count)
                        return list[realIndex];
                    firstIndex += list.Count;
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var list in lists)
                foreach (var item in list)
                    yield return item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}