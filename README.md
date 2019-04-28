## Tridion Publish Mirror - Event System Extension

SDL Web 8.5 Event-System extension to mirror publishing in a set of source publications which are defined in a configuration, to a set of target publications. This is particularly useful in combination with Experience Manager, where publishing initiated in XPM can be mirrored to other language websites for consistent user-experiences. 

The extension has been tested in SDL Web 8.5 with both Topology Manager publishing, and legacy publishing using publishing targets. 

### Use cases
There are a bunch of different use cases, but the main use case I had in mind when I built it was for implementations which have a "master" web level which other web publications inherit structure groups and common pages from. Often, users will publish from the "master web" puublication level with the advanced setting "Also publish/unpublish to child publications" to publish the item to all sites inheriting from the master web publication. 

<img src="https://user-images.githubusercontent.com/3137946/56857740-be2e6800-6960-11e9-9622-6419ee31d43c.png" width="500" />

* In the sample blueprint, items in XPM may be edited directly in the English publication (500 EN).
* It may be the case that items being edited in XPM are inherited from the 400 Web level, and should be published to all language sites when edited in XPM.
* This extension can be used to mirror publish / unpublish transactions from the 500 EN publication to the 400 master web publication, forcing the advanced setting to publish to all child publications as well - thus mirroring the publish to all sibling publications. 

## How it works
The extension is an asynchronous event-system extension which hooks into publishing & unpublishing events (transaction committed event phase). 
* When an item is published or published, the extension checks to see if the publish  was initiated in one of the source publications defined in configuration.
* If the transaction should be mirrored in the target publications, the extension initiates a publish or unpublish in each of the target publications.
* The publishing instructions are mirrored exactly.

Configurations exist to also force setting some of the advanced publishing settings, including publishing the minor and in-workflow versions (-v0). See the configuration section below.

## Config
The extension is configured with an accompanying DLL file, which is loaded as an EXE configuration - **Tridion.Events.config**.

- **SourcePublications** - comma-separated list of publication titles for all publications which should initiate publish mirroring
- **TargetPublications** - comma-separated list of publication titles for publications should publishing should be mirrored to
- **PublishLoggingEnabled** - indicate whether or not all publish & unpublish transactions should be logged
- **ForcePublishToChildPublications** - indicate whether the advanced setting should be set to force publishing to all child publications
- **ForcePublishWorkflowVersion** - indicate whether the advanced setting should be set to publish in-workflow versions
- **ForcePublishMinorVersion** - indicate whether the advanced setting should be set to publish minor versions


```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
  
    <!-- Define publications where publishing mirroring should be triggered -->
    <add key="SourcePublications" value="tcm:0-7-1"/>
    
    <!-- Define the publications which publishing  -->
    <add key="TargetPublications" value="400 Example Site"/>

    <!-- Enable logging of all publish transactions -->
    <add key="PublishLoggingEnabled" value="true" />

    <!-- Force the advanced publish / unpublish setting to publish to child publications -->
    <add key="ForcePublishToChildPublications" value="true" />

    <!-- Force the advanced publish setting to publish in-workflow versions -->
    <add key="ForcePublishWorkflowVersion" value="false" />

    <!-- Force the advanced publish setting to publish minor versions -->
    <add key="ForcePublishMinorVersion" value="false" />

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

Please log any defects on the repo issues page.
