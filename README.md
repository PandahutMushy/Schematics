# Schematics

## Setup:
- Grab the lastest release: https://github.com/PandahutMiku/Schematics/releases
- Either just plop in, or just plop in and edit in your Mysql Configuration

## Configuration:
-SchematicsDatabaseInfo:Fill in your database info here and set UseDatabase to true
-UseDatabase: Set to true if you want to use your database.
-MaxDistanceToLoadSchematic (Default 500): This makes it so you can't aim into space and accidentally spawn in your schematic there, you can disable it by setting it to 0.


## Usage:
-LoadSchematic (/ls):  <Name> [Optional: -KeepPos (Keeps -NoState -KeepHealth -SetOwner -SetGroup, Input any Steamid64 to set owner to it]. 
  -  KeepPos loads the schematic in at the saved position inside of the dat file
  - KeepHealth keeps the saved health
  - SetOwner sets Owner to you
  -SetGroup sets group to you
  -inputting any Steamid64 (76561198138254281) will set the elements to be owned by you.
  
-SaveSchematic (/ss): /SaveSchematic <name> <distance> [Optional Parameters: -Owner  -Group, Input any Steamid64 to only get results from it
  - Owner only grabs elements owned by you
  - Group only grabs elements owned by your group
 - inputting any Steamid64 (76561198138254281) will only grab elements owned by it.
 
## Known bugs
- Specific Steamid64 is buggy and sometimes doesn't work, will look into it tommorow