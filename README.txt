SimpleMIDI by Kevin Cosgrove 

using a C# Priority Queue implementation by Daniel "BlueRaja" Pflughoeft.

C# Library compiled in Mono for writing MIDI files. The MIDIGenerator object
is a collection of Note objects which initilizes as empty.
MIDIGenerator.WriteMIDI() will write a MIDI file containing all the Notes
currently in the collection. See Documentation.txt for details.

Please report bugs to kecosgrove@vassar.edu

To import this dll into your Visual Studio project:
1. Right click on your project and click Add -> Reference.
2. Click Browse.
3. Select SimpleMIDI.dll located in /bin/. Make sure Priority_Queue.dll is
   still in the same folder.
4. Click OK.
5. Add the line "using SimpleMIDI;" to the top of your source file.