
# How to pack

binフォルダとobjフォルダは事前に削除します。

```
dotnet clean
dotnet build
dotnet pack /p:PackageVersion=3.4.0 -c Release
```