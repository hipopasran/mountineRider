You should look RuntimeImport.cs file for simple example of implementation of runtime importing.

How to import svg files in runtime:

1. Use constructor to create SimplySVGImporter: SimplySVGImporter(string svgData, string name) where svgData is the text content of a svg file and name parameter is the name of an object.
2. Import document with SimplySVGImporter.Import() method
3. Build mesh with SimplySVGImporter.Build() method
4. Use results from SimplySVGImporter.mesh and SimplySVGImporter.errors
