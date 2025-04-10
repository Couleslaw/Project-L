all: ProjectLCore/bin/Release/net8.0/ProjectLCore.dll AIPlayerExample/bin/Release/net8.0/AIPlayerExample.dll ProjectLCoreDocs/Help AIPlayerExampleDocs/Help AIPlayerSimulation/AIPlayerDLLs/AIPlayerExample.dll

clean:
	rm -rf ProjectLCore/bin AIPlayerExample/bin ProjectLCoreDocs/Help AIPlayerExampleDocs/Help AIPlayerSimulation/AIPlayerDLLs/AIPlayerExample.dll

ProjectLCore/bin/Release/net8.0/ProjectLCore.dll: ProjectLCore/ProjectLCore.csproj
	dotnet build -c Release ProjectLCore/ProjectLCore.csproj

AIPlayerExample/bin/Release/net8.0/AIPlayerExample.dll: AIPlayerExample/AIPlayerExample.csproj
	dotnet build -c Release AIPlayerExample/AIPlayerExample.csproj

ProjectLCoreDocs/Help: ProjectLCoreDocs/ProjectLCoreDocs.shfbproj ProjectLCore/bin/Release/net8.0/ProjectLCore.dll
	msbuild.exe ProjectLCoreDocs/ProjectLCoreDocs.shfbproj

AIPlayerExampleDocs/Help: AIPlayerExampleDocs/AIPlayerExampleDocs.shfbproj ProjectLCore/bin/Release/net8.0/ProjectLCore.dll AIPlayerExample/bin/Release/net8.0/AIPlayerExample.dll
	msbuild.exe AIPlayerExampleDocs/AIPlayerExampleDocs.shfbproj

