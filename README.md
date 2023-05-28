# WiitarThing

A program that lets you connect Wii Guitar Hero instruments to a Windows PC wirelessly using a wiimote and Bluetooth.

## Setup

### Installation

1. Download and install [ViGEmBus](https://github.com/ViGEm/ViGEmBus/releases).
2. Download WiitarThing from [the "Releases" tab](https://github.com/TheNathannator/WiitarThing/releases), and extract it into a new folder.

### Connecting your Wiimote

1. Start up WiitarThing, then hit the Sync button in the top-left.
2. Sync your wiimote by pressing either the red sync button underneath the battery cover, or both 1+2 at the same time.
   - Be patient during this step, it may take a few tries.
3. Once your wiimote is synced, close the Sync menu, then hit the Connect button on the entry that appears on the left side of the main menu.

### Calibrating Guitars

Guitars must be calibrated before use, otherwise your tilt or whammy may not work correctly. Calibration can be done at any time by simply following these instructions, there is no specific menu you need to go to for it to work.

1. Lay the guitar flat with the frets facing up and neck pointing left, then press the `1` button on the wiimote.
2. Stand the guitar up with the neck pointing directly up, then press the `2` button on the wiimote.
3. Move your whammy bar all the way down and up a few times.
4. Move the joystick around in a few full circles.

## For Further Assistance

Join the [official Clone Hero server on Discord](https://discordapp.com/invite/Hsn4Cgu) and ask in the `#help-line` channel if you're having trouble setting things up or have any other questions.

## Other Instructions

### Using a Dolphinbar

1. Press the mode button on your Dolphinbar until it goes into mode 4, then sync your wiimote to it.
2. Open WiitarThing. 4 wiimotes will show up on the left, regardless of how many are connected to the Dolphinbar.
3. Click the ID button on each entry until your wiimote vibrates, then click Connect on it.

## Credits

WiitarThing is built upon [WiinUSoft and WiinUPro](https://github.com/KeyPuncher/WiinUPro), but not forked because the changes are too significant and messy. All credit for connecting Wiimotes in general and most of the UI goes to [KeyPuncher](https://github.com/KeyPuncher).

This fork is based on [the original WiitarThing fork](https://github.com/Meowmaritus/WiitarThing) by [Myst/Meowmaritus](https://github.com/Meowmaritus), with the ViGEmBus conversion code done by [MWisBest in their fork/issue](https://github.com/Meowmaritus/WiitarThing/issues/9). [Aida-Enna](https://github.com/Aida-Enna) merged the ViGEmBus code and built releases for it, and now [TheNathannator](https://github.com/TheNathannator) maintains it.
