
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace TypeResolver {

    /// <summary>
    /// Represents a collection of arguments bindings.
    /// </summary>
    [DebuggerDisplay( "BindingCollection: Count={Count}" )]
    public sealed class BindingCollection : IEnumerable<LinkList<Binding>> {

        private static readonly IEnumerable<object> SingleItem = new object[1];

        private readonly List<LinkList<Binding>> bindings_;


        /// <summary>
        /// Initializes a new instance of the <see cref="BindingCollection"/> class.
        /// </summary>
        public BindingCollection( ) {
            this.bindings_ = new List<LinkList<Binding>>( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingCollection"/> class with the specified initial bindings.
        /// </summary>
        public BindingCollection( LinkList<Binding> bindings ) {
            Debug.Assert( bindings != null );
            this.bindings_ = new List<LinkList<Binding>> { bindings };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingCollection"/> class with the specified initial bindings.
        /// </summary>
        public BindingCollection( IEnumerable<LinkList<Binding>> bindings ) {
            Debug.Assert( bindings != null );
            this.bindings_ = new List<LinkList<Binding>>( bindings );
        }


        /// <summary>
        /// Gets the number of bindings in the collection.
        /// </summary>
        public int Count {
            get { return this.bindings_.Count; }
        }


        /// <summary>
        /// Removes all bindings from the collection.
        /// </summary>
        public void Clear( ) {
            this.bindings_.Clear( );
        }

        /// <summary>
        /// Adds the specified bindings to the collection.
        /// </summary>
        public void Add( LinkList<Binding> bindings ) {
            Debug.Assert( bindings != null );

            this.bindings_.Add( bindings );
        }

        /// <summary>
        /// Adds each of the specified bindings to the collection.
        /// </summary>
        public void AddRange( IEnumerable<LinkList<Binding>> bindings ) {
            Debug.Assert( bindings != null );

            var collection = bindings as BindingCollection;
            var sourceBindings = collection == null ? bindings : collection.bindings_;
            this.bindings_.AddRange( sourceBindings );
        }


        /// <summary>
        /// Reduces the collection of bindings with the results from the specified input.
        /// </summary>
        public void Reduce<T>( IEnumerable<T> input, Func<LinkList<Binding>, T, LinkList<Binding>> processor ) {
            Debug.Assert( input != null );
            Debug.Assert( processor != null );

            int nonNullCount = this.Count;
            foreach( T item in input ) {
                // Update each item in collection, compacting null results.
                int compactIndex = 0;
                int count = nonNullCount;
                for( int i = 0; i < count; ++i ) {
                    var bindings = this.bindings_[i];
                    var newBindings = processor( bindings, item );

                    this.bindings_[i] = null;
                    if( newBindings == null ) {
                        --nonNullCount;
                    }
                    else {
                        this.bindings_[compactIndex] = newBindings;
                        ++compactIndex;
                    }
                }

                if( nonNullCount == 0 )
                    break;
            }

            // Clear out null entries.
            if( nonNullCount < this.Count ) {
                int nullStart = nonNullCount;
                int nullCount = this.Count - nullStart;
                this.bindings_.RemoveRange( nullStart, nullCount );
            }
        }

        /// <summary>
        /// Reduces the collection of bindings.
        /// </summary>
        public void Reduce( Func<LinkList<Binding>, LinkList<Binding>> processor ) {
            Debug.Assert( processor != null );

            this.Reduce( SingleItem, ( b, _ ) => processor( b ) );
        }

        /// <summary>
        /// Reduces the collection of bindings with the results from the specified input.
        /// </summary>
        public void Reduce<T>( IEnumerable<T> input, Func<LinkList<Binding>, T, IEnumerable<LinkList<Binding>>> processor ) {
            Debug.Assert( input != null );
            Debug.Assert( processor != null );

            foreach( T item in input ) {
                // Add all new bindings to collection.
                int count = this.Count;
                for( int i = count - 1; i >= 0; --i ) {
                    var bindings = this.bindings_[i];
                    var newBindings = processor( bindings, item );

                    this.bindings_[i] = null;
                    this.AddRange( newBindings );
                }

                // Remove source bindings from collection.
                this.bindings_.RemoveRange( 0, count );
                if( this.bindings_.Count == 0 )
                    break;
            }
        }

        /// <summary>
        /// Reduces the collection of bindings.
        /// </summary>
        public void Reduce( Func<LinkList<Binding>, IEnumerable<LinkList<Binding>>> processor ) {
            Debug.Assert( processor != null );

            this.Reduce( SingleItem, ( b, _ ) => processor( b ) );
        }


        /// <summary>
        /// Expands the collection of bindings with the results from the specified input.
        /// </summary>
        public void Expand<T>( IEnumerable<T> input, Action<BindingCollection, T> processor ) {
            Debug.Assert( input != null );
            Debug.Assert( processor != null );

            // Save current set of bindings.
            var currentBindings = new LinkList<Binding>[this.bindings_.Count];
            this.bindings_.CopyTo( currentBindings );
            this.bindings_.Clear( );

            // Get new bindings from all input items.
            foreach( T item in input ) {
                BindingCollection newBindings = new BindingCollection( currentBindings );
                processor( newBindings, item );
                this.bindings_.AddRange( newBindings );
            }
        }


        /// <summary>
        /// Transforms the collection of bindings, excepting null and duplicate results.
        /// </summary>
        public HashSet<T> Transform<T>( Func<LinkList<Binding>, T> transformer )
            where T : class {
            IEnumerable<T> results = this.bindings_.Select( transformer ).Where( ( item ) => item != null );
            var set = new HashSet<T>( results );
            return set;
        }


        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public IEnumerator<LinkList<Binding>> GetEnumerator( ) {
            return this.bindings_.GetEnumerator( );
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) {
            return this.GetEnumerator( );
        }

        #endregion

    }

}
