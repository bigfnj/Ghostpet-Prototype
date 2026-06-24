# Produces a single-file GhostPet.exe in ./dist
# Requires .NET 10 runtime on the target machine (not self-contained)
dotnet publish GhostPet\GhostPet.csproj `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -o dist
