﻿<?xml version='1.0' encoding='UTF-8'?>
<Project Type="Project" LVVersion="8608001">
	<Item Name="My Computer" Type="My Computer">
		<Property Name="server.app.propertiesEnabled" Type="Bool">true</Property>
		<Property Name="server.control.propertiesEnabled" Type="Bool">true</Property>
		<Property Name="server.tcp.enabled" Type="Bool">false</Property>
		<Property Name="server.tcp.port" Type="Int">0</Property>
		<Property Name="server.tcp.serviceName" Type="Str">My Computer/VI Server</Property>
		<Property Name="server.tcp.serviceName.default" Type="Str">My Computer/VI Server</Property>
		<Property Name="server.vi.callsEnabled" Type="Bool">true</Property>
		<Property Name="server.vi.propertiesEnabled" Type="Bool">true</Property>
		<Property Name="specify.custom.address" Type="Bool">false</Property>
		<Item Name="DCMachineMonitor.vi" Type="VI" URL="../DCMachineMonitor.vi"/>
		<Item Name="DCDriveStatus.vi" Type="VI" URL="../DCDriveStatus.vi"/>
		<Item Name="DCMachineController.vi" Type="VI" URL="../DCMachineController.vi"/>
		<Item Name="ACDriveStatus.vi" Type="VI" URL="../ACDriveStatus.vi"/>
		<Item Name="ACDriveCmdRd.vi" Type="VI" URL="../ACDriveCmdRd.vi"/>
		<Item Name="ACDriveCmdWr.vi" Type="VI" URL="../ACDriveCmdWr.vi"/>
		<Item Name="DCDriveCmdRd.vi" Type="VI" URL="../DCDriveCmdRd.vi"/>
		<Item Name="DCDriveCmdWr.vi" Type="VI" URL="../DCDriveCmdWr.vi"/>
		<Item Name="ACDriveConfig.vi" Type="VI" URL="../ACDriveConfig.vi"/>
		<Item Name="DCDriveConfig.vi" Type="VI" URL="../DCDriveConfig.vi"/>
		<Item Name="Dependencies" Type="Dependencies">
			<Item Name="vi.lib" Type="Folder">
				<Item Name="Simple Error Handler.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Simple Error Handler.vi"/>
				<Item Name="General Error Handler.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/General Error Handler.vi"/>
				<Item Name="General Error Handler CORE.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/General Error Handler CORE.vi"/>
				<Item Name="Check Special Tags.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Check Special Tags.vi"/>
				<Item Name="TagReturnType.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/TagReturnType.ctl"/>
				<Item Name="Set String Value.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Set String Value.vi"/>
				<Item Name="GetRTHostConnectedProp.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/GetRTHostConnectedProp.vi"/>
				<Item Name="Error Code Database.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Error Code Database.vi"/>
				<Item Name="Trim Whitespace.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Trim Whitespace.vi"/>
				<Item Name="whitespace.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/whitespace.ctl"/>
				<Item Name="Format Message String.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Format Message String.vi"/>
				<Item Name="Find Tag.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Find Tag.vi"/>
				<Item Name="Search and Replace Pattern.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Search and Replace Pattern.vi"/>
				<Item Name="Set Bold Text.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Set Bold Text.vi"/>
				<Item Name="Details Display Dialog.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Details Display Dialog.vi"/>
				<Item Name="Clear Errors.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Clear Errors.vi"/>
				<Item Name="DialogTypeEnum.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/DialogTypeEnum.ctl"/>
				<Item Name="ErrWarn.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/ErrWarn.ctl"/>
				<Item Name="eventvkey.ctl" Type="VI" URL="/&lt;vilib&gt;/event_ctls.llb/eventvkey.ctl"/>
				<Item Name="Not Found Dialog.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Not Found Dialog.vi"/>
				<Item Name="Three Button Dialog.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Three Button Dialog.vi"/>
				<Item Name="Three Button Dialog CORE.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Three Button Dialog CORE.vi"/>
				<Item Name="Longest Line Length in Pixels.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Longest Line Length in Pixels.vi"/>
				<Item Name="Convert property node font to graphics font.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Convert property node font to graphics font.vi"/>
				<Item Name="Get Text Rect.vi" Type="VI" URL="/&lt;vilib&gt;/picture/picture.llb/Get Text Rect.vi"/>
				<Item Name="Get String Text Bounds.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/Get String Text Bounds.vi"/>
				<Item Name="LVBoundsTypeDef.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/miscctls.llb/LVBoundsTypeDef.ctl"/>
				<Item Name="BuildHelpPath.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/BuildHelpPath.vi"/>
				<Item Name="GetHelpDir.vi" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/GetHelpDir.vi"/>
				<Item Name="DialogType.ctl" Type="VI" URL="/&lt;vilib&gt;/Utility/error.llb/DialogType.ctl"/>
				<Item Name="MB Ethernet Master Query (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query (poly).vi"/>
				<Item Name="MB Ethernet Master Query Read Coils (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Read Coils (poly).vi"/>
				<Item Name="MB Ethernet Master Query.vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query.vi"/>
				<Item Name="MB Modbus Command to Data Unit.vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Modbus Command to Data Unit.vi"/>
				<Item Name="MB Modbus Data Unit.ctl" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Modbus Data Unit.ctl"/>
				<Item Name="MB Modbus Command.ctl" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Modbus Command.ctl"/>
				<Item Name="MB Ethernet Transmit.vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Transmit.vi"/>
				<Item Name="MB Ethernet Receive.vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Receive.vi"/>
				<Item Name="MB Ethernet String to Modbus Data Unit.vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet String to Modbus Data Unit.vi"/>
				<Item Name="MB Decode Data.vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Decode Data.vi"/>
				<Item Name="MB Ethernet Master Query Read Discrete Inputs (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Read Discrete Inputs (poly).vi"/>
				<Item Name="MB Ethernet Master Query Read Input Registers (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Read Input Registers (poly).vi"/>
				<Item Name="MB Ethernet Master Query Write Multiple Coils (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Write Multiple Coils (poly).vi"/>
				<Item Name="MB Ethernet Master Query Write Multiple Registers (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Write Multiple Registers (poly).vi"/>
				<Item Name="MB Ethernet Master Query Write Single Coil (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Write Single Coil (poly).vi"/>
				<Item Name="MB Ethernet Master Query Write Single Register (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Write Single Register (poly).vi"/>
				<Item Name="MB Ethernet Master Query Read Exception Status (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Read Exception Status (poly).vi"/>
				<Item Name="MB Ethernet Master Query Read Holding Registers (poly).vi" Type="VI" URL="/&lt;vilib&gt;/NI Modbus.llb/MB Ethernet Master Query Read Holding Registers (poly).vi"/>
				<Item Name="subTimeDelay.vi" Type="VI" URL="/&lt;vilib&gt;/express/express execution control/TimeDelayBlock.llb/subTimeDelay.vi"/>
			</Item>
			<Item Name="ACDriveStatus_.vi" Type="VI" URL="../ACDriveStatus_.vi"/>
		</Item>
		<Item Name="Build Specifications" Type="Build"/>
	</Item>
</Project>
