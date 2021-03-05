start /WAIT Unity.exe -logFile %~dp0\playmodeTestLog.txt -batchmode -runTests -editorTestsResultFile %~dp0\playmodeTestResults.xml -testPlatform playmode
start /WAIT Unity.exe -logFile %~dp0\editmodeTestLog.txt -batchmode -runTests -editorTestsResultFile %~dp0\editmodeTestResults.xml -testPlatform editmode
extent.exe -i playmodeTestResults.xml -o Playmode
extent.exe -i editmodeTestResults.xml -o Editmode