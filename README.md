# CrowControl

Twitch Integration in Celeste without CrowdControl.

## What does this fork change?

This is the same as original CrowControl with the following fixes:

* Update version.
* Update to the current API/SDK.
* Update plugin layout.
* Requests properly count up.
* Only caw when we have a successful spawn of entities.
* Option: Commands can require unique user, allowing you to prevent the scenario of one user be able to spam `!die` a bunch of times.
* Option: Spawned enemy AI is cleaned up properly on death.
* Option: Chat commands can require an `!` or not. Does not apply to bits or channel point redemptions.
  * When this is on, matches are only done if the command is the first thing in the message.
* Option: Allow users to only be able to create one type of spawn at any given time, this works for a few entities.
  * Only the Oshiro, Snowball and Seeker are affected by this setting.
  * This does not apply to bit and channel point based redeems.
  * This makes it so one user cannot have 5 snowballs at any given time. However, five different users can spawn one snowball each.
  * This does work with spawn commands that require multiple votes.
* Fix `!die` to work properly when already dead.
* Exit the application cleanly, when connected.
* Fix high CPU usage (because of inf. loop).
* Fix websocket connection options so they do not crash the application.
* Adds github runners.

I do not play Celeste, I just made these fixes for [ffoxes](https://twitch.tv/ffoxes).

You can download this [from GameBannana](https://gamebanana.com/tools/6741) or via the releases tab, so long as the github actions decide to work.
