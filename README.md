# ArkApi Plugin Config Editor

### This is config.json editor for Ark Survival Evolved Ark Api Plugins made from WPF (Windows Presentation Foundation) in C# .NET6.0
### Here's the link for the [Ark APi Framework](https://gameservershub.com/forums/resources/ark-server-api.12/)
### Link to download on Game server hub [Ark Api Plugin Config Editor](https://gameservershub.com/forums/resources/arkapi-plugin-configuration-editor.610/)

# Requirements
- Windows only
- ArkApi framework 
- exact location of servers and plugins folder
- config.json of plugin

# Libraries
- Newtonsoft json (Im not really sure if i've used this still confused on System.Text.Json and this one)
- Okii Dialogs (For openning directories)


# Features
- Modify whole cluster plugins
- Include/Exclude maps
- change config.json filename to be modify (other purposes but not recommended)
- plugin selection
- structured treeview of all configuration elements and values
- raw data for adding new elements to the json

### Defaults
#### StartIn (all maps/server directory) sample
```
"C:/ArkSe/Servers/"
```

#### plugin location must be in
```
"C:/ArkSe/Servers/<MAPNAME>/ShooterGame/Binaries/Win64/ArkApi/Plugins/<PLUGINNAME>"
```

#### Config file location
```
"C:/ArkSe/Servers/<MAPNAME>/ShooterGame/Binaries/Win64/ArkApi/Plugins/<PLUGINNAME>/config.json"
```

![Plugin selection](/ss/ss2.png?raw=true)
![Maps](/ss/ss4.png?raw=true)
![Sample](/ss/ss1.png?raw=true)
![Raw data](/ss/ss3.png?raw=true)

