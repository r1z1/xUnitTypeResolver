
using System.Collections.Generic;
using System.Diagnostics;


namespace TypeResolver.Internal {

    /// <summary>
    /// Represents a queue of items to be visited.
    /// </summary>
    /// <remarks>
    /// The collection can be updated while it is being enumerated.
    /// Items can only be added once (duplicate items are ignored).
    /// </remarks>
    internal sealed class VisitorQueue<T> : IEnumerable<T>
        where T : class {

        private readonly List<T> items_ = new List<T>( );


        public VisitorQueue( ) { }

        public VisitorQueue( IEnumerable<T> items ) {
            this.AddRange( items );
        }


        public void Add( T item ) {
            Debug.Assert( item != null );

            if( !this.items_.Contains( item ) )
                this.items_.Add( item );
        }

        public void AddRange( IEnumerable<T> items ) {
            items.ForEach( this.Add );
        }


        public override string ToString( ) {
            return "{" + this.Join( ", " ) + "}";
        }


        #region IEnumerable Members

        public IEnumerator<T> GetEnumerator( ) {
            return new VisitorQueueEnumerator( this );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) {
            return this.GetEnumerator( );
        }


        /// <summary>
        /// Custom enumerator allows the list to be modified while it is being traversed.
        /// </summary>
        private sealed class VisitorQueueEnumerator : IEnumerator<T> {
            private readonly List<T> items_;
            private const int InitialIndex = -1;
            private int currentIndex_;

            public VisitorQueueEnumerator( VisitorQueue<T> parent ) {
                Debug.Assert( parent != null );

                this.items_ = parent.items_;
                this.currentIndex_ = InitialIndex;
            }

            #region IEnumerator Members

            public T Current {
                get { return this.items_[this.currentIndex_]; }
            }

            object System.Collections.IEnumerator.Current {
                get { return this.Current; }
            }

            public bool MoveNext( ) {
                ++this.currentIndex_;

                return this.currentIndex_ < this.items_.Count;
            }

            public void Reset( ) {
                this.currentIndex_ = InitialIndex;
            }

            public void Dispose( ) { }

            #endregion
        }

        #endregion

    }

}
