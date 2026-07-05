# Gunfish

## Overview

1. I have a gun
1. I have a fish
1. Uh
1. Gunfish

## Fish Prefabs

Fish bodies used to be conjured out of thin air at runtime (physics segments, gun, the works — all assembled fresh every spawn and respawn). Not anymore: every fish is now baked into a real prefab ahead of time, so you can actually see one sitting in a level while you build it, and spawning got a lot cheaper too. If you're tuning a fish and Play mode isn't reflecting your changes, you probably just need to hit the **Garbulate** button on its `GunfishData` asset. See `CLAUDE.md`'s "Fish Body Generation" section for the full story.

## Color Palette

1. Deep Sea Blue: #02478e - This color could be used for the game's background, representing the ocean where the fish are battling.
2. Aquamarine: #7FFFD4 - This could be the color for some of the fish, or to highlight some special powers or weapons.
3. Coral Orange: #FF7F50 - This can be utilized for the contrast to the blue, probably for some of the fish or bullets.
4. Sun Yellow: #FFDF00 - It could be the color of the bullets or other accessories in the game.
5. Neon Green: #39FF14 - Can be used to signify health bars, or power-up items.
6. Bubblegum Pink: #FF69B4 -  For the 'silly' aspect, this could be used as another fish color or for effects like explosions, score pop-ups, etc.
7. Bright Red: #FF0000 - could be used to show damage or decreasing health.