# Choose Your Fighter

A mod to allow players to select the tile for their character when they start a
new game.

## Adding custom tiles

### Using tiles from existing objects

If you want to use a tile for your player that isn't listed but is already in
the game, you can use the `Choose tile from blueprint` option and enter the ID
for the creature you want to play as. For instance, if you wanted to play as a
great magma crab, you enter "Great Magma Crab" as your blueprint. Refer to [the
wiki](https://wiki.cavesofqud.com) for a list of IDs for each creature.

### Modding in new tiles

Choose Your Fighter presents a small interface you can use to mod in new tiles.
To do so, add a file called `ChooseYourFighter.xml` specifying the new tiles you
want to add. For instance, the following XML would add a new tile corresponding
to Great Magma Crab:

```xml
<KernelmethodChooseYourFighter>
  <model ID="BigCrabTile" Name="big crab!">
    <tile Path="creatures/sw_magmacrab.png" DetailColor="W" />
  </model>
</KernelmethodChooseYourFighter>
```

This would be listed under `big crab!` in the tile selection menu.

**New in Choose Your Fighter 0.5.0:** several models can also be grouped
together, like so:

```xml
<KernelmethodChooseYourFighter>
  <group ID="EaterStatues" Name="Eater Statues">
    <model ID="EaterStatue1" Name="Eater Statue 1">
      <tile Path="Terrain/sw_ueater1.bmp" Foreground="y" DetailColor="g" />
    </model>

    <model ID="EaterStatue2" Name="Eater Statue 2">
      <tile Path="Terrain/sw_ueater2.bmp" Foreground="y" DetailColor="g" />
    </model>
  </group>
</KernelmethodChooseYourFighter>
```

This will appear as a group called `Eater Statues` in the `Expansions` submenu
of the tile selection menu.

A full example of a `ChooseYourFighter.xml` file (using existing in-game tiles)
can be found in
[examples/ChooseYourFighter.xml](examples/ChooseYourFighter.xml).

### Tips and tricks for modders

- Tiles are sorted alphabetically by ID name. For instance, in the XML below,
  `BigCrabTile` would show up before `WardenUne`, even though the `WardenUne`
  model is listed before `BigCrabModel`.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<KernelmethodChooseYourFighter>
  <model ID="WardenUne" Name="Warden Une">
    <tile Path="Creatures/sw_une.bmp" DetailColor="B" />
  </model>

  <model ID="BigCrabTile" Name="{{R|big crab}}">
    <tile Path="creatures/sw_magmacrab.png" DetailColor="W" />
  </model>
</KernelmethodChooseYourFighter>
```

- You can colorize tile names and group names by using Qud's [color markup
  language](https://wiki.cavesofqud.com/wiki/Modding:Colors_%26_Object_Rendering)
  on "Name" fields. For instance, adding `{{R|...}}`, e.g.
  `Name="{{R|My Tile Name}}"` would color your tile's name red.

- I _highly recommend_ using groups (i.e. the `<group>` tag) if you're planning
  on adding many tiles at once with your mod. This will make it easier for
  players to search for your tiles. Depending on the type of tiles you're
  modding in, you can either group your tiles semantically (e.g.  `"Cat girls"`,
  `"Dog girls"`) or into a single group for your entire mod (e.g.
  `"SuperQudUser420's Cat and Dog Girl Mod"`).

Finally, feel free to ping me (@kernelmethod) for help or feature requests in
the `#modding` channel of the Caves of Qud Discord, or in the `#caves-of-qud`
channel in the Kitfox Discord.
