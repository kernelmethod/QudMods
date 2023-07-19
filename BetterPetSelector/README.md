# Better Pet Selector

Improves the pet selection menu at the start of a new game, by adding icons and
flavor text for each pet.

Steam Workshop page: https://steamcommunity.com/sharedfiles/filedetails/?id=3006503292

## Information for modders

If you're modding your own pet into the game and wish to leverage this mod's
features (for players who have it installed), you should add one or both of the
following tags to objects in your your `ObjectBlueprints.xml`.

### `Kernelmethod_BetterPetSelector_Description`

**Example:**

```xml
<object Name="Ebenshabat" Load="Merge">
    <tag Name="Kernelmethod_BetterPetSelector_Description" Value="A starapple-loving robot" />
</object>
```

This tag controls the description of the pet that's shown in the pet selection
menu.

### `Kernelmethod_BetterPetSelector_RenderBlueprint`

**Example:**

```xml
<object Name="EitherOrPetSpawner" Load="Merge">
    <tag Name="Kernelmethod_BetterPetSelector_Description" Value="A pair of the player's alternate selves" />
    <tag Name="Kernelmethod_BetterPetSelector_RenderBlueprint" Value="Kernelmethod_BetterPetSelector_EitherOrBlueprint" />
</object>
```

This tag is optional; it's useful in cases where the icon you want to display is
different from the actual tile assigned to the object. In this case, you can
pass in the blueprint for another object, and the icon will be rendered from
that blueprint.

![Picture of the new pet selection menu on starting a new game](assets/menu_full.png)




