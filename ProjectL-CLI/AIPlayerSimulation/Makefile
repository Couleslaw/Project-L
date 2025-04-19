all:  AIPlayerDLLs/AIPlayerExample.dll bin/Release/net8.0/AIPlayerSimulation.exe

clean:
	rm -rf AIPlayerDLLs/AIPlayerExample.dll bin

AIPlayerDLLs/AIPlayerExample.dll: ../AIPlayerExample/bin/Release/netstandard2.1/AIPlayerExample.dll
	mkdir -p AIPlayerDLLs
	cp ../AIPlayerExample/bin/Release/netstandard2.1/AIPlayerExample.dll AIPlayerDLLs

bin/Release/net8.0/AIPlayerSimulation.exe: AIPlayerSimulation.csproj
	dotnet build -c Release AIPlayerSimulation.csproj