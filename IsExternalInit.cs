// Required for 'init' accessor support on net48 (C# 9 feature not in .NET Framework BCL)
#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
