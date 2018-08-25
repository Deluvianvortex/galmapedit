# galmapedit

The Galaxy Map Editor for Legacy 

Version 1

Requirements: .NET 4.5 and a computer capable of running that distro (windows definately, linux/macos probably, not others)

# Quick N Dirty Tutorial

Compile the program an open the executable. 

You will see a gridview on the left. This is the Galaxy Map. Clicking on any of the gridsquares will load that particular sector
into the editor window on the right.

From there, you may see the planetary hierarchy inside the sector in the above treeview. Sectors will always have two branches - Comets and Stars. 

Comets are large pieces of ice and rock that exist beyond the gravity well of a particular star. They exist as objects inside interstellar space. Comets are filtered into the 'Comets' branch and have no child branches of their own.

Stars are large masses of hydrogen and helium that have other, smaller objects in orbit around them. A Star exists as an object in instersellar space. Stars are filtered into the 'Stars' branch and will always have at least one child branch - Planets. (Planets can be 0, but the tree will still exist)

Planets are masses of elements that exist inside the gravity well of a particular star. They exist as objects outside interstellar space. Planets are always bound to their host star, and cannot exist without one. Planets will always have at least one child branch - Moons.

Moons are masses of elements that exist inside the gravity well of a particular planet. They exist as object outside interstellar space. Moons are always bound to their host planet, and cannot exist without one. Moons will generally not have child branches.

<todo> Asteroids are masses of rock and metals that exist pretty much everywhere. They exist both as objects inside and outside interstellar space. Asteroids can be bound to host stars, planets, and moons. Asteroids will never have child branches. </todo>

If you don't see any of these things in the treeview, its because you don't have any information loaded in the sectors. To fix that, go to File -> New -> Random, and select 'No' when it asks to save (unless you want to save a blank galaxy, you can, I'm not your mom).

Then you'll see that information appears in the sectors. You can browse through them at your pleasure.

If you want to change that information, then you're kinda out of luck because I haven't programmed that in yet. I'm not even sure you can see it. But the Treeview does update, so the information is there.

Work continues.


-DV


CHANGELOG:

Version 1: Barebones build so I can test the files in the actual game. The Help menu does nothing. You cannot import from bmp.
There may be other unreported bugs. 

Version 1.1: I found a horrible bug and fixed it so hopefully the thing won't crash on people anymore lol
