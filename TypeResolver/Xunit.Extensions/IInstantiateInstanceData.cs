
using System;


namespace Xunit.Extensions {

    /// <summary>
    /// Acts as a type that assists with instantiating instance data.
    /// </summary>
    /// <seealso cref="InstantiateInstanceDataAttribute"/>
    public interface IInstantiateInstanceData {

        /// <summary>
        /// Performs the invocation of the specified instance data instantiation method.
        /// </summary>
        /// <param name="instantiate">An instance data instantiation method.</param>
        /// <param name="arguments">The arguments to the instantiation method.</param>
        /// <returns>The instantiated instance data.</returns>
        T Invoke<T>( Func<object[], T> instantiate, object[] arguments );

    }

}
