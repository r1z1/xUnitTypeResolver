
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TypeResolver.Internal;


namespace TypeResolver {

    /// <summary>
    /// An immutable linked list of <typeparamref name="T"/> items.
    /// </summary>
    public abstract partial class LinkList<T> : IEnumerable<T> {

        /// <summary>
        /// The empty list.
        /// </summary>
        public static readonly LinkList<T> Empty = new EmptyList( );


        /// <summary>
        /// Returns the number of items in the list.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Returns the value of the current list element.
        /// </summary>
        public abstract T Value { get; }

        /// <summary>
        /// Returns the tail of the current list element.
        /// </summary>
        public abstract LinkList<T> Tail { get; }

        /// <summary>
        /// Returns <see langword="true"/> if the list has no elements;
        /// otherwise, <see langword="false"/>.
        /// </summary>
        public bool IsEmpty {
            get { return this.Count == 0; }
        }


        /// <summary>
        /// Returns a new list with the specified value added to the start of the existing list.
        /// </summary>
        [DebuggerHidden]
        public LinkList<T> Add( T value ) {
            return new ValueList( value, this );
        }

        /// <summary>
        /// Converts the list of <typeparamref name="T"/> values to a <typeparamref name="T"/> array.
        /// </summary>
        public T[] ToArray( ) {
            T[] array = new T[this.Count];
            for( var list = this; !list.IsEmpty; list = list.Tail )
                array[list.Count - 1] = list.Value;
            return array;
        }

        /// <summary>
        /// Returns a <see cref="String"/> representation of the object.
        /// </summary>
        public override string ToString( ) {
            return "{" + this.Join( ", " ) + "}";
        }


        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator( ) {
            for( var list = this; !list.IsEmpty; list = list.Tail )
                yield return list.Value;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( ) {
            return this.GetEnumerator( );
        }

        #endregion

    }

}
