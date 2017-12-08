
using System;


namespace TypeResolver {

    /// <summary>
    /// Represents an object that can create instances of a particular object.
    /// </summary>
    public interface IInstanceCreator {

        /// <summary>
        /// Returns the type of object created by the <see cref="CreateInstance"/> method.
        /// </summary>
        Type InstanceType { get; }

        /// <summary>
        /// Creates an instance of the <see cref="InstanceType"/>.
        /// </summary>
        object CreateInstance( );

    }

}
