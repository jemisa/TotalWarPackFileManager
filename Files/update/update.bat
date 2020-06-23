set /p schema_version=<..\xmlversion
set /a updated_version=%schema_version%+1

echo updating to %updated_version%

set current_file=schema_%schema_version%.zip
set updated_file=schema_%updated_version%.zip

copy ..\master_schema.xml .
echo %updated_version%> xmlversion
start /wait WinRAR.exe x "%current_file%" "maxVersions_*.xml"
start /wait WinRAR.exe a "%updated_file%" "master_schema.xml" "maxVersions_*.xml" "xmlversion"
del master_schema.xml maxVersions_*.xml

set /p dummy=hit enter