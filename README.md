# What this does

Vintage Story world generation is affected by the order that landforms are defined in JSON.
This is normally fine, as the order of these definitions won't be changing in the vanilla game.
When multiple landform mods interact, however, the order of the JSON patching involved may have an effect
on the order of the resultant landform array, which will affect world generation results.

This mod patches the landform.json loading function to sort the landforms alphabetically by their name,
which should ensure that their order is consistent no matter how many mods are running and what order they
are loaded in.

This has the downside that this mod by itself has an effect on world generation, because it changes the
order of whatever landforms you are running (including the vanilla landforms)
to be alphabetical. It is not a change for the worse or for the better, it's just different, like using a different seed.
Therefore you should not install or remove this mod on an existing world.