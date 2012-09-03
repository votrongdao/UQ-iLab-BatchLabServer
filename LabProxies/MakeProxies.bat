@echo off
set wsdlpath="C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin"
@echo on
%wsdlpath%\wsdl /v /sharetypes /f /n:Library.LabServerEngine /o:./Proxies/ProxyServiceBroker.cs http://localhost:8085/IServiceBrokerService.asmx?WSDL
%wsdlpath%\wsdl /v /sharetypes /f /n:Library.ServiceBroker /o:./Proxies/ProxyLabServer.cs http://localhost:8086/ILabServerWebService.asmx?WSDL
%wsdlpath%\wsdl /v /sharetypes /f /n:Library.LabServerEngine.Drivers.Equipment /o:./Proxies/ProxyEquipmentService.cs http://localhost:8087/IEquipmentService.asmx?WSDL
@echo off
rem
rem %wsdlpath%\wsdl /v /sharetypes /f /n:Library.LabServer /o:ProxyServiceBroker_openilabs.cs http://openilabs.ilab.uq.edu.au/ServiceBroker/Services/ServiceBrokerService.asmx?WSDL
rem %wsdlpath%\wsdl /v /sharetypes /f /n:Library.ServiceBroker /o:ProxyLabServer_TimeOfDayLabServer.cs http://openilabs.ilab.uq.edu.au/TimeOfDayLabServer/LabServerWebService.asmx?WSDL
rem
rem %wsdlpath%\wsdl /v /sharetypes /f /n:Library.ServiceBroker /o:ProxyLabServer_weblab2_mit.cs http://olid.mit.edu/services/weblabservice.asmx?WSDL