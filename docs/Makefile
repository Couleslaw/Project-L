all: ProjectLCoreDocs SimpleAIPlayerDocs TechnicalDocs/unity

clean:
	rm -rf ProjectLCoreDocs SimpleAIPlayerDocs TechnicalDocs/unity

ProjectLCoreDocs: ../ProjectL-CLI/ProjectLCoreDocs/Help
	cp -r ../ProjectL-CLI/ProjectLCoreDocs/Help ProjectLCoreDocs

SimpleAIPlayerDocs: ../ProjectL-CLI/SimpleAIPlayerDocs/Help
	cp -r ../ProjectL-CLI/SimpleAIPlayerDocs/Help SimpleAIPlayerDocs

TechnicalDocs/unity: ../Unity/Documentation/docfx.json
	docfx ../Unity/Documentation/docfx.json
	cp -r ../Unity/Documentation/_site/ TechnicalDocs/unity/