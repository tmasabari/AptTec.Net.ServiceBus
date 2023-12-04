install nuget packages to use c#8 with standard 2.0 projects and .NET framework 4.8
 * "Microsoft.Bcl.AsyncInterfaces" Version="8.0.0"
 * "Microsoft.CSharp" Version="4.7.0"
 * Microsoft.Bcl.HashCode
 * <PropertyGroup>
 *  <LangVersion>latest</LangVersion>
 * </PropertyGroup>
	* https://dirkstrauss.com/enabling-c-8-in-visual-studio-2019/
	* if you explicitly want access to preview features https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version
	* there is a package from Microsoft that provides the IAsyncDisposable interface and related types for netstandard2.0 and .NET 4.6.1
	* https://alistairevans.co.uk/2019/10/24/net-asynchronous-disposal-tips-for-implementing-iasyncdisposable-on-your-own-types/