@echo off

echo Stopping Tridion App Pool
c:\windows\system32\inetsrv\appcmd stop apppool /apppool.name:"SDL Tridion"

echo Stopping Services
echo World Wide Web Publishing Service
net stop "w3svc"

echo COM+ System Application
net stop "COMSysApp"

echo System Event Notification Service
net stop "SENS"

echo COM+ Event System
net stop "EventSystem"

echo SDL Web Content Manager Publisher
net stop "TcmPublisher"

echo SDL Web Content Distributor Transport Service
net stop "TCDTransportService"

echo SDL Web Content Manager Batch Processor
net stop "TcmBatchProcessor"

echo SDL Web Content Manager Search Indexer
net stop "TcmSearchIndexer"

echo SDL Web Content Manager Workflow Agent
net stop "TCMWorkflow"

echo SDL Web Content Manager Service Host
net stop "TcmServiceHost"

echo Deploying DLL to deployment folder
xcopy "C:\Users\Content Bloom\source\repos\Tridion.Events\Tridion.Events\bin\Debug\Tridion.Events.PublishMirror.Merged.dll" "C:\Program Files (x86)\SDL Web\bin" /yi
xcopy "C:\Users\Content Bloom\source\repos\Tridion.Events\Tridion.Events\bin\Debug\Tridion.Events.PublishMirror.Merged.pdb" "C:\Program Files (x86)\SDL Web\bin" /yi
xcopy "C:\Users\Content Bloom\source\repos\Tridion.Events\Tridion.Events\bin\Debug\Tridion.Events.config" "C:\Program Files (x86)\SDL Web\bin" /yi
xcopy "C:\Users\Content Bloom\source\repos\Tridion.Events\Tridion.Events\bin\Debug\NLog.config" "C:\Program Files (x86)\SDL Web\bin" /yi

echo Starting Services
net start "TcmServiceHost"
net start "TCMWorkflow"
net start "TcmSearchIndexer"
net start "TcmBatchProcessor"
net start "TCDTransportService"
net start "TcmPublisher"
net start "EventSystem"
net start "SENS"
net start "COMSysApp"
net start "w3svc"

echo Starting Tridion App Pool
c:\windows\system32\inetsrv\appcmd start apppool /apppool.name:"SDL Tridion"

pause