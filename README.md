## Avisynth black-box testing app ##

This is a very simple project for black-box testing of avisynth core filters. It doesn't do much useful yet (it hardly even works) but one day it might. 
Primary purpose of this app is to do regression testing during internal filters refactoring/optimization.

### Current goals ###
* Correctness testing: comparing output from two different avisynth versions at runtime.
* Perfrormance testing.
* Ability to test multiple versions of external filters using the same avisynth.dll.
* Ability to test output against some pre-saved image.
* Fancy commandline support - exclude some tests, specify default parameters.
* Rework inter-process communication to something like anonymous pipes maybe?
* Multi-threading.

Don't hold your breath since I'm doing this just for fun.
