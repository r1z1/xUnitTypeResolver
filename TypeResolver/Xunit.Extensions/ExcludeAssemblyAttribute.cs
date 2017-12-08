
using System;
using System.Diagnostics;
using System.Reflection;


namespace Xunit.Extensions {

    /// <summary>
    /// Indicates an <see cref="Assembly"/> that should be excluded from instance data creation.
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly, AllowMultiple = true )]
    public class ExcludeAssemblyAttribute : Attribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAssemblyAttribute"/> class with the specified assembly name.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to exclude.</param>
        public ExcludeAssemblyAttribute( string assemblyName ) {
            Debug.Assert( assemblyName != null );

            this.AssemblyName = assemblyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeAssemblyAttribute"/> class with the specified representative type.
        /// </summary>
        /// <param name="representativeType">A <see cref="Type"/> from the assembly to exclude.</param>
        public ExcludeAssemblyAttribute( Type representativeType ) {
            Debug.Assert( representativeType != null );

            this.AssemblyName = representativeType.Assembly.FullName;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> to limit.
        /// </summary>
        public string AssemblyName { get; private set; }

    }

}
