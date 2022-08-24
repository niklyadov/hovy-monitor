set serviceName=HovyMonitor
set fullServiceName=HovyMonitor.Api
set hostedPath=C:\Hosted
set servicePath=C:\Hosted\%serviceName%

net stop %serviceName%

del /y /S %servicePath%\*

dotnet publish -c Release --no-self-contained --output %servicePath% %fullServiceName%.csproj
sc.exe delete %serviceName%
sc.exe create %serviceName% binPath="%servicePath%\%fullServiceName%.exe"

copy /y "%hostedPath%\%serviceName%-config.json" "%servicePath%\appsettings.json"

net start %serviceName%
pause