
#Setup:
- Grab the lastest release: https://github.com/PandahutMiku/Schematics/releases
- Either just plop in, or just plop in and edit in your Mysql Configuration

##Configuration:
SchematicsDatabaseInfo:Fill in your database info here and set UseDatabase to true
UseDatabase: Set to true if you want to use your database.
MaxDistanceToLoadSchematic (Default 500): This makes it so you can't aim into space and accidentally spawn in your schematic there, you can disable it by setting it to 0.


##Usage:
LoadSchematic (/ls):  <Name> [Optional: -KeepPos -NoState -KeepHealth -SetOwner -SetGroup, Input any Steamid64 to set owner to it].
Example: 
