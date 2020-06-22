# Vagrant-Story-Unity-Parser
Editor Script that read datas from the original Square game Vagrant Story to build prefabs and databases.

Add a Vagrant Story window into Unity where you can import models, maps and other information directly from the original Vagrant Story CD-ROM.

Currently supported formats : 
WEP ++

SHP +

SEQ ++ (from great work of morris : https://github.com/morris/vstools)

ARM ++

ZND +

MPD =

ZUD +

AKAO ++
Akao's file formats is finally relatively well known now, so it's now possible to convert music instruction into a MIDI file, and create a sf2 soundfont to go with.
Here is some examples of re-sampled musics using this tool (and some other fonts to try to get a better sound): https://www.youtube.com/playlist?list=PLLk6oPkqLHuGNvKgo8T2zjSt-O8oo6Eky


There is a dependency for AKAO conversion to WAV, just copy my forked version of Unity-Midi in Assets Folder : https://github.com/korobetski/unity-midi/tree/master/Assets/UnityMidi



Discord channel here : discord.gg/dXvJNVD


SHP view (special case when faces have vertex colors)
<img src="https://github.com/korobetski/Vagrant-Story-Unity-Parser/raw/master/SHP_3A.png"/>

MPD view
<img src="https://github.com/korobetski/Vagrant-Story-Unity-Parser/raw/master/wireframe.png"/>

ARM view
<img src="https://github.com/korobetski/Vagrant-Story-Unity-Parser/raw/master/minimap.png"/>

WEP view
<img src="https://github.com/korobetski/Vagrant-Story-Unity-Parser/raw/master/vs_parser.png"/>
