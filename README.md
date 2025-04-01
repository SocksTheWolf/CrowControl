# CrowControl

Twitch Integration in Celeste without CrowdControl.

## What does this fork change?

This is the same as original CrowControl with the following fixes:

* Update version.
* Update to the current API/SDK.
* Update plugin layout layout.
* Requests properly count up.
* Option: Commands can require unique user, allowing you to prevent the scenario of one user be able to spam `!die` a bunch of times.
* Option: Spawned enemy AI is cleaned up properly on death.
* Fix `!die` to work properly when already dead.
* Exit the application cleanly, when connected.
* Fix high CPU usage (because of inf. loop).
* Fix websocket connection options so they do not crash the application.
* Adds github runners.

I do not play Celeste, I just made these fixes for [ffoxes](https://twitch.tv/ffoxes).

You can download this from the releases tab, so long as the github actions decide to work.
