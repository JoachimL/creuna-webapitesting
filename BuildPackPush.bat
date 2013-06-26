msbuild Build.proj
.nuget\nuget pack Creuna.WebApiTesting\Creuna.WebApiTesting.csproj -Prop Configuration=Release
.nuget\nuget push Creuna.WebApiTesting.1.0.0.1.nupkg