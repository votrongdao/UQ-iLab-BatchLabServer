@echo off
set myDatabase=LabServer
if %1.==. goto runScripts
set myDatabase=%1

:runScripts
call SetDbEnv.bat

echo Running %myDatabase% scripts ...
echo Running %myDatabase% scripts ... > %myLogfile%

:cmd1
set mysqlscript=.\LabServerTables.sql
echo Processing %mysqlscript%
echo Processing %mysqlscript% >> %myLogfile%
%mydbcmd% -S %myServer%\%myInstance% -E -d %myDatabase% -i %mysqlscript% -b -o %myTmpfile%
if errorlevel 1 goto error
echo Ok. >> %myTmpfile%
type %myTmpfile% >> %myLogfile%
if errorlevel 1 goto error

:cmd2
set mysqlscript=.\LabServerProcedures.sql
echo Processing %mysqlscript%
echo Processing %mysqlscript% >> %myLogfile%
%mydbcmd% -S %myServer%\%myInstance% -E -d %myDatabase% -i %mysqlscript% -b -o %myTmpfile%
if errorlevel 1 goto error
echo Ok. >> %myTmpfile%
type %myTmpfile% >> %myLogfile%
if errorlevel 1 goto error

:cmd3
set mysqlscript=.\LabServerDefaultValues.sql
echo Processing %mysqlscript%
echo Processing %mysqlscript% >> %myLogfile%
%mydbcmd% -S %myServer%\%myInstance% -E -d %myDatabase% -i %mysqlscript% -b -o %myTmpfile%
if errorlevel 1 goto error
echo Ok. >> %myTmpfile%
type %myTmpfile% >> %myLogfile%
if errorlevel 1 goto error

:ok
echo.
echo OK. See %myLogfile% for details
goto done

:error
echo.
echo *** ERROR! *** See %myLogfile% for details

:done
del %myTmpfile%

call ClearDbEnv.bat
