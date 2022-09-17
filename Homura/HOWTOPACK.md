
# How to pack

binフォルダとobjフォルダは事前に削除します。

```
dotnet clean
dotnet build
dotnet pack Homura.csproj -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg /p:PackageVersion=3.5.8 -c Release
```