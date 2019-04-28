## Tridion Publish Mirror - Event System Extension

SDL Web 8.5 Event-System extension to mirror publishing in a set of source publications which are defined in a configuration, to a set of target publications. This is particularly useful in combination with Experience Manager, where publishing initiated in XPM can be mirrored to other language websites for consistent user-experiences. 

_The extension has been tested in SDL Web 8.5 with both Topology Manager publishing, and legacy publishing using publishing targets._

## Extension use cases
The main use case I had in mind when building the extension was for implementations which have a (web) publication in between the content and website publications intended for creating content manager pages and structure groups which are shared across a set of child websites. Often times, these publications will have publishing enabled, even though there is typically no representative website for this publication, in order to allow the users to select the advanced setting "Also publish/unpublish to child publications" which allows them to publish items in a single publish to all web publications at once.

While this was the main use case, there are probably a ton of different scenarios where you would want to mirror all publishing between two publications.

<p align="center">
<img src="https://user-images.githubusercontent.com/3137946/56857740-be2e6800-6960-11e9-9622-6419ee31d43c.png" width="400" />
</p>

_Example blueprint showing the most common use case I had in mind for the extension (described in more detail below)._

The example blueprint above shows one of the primary use cases of the extension:
* Items in XPM may be edited directly in the English publication (500 EN).
* Iitems being edited in XPM are inherited from the 400 Web level, and should be published to all language sites when edited in XPM.
* This extension can be used to mirror publish / unpublish transactions from the 500 EN publication to the 400 master web publication, forcing the advanced setting to publish to all child publications as well - thus mirroring the publish to all sibling publications. 

## How it works
The extension is an asynchronous event-system extension which hooks into publishing & unpublishing events (transaction committed event phase). 
* When an item is published or published, the extension checks to see if the publish  was initiated in one of the source publications defined in the configuration file.
* If the transaction should be mirrored in the target publications, the extension initiates a publish or unpublish in each of the target publications defined in the configuration file.
* The publishing instructions are mirrored exactly, including the publish / unpublish instructions.

Configurations exist to also force setting some of the advanced publishing settings, including publishing the minor and in-workflow versions (-v0). See the configuration section below.

## Configurations
The extension is configured with an accompanying DLL file, which is loaded as an EXE configuration - **Tridion.Events.config**.

- **SourcePublications**: comma-separated list of publication titles or TCM Ids for all publications which should initiate publish mirroring.
- **TargetPublications**: comma-separated list of publication titles or TCM IDs for publications should publishing should be mirrored to.
- **PublishLoggingEnabled**: indicate whether or not all publish & unpublish transactions should be logged.
- **ForcePublishToChildPublications**: indicate whether the advanced setting should be set to force publishing to all child publications.
- **ForcePublishWorkflowVersion**: indicate whether the advanced setting should be set to publish in-workflow versions.
- **ForcePublishMinorVersion**: indicate whether the advanced setting should be set to publish minor versions.


```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
  
    <!-- Define publications by title or TCM ID (comma-separated) where publishing mirroring should be triggered -->
    <add key="SourcePublications" value="tcm:0-7-1"/>
    
    <!-- Define the publications by title or TCM ID (comma-separated) which publishing  -->
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

## Building & Deploying
The extension uses ILMerge (v3.0.29) to merge together the extension DLL which is built with dependencies. Logging can also be enabled to log all publish / unpublish transactions for debugging, which can prove useful for auditing purposes.

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

## Logging
The logging library used is NLog (v4.6.2) which is configured in NLog.config. 

You can read more about NLog over on their [https://github.com/NLog](GitHub page).

## ILMerge
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
