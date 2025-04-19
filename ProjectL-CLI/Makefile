all: ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll AIPlayerExample/bin/Release/netstandard2.1/AIPlayerExample.dll ProjectLCoreDocs/Help AIPlayerExampleDocs/Help

clean:
	rm -rf ProjectLCore/bin AIPlayerExample/bin ProjectLCoreDocs/Help AIPlayerExampleDocs/Help

ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll: ProjectLCore/ProjectLCore.csproj
	dotnet build -c Release ProjectLCore/ProjectLCore.csproj

AIPlayerExample/bin/Release/netstandard2.1/AIPlayerExample.dll: AIPlayerExample/AIPlayerExample.csproj
	dotnet build -c Release AIPlayerExample/AIPlayerExample.csproj

ProjectLCoreDocs/Help: ProjectLCoreDocs/ProjectLCoreDocs.shfbproj ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll
	msbuild.exe ProjectLCoreDocs/ProjectLCoreDocs.shfbproj

AIPlayerExampleDocs/Help: AIPlayerExampleDocs/AIPlayerExampleDocs.shfbproj ProjectLCore/bin/Release/netstandard2.1/ProjectLCore.dll AIPlayerExample/bin/Release/netstandard2.1/AIPlayerExample.dll
	msbuild.exe AIPlayerExampleDocs/AIPlayerExampleDocs.shfbproj

