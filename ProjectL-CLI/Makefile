all: ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll SimpleAIPlayer/bin/Release/netstandard2.1/SimpleAIPlayer.dll ProjectLCoreDocs/Help SimpleAIPlayerDocs/Help

clean:
	rm -rf ProjectLCore/bin SimpleAIPlayer/bin ProjectLCoreDocs/Help SimpleAIPlayerDocs/Help

ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll: ProjectLCore/ProjectLCore.csproj
	dotnet build -c Release ProjectLCore/ProjectLCore.csproj

SimpleAIPlayer/bin/Release/netstandard2.1/SimpleAIPlayer.dll: SimpleAIPlayer/SimpleAIPlayer.csproj
	dotnet build -c Release SimpleAIPlayer/SimpleAIPlayer.csproj

ProjectLCoreDocs/Help: ProjectLCoreDocs/ProjectLCoreDocs.shfbproj ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll
	msbuild.exe ProjectLCoreDocs/ProjectLCoreDocs.shfbproj

SimpleAIPlayerDocs/Help: SimpleAIPlayerDocs/SimpleAIPlayerDocs.shfbproj ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll SimpleAIPlayer/bin/Release/netstandard2.1/SimpleAIPlayer.dll
	msbuild.exe SimpleAIPlayerDocs/SimpleAIPlayerDocs.shfbproj

