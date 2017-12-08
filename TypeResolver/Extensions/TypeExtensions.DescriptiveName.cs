
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using TypeResolver.Internal;


namespace TypeResolver.Extensions {

    /// <summary>
    /// Contains useful extensions for dealing with <see cref="Type"/>s.
    /// </summary>
    public static partial class TypeExtensions {

        private sealed class DescriptiveName {

            private readonly StringBuilder sb_ = new StringBuilder( );


            public DescriptiveName( Type type ) {
                Debug.Assert( type != null );

                if( type.IsGenericParameter ) {
                    this.sb_.Append( type.Name );
                    this.sb_.Append( " on " );
                    if( type.DeclaringMethod != null )
                        this.AppendMethodName( type.DeclaringMethod );
                    else
                        this.AppendTypeName( type.DeclaringType );
                }
                else {
                    this.AppendTypeName( type );
                }
            }

            public DescriptiveName( MethodBase method ) {
                Debug.Assert( method != null );

                this.AppendMethodName( method );
            }


            public override string ToString( ) {
                return this.sb_.ToString( );
            }


            private void AppendTypeName( Type type ) {
                this.sb_.Append( type.Name.Split( '`' )[0] );
                this.AppendGenericArguments( type.GetGenericArguments( ) );
            }

            private void AppendMethodName( MethodBase methodBase ) {
                Debug.Assert( methodBase.Is<MethodInfo>( ) || methodBase.Is<ConstructorInfo>( ) );

                var method = methodBase as MethodInfo;
                if( method != null ) {
                    this.sb_.Append( method.Name );
                    if( methodBase.ContainsGenericParameters )
                        this.AppendGenericArguments( methodBase.GetGenericArguments( ) );
                }
                else {
                    var constructor = (ConstructorInfo)methodBase;
                    this.sb_.Append( "new " );
                    this.AppendTypeName( constructor.DeclaringType );
                }

                var parameterTypes = methodBase.GetParameters( ).Select( ( param ) => param.ParameterType );
                this.sb_.Append( "( " );
                parameterTypes.Join( ", ", this.sb_, this.AppendTypeName );
                this.sb_.Append( " )" );

                if( method != null && method.ReturnType != typeof( void ) ) {
                    this.sb_.Append( " : " );
                    this.AppendTypeName( method.ReturnType );
                }
            }

            private void AppendGenericArguments( Type[] genericArguments ) {
                if( genericArguments.Length > 0 ) {
                    this.sb_.Append( '<' );
                    genericArguments.Join( ",", this.sb_, this.AppendTypeName );
                    this.sb_.Append( '>' );
                }
            }
        }

    }

}
