all: AIPlayerDLLs/AIPlayerExample.dll

clean:
	rm -rf AIPlayerDLLs/AIPlayerExample.dll

AIPlayerDLLs/AIPlayerExample.dll: ../AIPlayerExample/bin/Release/net8.0/AIPlayerExample.dll
	mkdir -p AIPlayerDLLs
	cp ../AIPlayerExample/bin/Release/net8.0/AIPlayerExample.dll AIPlayerDLLs
