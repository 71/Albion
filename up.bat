@ECHO	OFF
REM  	UPDATE NUGET PACKAGE

SET	MSB="C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe"

MKDIR	nuget\lib\portable-net45+netcore45+win8+wp8+MonoAndroid+Xamarin.iOS10+MonoTouch>nul

%MSB%	Albion\Albion.csproj /property:Configuration=Release;OutDir="..\nuget\lib\portable-net45+netcore45+win8+wp8+MonoAndroid+Xamarin.iOS10+MonoTouch"
COPY	Albion.nuspec nuget>nul

NUGET	pack nuget\Albion.nuspec  -OutputDirectory .>nul
REM RMDIR	nuget /S /Q>nul