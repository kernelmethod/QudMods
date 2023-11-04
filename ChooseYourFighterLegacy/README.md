# Choose Your Fighter

A mod to allow players to select the tile for their character when they start a
new game.

## Adding custom tiles

If you want to use a tile for your player that isn't listed but is already in
the game, you can use the `Choose tile from blueprint` option and enter the ID
for the creature you want to play as. For instance, if you wanted to play as a
great magma crab, you enter "Great Magma Crab" as your blueprint. Refer to [the
wiki](https://wiki.cavesofqud.com) for a list of IDs for each creature.

It's also possible to mod in new tiles. To do so, add a file
`ChooseYourFighter.xml` containing the new tiles you want to add. For instance,
the following XML would add a new tile corresponding to Great Magma Crab:

```xml
<KernelmethodChooseYourFighter>
  <model ID="BigCrabTile" Name="{{M|big crab!}}">
    <tile Path="creatures/sw_magmacrab.png" DetailColor="W" />
  </model>
</KernelmethodChooseYourFighter>
```

This would be listed under `big crab!` in the tile selection menu.
