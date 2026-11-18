# Community menu link — beta.7

The former achievement-button area now has a text-only **dc.gg/mtou** link. The displayed label has no protocol prefix, icon or extra caption. Its PassiveButton click listener calls the game's standard `Constants.OpenURL` with `https://dc.gg/mtou`. Hover changes the underlined text color. A collider provides the clickable area. Safe-area positioning keeps it in the right/top menu region as the window changes size; a regular sans font preserves the lowercase URL.

Creation is idempotent and the object belongs to the menu hierarchy, so reopening/reinitializing does not add duplicate links or leave a persistent lobby overlay. The existing Discord Server menu button uses the same destination. Achievement removal and the corrected Marmo logo layout from beta.6 remain unchanged.

Release compilation and native UI fixture results are recorded in the preparation directory's audit folder. UI checks cover the exact label/URL, presence of a click event/collider, one link object, text-only presentation and viewport bounds. The fixture does not open the external website or verify whether the invitation is live.

No images were generated or edited, no Discord message was sent, and the installed TEST MODY DLL was not overwritten. Display version remains 1.1 BETA; candidate version is 1.1.0-beta.7.
