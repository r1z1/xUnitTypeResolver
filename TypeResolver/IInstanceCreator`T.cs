
using System;


namespace TypeResolver {

    /// <summary>
    /// Represents an object that can create <typeparamref name="T"/> object instances.
    /// </summary>
    public interface IInstanceCreator<out T> : IInstanceCreator {

        /// <summary>
        /// Creates an instance of type <typeparamref name="T"/>.
        /// </summary>
        new T CreateInstance( );

    }

}
