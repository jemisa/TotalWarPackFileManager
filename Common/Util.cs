using System;
using System.Collections.Generic;

namespace Common {
    /*
     * An Enumerator that relies on other enumerators to provide actual values.
     * This implementation will provide the functionality to enumerate the enumerators
     * to retrieve the values from them; subclasses will need to provide the contained
     * enumerators by implementing the NextEnumerator() method to provide actual
     * values (of type <T>).
     */
    public abstract class DelegatingEnumerator<T> : IEnumerator<T>, IDisposable {
        IEnumerator<T> currentEnumerator;
        public T Current {
            get {
                return currentEnumerator.Current;
            }
        }
        object System.Collections.IEnumerator.Current {
            get {
                return Current;
            }
        }
        public bool MoveNext() {
            bool result = true;
            if (currentEnumerator == null || !currentEnumerator.MoveNext()) {
                currentEnumerator = NextEnumerator();
                result = currentEnumerator != null && currentEnumerator.MoveNext();
            }
            return result;
        }
        public virtual void Reset() {
            if (currentEnumerator != null) {
                currentEnumerator.Dispose();
                currentEnumerator = null;
            }
        }
        protected abstract IEnumerator<T> NextEnumerator();

        public virtual void Dispose() {
            if (currentEnumerator != null) {
                currentEnumerator.Dispose();
            }
        }
    }
}
