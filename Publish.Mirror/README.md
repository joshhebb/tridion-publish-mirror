## Tridion Publish Mirror - Event System Extension

SDL Web 8.5 Event-System extension to mirror publishing in a set of source publications which are defined in a configuration, to a set of target publications. This is particularly useful in combination with Experience Manager, where publishing initiated in XPM can be mirrored to other websites for consistent experiences across language sites. 

The extension has been tested in SDL Web 8.5 with both Topology Manager publishing, and legacy publishing using publishing targets. 

## How it works
The extension is an event-system extension which hooks into publishing & unpublishing events. 
* When an item is published or published, the extension checks to see if the publish  was initiated in one of the source publications defined in configuration.
* If the transaction should be mirrored in the target publications, the extension initiates a publish or unpublish in each of the target publications.
* The publishing instructions are mirrored exactly.

## Config
The extension is configured with an accompanying DLL file, which is loaded as an EXE configuration - **Tridion.Events.config**.

- **SourcePublications** - comma-separated list of publication titles for all publications which should initiate publish mirroring
- **TargetPublications** - comma-separated list of publication titles for publications should publishing should be mirrored to
- **OnlyMirrorIfPublishToChildrenSelected** - indicate whether or not publish mirroring should happen only if the setting 'Also publish/unpublish in child publications' is set to true

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
  
    <!-- Define publications where publishing mirroring should be triggered -->
    <add key="SourcePublications" value="600 Example DE-DE"/>
    
    <!-- Define the publications which publishing  -->
    <add key="TargetPublications" value="400 Example Site,500 Example Site DE"/>

    <!-- If set to true, publish mirroring will only happen if the advanced setting 'Also Publish/Unpublish in Child Publications' is selected -->
    <add key="OnlyMirrorIfPublishToChildrenSelected" value="false" />
    
  </appSettings>
</configuration>
```

## Logging
The logging library used is NLog (v4.6.2) which is configured in NLog.config. 

## Building & Deploying
The extension uses ILMerge (v3.0.29) to merge together the extension DLL which is built with dependencies.

The dependencies utilized include:
* NLog (v4.6.2)

You can either download the latest release and deploy it directly, or download the source and compile the project for yourself.

In order to build & deploy the extension:
* Build the project in Visual Studio
* Copy Tridion.Events.PublishMirror.Merged.dll, Tridion.Events.PublishMirror.Merged.pdb, Tridion.Events.config and NLog.config into %SDL Web Install%/bin
* Open %SDL Web Install%/config/Tridion.ContentManager.config and add the following entry to the <extensions> list:

```xml
<add assemblyFileName="C:\Program Files (x86)\SDL Web\bin\Tridion.Events.PublishMirror.Merged.dll" />
```

### ILMerge
ILMerge is configured in the .csproj file:

```xml
<Target Name="AfterBuild">
  <ItemGroup>
    <MergeAssemblies Include="$(OutputPath)Tridion.Events.PublishMirror.dll" />
    <MergeAssemblies Include="$(OutputPath)NLog.dll" />
  </ItemGroup>
  <PropertyGroup>
    <OutputAssembly>$(OutputPath)Tridion.Events.PublishMirror.Merged.dll</OutputAssembly>
    <Merger Condition="('$(OS)' == 'Windows_NT')">"$(SolutionDir)packages\ILMerge.3.0.29\tools\net452\ILMerge.exe"</Merger>
  </PropertyGroup>
  <Message Text="MERGING: @(MergeAssemblies->'%(Filename)') into $(OutputAssembly)" Importance="High" />
  <Exec Command="$(Merger) /out:&quot;$(OutputAssembly)&quot; @(MergeAssemblies->'&quot;%(FullPath)&quot;', ' ')" />
</Target>
```
