Install:
This step is optional, but highly recommended.

Copy SVGExporter.jsx file to Illustator's scripts directory.

On Windows the path should be somehting like this:
C:\Program Files\Adobe\Adobe Illustrator CC 2015\Presets\en_US\Scripts

On Mac OS X, the directory can be found using Finder:
Applications -> Adobe Illustrator CC 2015 -> Presets -> en_US -> Scripts

If can't find it, refer to Adobes instructions: http://www.adobe.com/devnet/illustrator/scripting.html

NOTE: If Illustrator is running, you may have to close it and then start it again to make the script appear in the File -> Scripts menu.


Use:
0. Start Illutrator and draw something
1.a. If you installed the script, select File -> Scripts -> SVGExporter
1.b. If you didn't install the script, select File -> Scripts -> Other Script... and find the SVGExporter.jsx file
2. Set options and select which artboards and layers you want to export
3. Press export


Options:
Export artboards - Select arboards that you want to include in exports.

Export layers - Select layers that you want to include in exports.
Additionally for your selection you can exclude some artboards and layers by adding '-' character as prefix to it's name.

File prefix and suffix - You can set a prefix and a suffix that will be used when layers are exported.
Example: prefix="MarioGameProject " suffix = " VectorFromLayer" and you have layers named "hat" and "character". Then the exported files will be named "MarioGameProject hat VectorFromLayer" and "MarioGameProject character VectorFromLayer".

Output directory - Select where you want the files to be exported.

Export format - Set whether raster images should be embed in the SVG files. NOTE: The current version of Simply SVG doesn't support embedded images, so don't embed them.

Trim Edges - Should the exporter trim empty space around layers or use arboards edges as limits. Trim edges enabled means that exported images are trimmed and thus smaller.

In the bottom of the window, there is a progress bar and a info label. The Info label shows how many layers will be exported. The exporter may make use of an extra layer and thus it may say for example "will export 2 of 3 layers" even tought you only have 2 layers in your document.


Buttons:
Cancel - Will cancel export
Save and Close - Will save your options and close the exporter window
Export - Will export with selected options

Ingore Warnings can be tapped to make export experience more enjoyable.


Licence:
This script bases on open source version of Multi layer exporter. You can freely modify and use it. Orginal makers are marked in comments. Versions with 'n' prefix are work of NordicEdu Ltd.
