all:  AIPlayerDLLs/SimpleAIPlayer.dll bin/Release/net8.0/AIPlayerSimulation.exe

clean:
	rm -rf AIPlayerDLLs/SimpleAIPlayer.dll bin

AIPlayerDLLs/SimpleAIPlayer.dll: ../SimpleAIPlayer/bin/Release/netstandard2.1/SimpleAIPlayer.dll
	mkdir -p AIPlayerDLLs
	cp ../SimpleAIPlayer/bin/Release/netstandard2.1/SimpleAIPlayer.dll AIPlayerDLLs

bin/Release/net8.0/AIPlayerSimulation.exe: AIPlayerSimulation.csproj
	dotnet build -c Release AIPlayerSimulation.csproj